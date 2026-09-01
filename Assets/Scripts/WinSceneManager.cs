using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class WinSceneManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text winText;
    [SerializeField] private TMP_Text messageText;
    
    [Header("Button References")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private Button exitButton;
    
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "SampleScene";
    [SerializeField] private string menuSceneName = "MainMenuScene";
    
    [Header("Audio & Effects")]
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private ParticleSystem confettiEffect;
    
    private AudioSource audioSource;
    
    void Start()
    {
        // FIX: Ensure AudioListener exists in scene
        EnsureAudioListenerExists();
        
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) 
            audioSource = gameObject.AddComponent<AudioSource>();
        
        // Configure AudioSource
        audioSource.playOnAwake = false;
        audioSource.volume = 1f;
        audioSource.spatialBlend = 0f; // 2D sound
        
        // Play victory sound
        if (victorySound != null)
        {
            Debug.Log("Playing victory sound...");
            audioSource.PlayOneShot(victorySound);
        }
        else
        {
            Debug.LogWarning("Victory sound clip is not assigned in Inspector!");
        }
        
        // Play confetti effect
        if (confettiEffect != null)
        {
            confettiEffect.Play();
        }
        
        // Setup buttons
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
        
        if (menuButton != null)
            menuButton.onClick.AddListener(OnMenuClicked);
        
        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitClicked);
        
        // Display stats
        DisplayStats();
        
        // Add entrance animation
        StartCoroutine(EntranceAnimation());
    }
    
    // NEW METHOD: Ensure AudioListener exists
    void EnsureAudioListenerExists()
    {
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        
        if (listeners.Length == 0)
        {
            Debug.Log("No AudioListener found. Adding one...");
            
            // Try to find main camera
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Camera[] cameras = FindObjectsOfType<Camera>();
                if (cameras.Length > 0)
                {
                    mainCamera = cameras[0];
                }
            }
            
            // Add to existing camera
            if (mainCamera != null)
            {
                AudioListener listener = mainCamera.GetComponent<AudioListener>();
                if (listener == null)
                {
                    mainCamera.gameObject.AddComponent<AudioListener>();
                    Debug.Log($"Added AudioListener to {mainCamera.gameObject.name}");
                }
            }
            else
            {
                // Create new camera if none exists
                GameObject newCamera = new GameObject("Audio Camera");
                newCamera.AddComponent<Camera>();
                newCamera.AddComponent<AudioListener>();
                newCamera.tag = "MainCamera";
                Debug.Log("Created new camera with AudioListener");
            }
        }
        else
        {
            Debug.Log($"Found {listeners.Length} AudioListener(s)");
        }
    }
    
    void OnRestartClicked()
    {
        PlayButtonSound();
        Debug.Log("Restarting game...");
        SceneManager.LoadScene(gameSceneName);
    }
    
    void OnMenuClicked()
    {
        PlayButtonSound();
        Debug.Log("Returning to main menu...");
        SceneManager.LoadScene(menuSceneName);
    }
    
    void OnExitClicked()
    {
        PlayButtonSound();
        Debug.Log("Exiting game...");
        Invoke(nameof(QuitGame), 0.3f);
    }
    
    void DisplayStats()
    {
        if (messageText != null)
        {
            messageText.text = "All 5 Packages Successfully Delivered!";
        }
    }
    
    IEnumerator EntranceAnimation()
    {
        Transform winBanner = transform.Find("WinBanner");
        if (winBanner != null)
        {
            Vector3 originalScale = winBanner.localScale;
            winBanner.localScale = Vector3.zero;
            
            float duration = 0.5f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                winBanner.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            winBanner.localScale = originalScale;
        }
    }
    
    void PlayButtonSound()
    {
        if (buttonClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
    
    void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Space))
        {
            OnRestartClicked();
        }
        
        if (Input.GetKeyDown(KeyCode.M))
        {
            OnMenuClicked();
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnExitClicked();
        }
        
        // Debug: Test sound with T key
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (victorySound != null)
            {
                AudioSource.PlayClipAtPoint(victorySound, Camera.main.transform.position);
                Debug.Log("Test sound played");
            }
        }
    }
}