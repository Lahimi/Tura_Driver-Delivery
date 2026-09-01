using UnityEngine;
using System.Collections;

public class MovementRigidbody : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Transform spriteTransform;
    [SerializeField] private Animator animator;
    
    [Header("Movement Settings")]
    [SerializeField] private float baseMoveSpeed = 15f;
    private float currentSpeed;
    
    [Header("Boost Settings")]
    [SerializeField] private float boostMultiplier = 2f;
    [SerializeField] private float boostDuration = 3f;
    [SerializeField] private string sprintAnimationBool = "isSprinting";
    [SerializeField] private AudioSource audioSource;
    
    [Header("Collision Effects")]
    [SerializeField] private float bounceForce = 50f; // Increased from 10f
    [SerializeField] private float bounceDuration = 0.3f; // NEW: How long bounce lasts
    [SerializeField] private float slowDuration = 3f;
    [SerializeField] private float slowMultiplier = 0.5f;
    [SerializeField] private LayerMask obstacleLayer;
    
    private bool isFacingRight = true;
    private float movementThreshold = 0.1f;
    
    // State tracking
    private bool isSlowed = false;
    private bool isBoosted = false;
    private bool isBouncing = false; // NEW: Track bounce state
    private Coroutine slowCoroutine;
    private Coroutine boostCoroutine;
    private Coroutine bounceCoroutine; // NEW: Bounce timer
    private Color originalColor;
    
    void Start()
    {
        if (spriteTransform == null) spriteTransform = transform;
        if (animator == null)
        {
            animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
        }
        
        currentSpeed = baseMoveSpeed;
        rb.freezeRotation = true;
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
        
        SpriteRenderer sr = spriteTransform.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            originalColor = sr.color;
        }
    }

    void Update()
    {
        HandleAnimation();
        HandleSpriteFlip();
    }
    
    void FixedUpdate()
    {
        // Only apply movement if NOT bouncing
        if (!isBouncing)
        {
            HandleMovement();
        }
    }
    
    private void HandleMovement()
    {
        Vector2 velocity = inputManager.moveInput * currentSpeed;
        rb.linearVelocity = velocity;
    }
    
    private void HandleAnimation()
    {
        float horizontalInput = inputManager.moveInput.x;
        float verticalInput = inputManager.moveInput.y;
        
        bool isMovingHorizontally = Mathf.Abs(horizontalInput) > movementThreshold;
        bool isMovingUp = verticalInput > movementThreshold;
        bool isMovingDown = verticalInput < -movementThreshold;
        
        if (isMovingUp || isMovingDown)
        {
            animator.SetBool("isRunning", false);
            animator.SetBool("isRunningUp", isMovingUp);
            animator.SetBool("isRunningDown", isMovingDown);
        }
        else
        {
            animator.SetBool("isRunning", isMovingHorizontally);
            animator.SetBool("isRunningUp", false);
            animator.SetBool("isRunningDown", false);
        }
        
        if (animator != null)
        {
            animator.SetBool(sprintAnimationBool, isBoosted);
        }
        
        if (isSlowed)
        {
            animator.speed = slowMultiplier;
        }
        else if (isBoosted)
        {
            animator.speed = boostMultiplier;
        }
        else
        {
            animator.speed = 1f;
        }
    }
    
    public void ActivateBoost(float multiplier = 2f, float duration = 3f, AudioClip boostSound = null)
    {
        if (boostCoroutine != null)
        {
            StopCoroutine(boostCoroutine);
        }
        
        if (isSlowed && slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
            isSlowed = false;
            ApplySlowVisuals(false);
        }
        
        if (boostSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(boostSound);
        }
        
        boostCoroutine = StartCoroutine(BoostRoutine(multiplier, duration));
    }
    
    IEnumerator BoostRoutine(float multiplier, float duration)
    {
        isBoosted = true;
        currentSpeed = baseMoveSpeed * multiplier;
        
        ApplyBoostVisuals(true);
        
        Debug.Log($"Boost activated! Speed: {currentSpeed}");
        
        yield return new WaitForSeconds(duration);
        
        isBoosted = false;
        currentSpeed = baseMoveSpeed;
        ApplyBoostVisuals(false);
        
        Debug.Log("Boost ended");
        
        boostCoroutine = null;
    }
    
    void ApplyBoostVisuals(bool isBoosting)
    {
        SpriteRenderer sr = spriteTransform.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = isBoosting ? 
                Color.Lerp(originalColor, Color.yellow, 0.5f) : 
                originalColor;
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Collision with: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");
        
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("Obstacle tag matched! Applying bounce.");
            ApplyCollisionEffects(collision);
        }
        else if (obstacleLayer == (obstacleLayer | (1 << collision.gameObject.layer)))
        {
            Debug.Log("Obstacle layer matched! Applying bounce.");
            ApplyCollisionEffects(collision);
        }
    }
    
    void ApplyCollisionEffects(Collision2D collision)
    {
        Debug.Log("ApplyCollisionEffects called");
        
        // Cancel boost if active
        if (isBoosted && boostCoroutine != null)
        {
            StopCoroutine(boostCoroutine);
            isBoosted = false;
            currentSpeed = baseMoveSpeed;
            ApplyBoostVisuals(false);
            Debug.Log("Boost cancelled due to collision!");
        }
        
        ApplyBounceEffect(collision);
        ApplySlowEffect();
    }
    
    void ApplyBounceEffect(Collision2D collision)
    {
        // Stop any existing bounce
        if (bounceCoroutine != null)
        {
            StopCoroutine(bounceCoroutine);
        }
        
        // Calculate bounce direction
        Vector2 bounceDirection = Vector2.zero;
        
        if (collision.contacts.Length > 0)
        {
            // Use collision normal for accurate bounce
            bounceDirection = collision.contacts[0].normal.normalized;
            Debug.Log($"Bounce normal: {bounceDirection}");
        }
        else
        {
            // Fallback: bounce away from obstacle center
            bounceDirection = (transform.position - collision.transform.position).normalized;
            Debug.Log($"Fallback bounce direction: {bounceDirection}");
        }
        
        // Apply bounce force
        isBouncing = true;
        rb.linearVelocity = Vector2.zero; // Clear current velocity
        rb.AddForce(bounceDirection * bounceForce, ForceMode2D.Impulse);
        Debug.Log($"Applied bounce force: {bounceDirection * bounceForce}");
        
        // Start bounce timer
        bounceCoroutine = StartCoroutine(BounceDuration());
    }
    
    IEnumerator BounceDuration()
    {
        yield return new WaitForSeconds(bounceDuration);
        
        // End bounce
        isBouncing = false;
        Debug.Log("Bounce ended");
        
        bounceCoroutine = null;
    }
    
    void ApplySlowEffect()
    {
        if (isSlowed && slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
        }
        
        isSlowed = true;
        currentSpeed = baseMoveSpeed * slowMultiplier;
        ApplySlowVisuals(true);
        slowCoroutine = StartCoroutine(SlowEffectCoroutine());
        
        Debug.Log("Slow effect applied");
    }
    
    IEnumerator SlowEffectCoroutine()
    {
        yield return new WaitForSeconds(slowDuration);
        
        isSlowed = false;
        currentSpeed = baseMoveSpeed;
        ApplySlowVisuals(false);
        
        Debug.Log("Slow effect ended");
    }
    
    void ApplySlowVisuals(bool isSlowing)
    {
        SpriteRenderer sr = spriteTransform.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = isSlowing ? 
                Color.Lerp(originalColor, Color.blue, 0.3f) : 
                originalColor;
        }
    }
    
    private void HandleSpriteFlip()
    {
        float horizontalInput = inputManager.moveInput.x;
        
        if (Mathf.Abs(horizontalInput) > movementThreshold)
        {
            if (horizontalInput > 0 && isFacingRight)
            {
                Flip();
            }
            else if (horizontalInput < 0 && !isFacingRight)
            {
                Flip();
            }
        }
    }
    
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 currentScale = spriteTransform.localScale;
        currentScale.x *= -1;
        spriteTransform.localScale = currentScale;
    }
    
    public bool IsBoosted() => isBoosted;
    public bool IsSlowed() => isSlowed;
    public float GetCurrentSpeed() => currentSpeed;
}