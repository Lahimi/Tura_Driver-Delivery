using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject creditsPanel;
    
    [Header("Button References")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button backButton;
    
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "SampleScene";
    
    [Header("Audio")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip menuMusic;
    private AudioSource audioSource;
    
    void Awake()
    {
        // FIX: Ensure AudioListener exists BEFORE trying to play any audio
        EnsureAudioListenerExists();
    }
    
    void Start()
    {
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) 
            audioSource = gameObject.AddComponent<AudioSource>();
        
        // Configure AudioSource
        audioSource.playOnAwake = true;  // Enable Play on Awake
        audioSource.loop = true;
        audioSource.volume = 0.5f;
        audioSource.spatialBlend = 0f;   // 2D sound
        
        // Assign and play music
        if (menuMusic != null)
        {
            audioSource.clip = menuMusic;
            // No need to call Play() if playOnAwake is true
        }
        else
        {
            Debug.LogError("Menu music is not assigned in Inspector!");
        }
        
        // Setup buttons
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);
        
        if (creditsButton != null)
            creditsButton.onClick.AddListener(OnCreditsClicked);
        
        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitClicked);
        
        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);
        
        // Show main menu, hide credits
        ShowMainMenu();
    }
    
    // NEW: This method ensures an AudioListener always exists
    void EnsureAudioListenerExists()
    {
        // Check for any AudioListeners in the scene
        AudioListener[] listeners = FindObjectsOfType<AudioListener>(true); // Include inactive
        
        Debug.Log($"Found {listeners.Length} AudioListener(s) in scene");
        
        if (listeners.Length == 0)
        {
            Debug.LogWarning("❌ NO AUDIO LISTENER FOUND! Creating one...");
            
            // Try to find the main camera
            Camera mainCamera = Camera.main;
            
            // If no main camera, look for any camera
            if (mainCamera == null)
            {
                Camera[] allCameras = FindObjectsOfType<Camera>(true);
                if (allCameras.Length > 0)
                {
                    mainCamera = allCameras[0];
                    Debug.Log($"Using camera: {mainCamera.gameObject.name}");
                }
            }
            
            // Add AudioListener to existing camera
            if (mainCamera != null)
            {
                AudioListener listener = mainCamera.GetComponent<AudioListener>();
                if (listener == null)
                {
                    listener = mainCamera.gameObject.AddComponent<AudioListener>();
                    Debug.Log($"✅ Added AudioListener to {mainCamera.gameObject.name}");
                }
                else
                {
                    Debug.Log($"✅ AudioListener already exists on {mainCamera.gameObject.name}");
                }
                
                // Make sure it's enabled
                listener.enabled = true;
            }
            else
            {
                // Create a new camera if none exists
                Debug.LogWarning("No camera found! Creating one with AudioListener...");
                CreateCameraWithAudioListener();
            }
        }
        else
        {
            // Make sure at least one listener is enabled
            foreach (AudioListener listener in listeners)
            {
                if (listener.enabled)
                {
                    Debug.Log($"✅ Active AudioListener found on {listener.gameObject.name}");
                    return;
                }
            }
            
            // If we get here, all listeners are disabled
            Debug.LogWarning("All AudioListeners are disabled! Enabling the first one...");
            listeners[0].enabled = true;
        }
    }
    
    void CreateCameraWithAudioListener()
    {
        GameObject newCamera = new GameObject("Main Menu Camera");
        Camera cameraComponent = newCamera.AddComponent<Camera>();
        
        // Set up camera for 2D UI
        cameraComponent.orthographic = true;
        cameraComponent.orthographicSize = 5f;
        cameraComponent.clearFlags = CameraClearFlags.SolidColor;
        cameraComponent.backgroundColor = Color.black;
        
        // Add AudioListener
        newCamera.AddComponent<AudioListener>();
        
        // Set as main camera
        newCamera.tag = "MainCamera";
        
        // Position it properly
        newCamera.transform.position = new Vector3(0, 0, -10);
        
        Debug.Log("✅ Created new camera with AudioListener");
    }
    
    void OnPlayClicked()
    {
        PlayButtonSound();
        Debug.Log("Loading game scene: " + gameSceneName);
        
        if (DoesSceneExist(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError($"Scene '{gameSceneName}' not found in build settings!");
        }
    }
    
    void OnCreditsClicked()
    {
        PlayButtonSound();
        ShowCredits();
    }
    
    void OnExitClicked()
    {
        PlayButtonSound();
        Debug.Log("Exiting game...");
        Invoke(nameof(QuitGame), 0.3f);
    }
    
    void OnBackClicked()
    {
        PlayButtonSound();
        ShowMainMenu();
    }
    
    void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }
    
    void ShowCredits()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }
    
    void PlayButtonSound()
    {
        if (buttonClickSound != null && audioSource != null)
        {
            // Use PlayOneShot so it doesn't interrupt music
            audioSource.PlayOneShot(buttonClickSound, 0.7f);
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
    
    bool DoesSceneExist(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            
            if (sceneNameInBuild == sceneName)
            {
                return true;
            }
        }
        return false;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && creditsPanel != null && creditsPanel.activeSelf)
        {
            OnBackClicked();
        }
        
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            OnPlayClicked();
        }
        
        // Debug: Test AudioListener
        if (Input.GetKeyDown(KeyCode.L))
        {
            CheckAudioListenerStatus();
        }
    }
    
    void CheckAudioListenerStatus()
    {
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        Debug.Log($"=== AudioListener Check ===");
        Debug.Log($"Total Listeners: {listeners.Length}");
        
        foreach (var listener in listeners)
        {
            Debug.Log($"- {listener.gameObject.name}: Enabled={listener.enabled}, Active={listener.gameObject.activeInHierarchy}");
        }
        
        // Test sound
        if (audioSource != null)
        {
            Debug.Log($"AudioSource: IsPlaying={audioSource.isPlaying}, Volume={audioSource.volume}");
        }
    }
}