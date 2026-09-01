using UnityEngine;
using System.Collections;

public class BoostPickup : MonoBehaviour
{
    [Header("Boost Settings")]
    [SerializeField] private float boostMultiplier = 2f; // 2x speed
    [SerializeField] private float boostDuration = 3f;
    [SerializeField] private AudioClip boostSound;
    
    [Header("Respawn Settings")]
    [SerializeField] private float respawnTime = 5f;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject pickupEffect;
    
    // Components
    private SpriteRenderer spriteRenderer;
    private Collider2D pickupCollider;
    private bool isActive = true;
    
    void Start()
    {
        // Get components
        spriteRenderer = GetComponent<SpriteRenderer>();
        pickupCollider = GetComponent<Collider2D>();
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return; // Don't pick up if not active
        
        if (other.CompareTag("Player"))
        {
            MovementRigidbody playerMovement = other.GetComponent<MovementRigidbody>();
            if (playerMovement != null)
            {
                playerMovement.ActivateBoost(boostMultiplier, boostDuration, boostSound);
                
                // Visual effect
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }
                
                // Hide and disable instead of destroying
                DeactivateBoost();
                
                // Start respawn timer
                StartCoroutine(RespawnBoost());
            }
        }
    }
    
    void DeactivateBoost()
    {
        isActive = false;
        
        // Hide the sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        // Disable the collider
        if (pickupCollider != null)
        {
            pickupCollider.enabled = false;
        }
    }
    
    void ActivateBoost()
    {
        isActive = true;
        
        // Show the sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        
        // Enable the collider
        if (pickupCollider != null)
        {
            pickupCollider.enabled = true;
        }
        
        Debug.Log("Boost respawned!");
    }
    
    IEnumerator RespawnBoost()
    {
        // Wait for respawn time
        yield return new WaitForSeconds(respawnTime);
        
        // Reactivate the boost
        ActivateBoost();
    }
    
    // Optional: Add a pulsing effect while active
    void Update()
    {
        if (isActive && spriteRenderer != null)
        {
            // Simple pulse effect (optional)
            float pulse = Mathf.Sin(Time.time * 3f) * 0.2f + 0.8f;
            spriteRenderer.color = new Color(1, 1, 1, pulse);
        }
    }
    
    // Optional: Draw gizmo to show respawn area when selected
    void OnDrawGizmosSelected()
    {
        if (!isActive)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.DrawIcon(transform.position + Vector3.up, "d_Refresh", true);
        }
    }
}