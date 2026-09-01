using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class SimpleDeliverySystem : MonoBehaviour
{
    [Header("Audio & Effects")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private GameObject pickupEffect;
    [SerializeField] private AudioClip deliverySound;
    [SerializeField] private GameObject deliveryEffect;
    
    [Header("UI References")]
    [SerializeField] private TMP_Text deliveriesText;
    [SerializeField] private TMP_Text packageCounterText;
    
    [Header("Logo References for PackageManager")]
    [SerializeField] private Image logoImage;
    [SerializeField] private Sprite normalLogo;
    [SerializeField] private Sprite pickupLogo;
    
    [Header("Cactuar Assignment - Only for Delivery Zones")]
    [SerializeField] private GameObject myCactuar;
    
    [Header("Win Scene Settings")] // NEW
    [SerializeField] private string winSceneName = "WinScene";
    
    // Static state
    private static int totalDeliveries = 0;
    private static TMP_Text uiTextReference;
    private static bool isPackageManagerInitialized = false;
    
    // Track which cactuars have been PERMANENTLY delivered
    private static HashSet<GameObject> deliveredCactuars = new HashSet<GameObject>();
    
    // Package state
    private bool isActive = true;
    private SpriteRenderer spriteRenderer;
    private Collider2D collider2d;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider2d = GetComponent<Collider2D>();
        
        // Store UI reference
        if (deliveriesText != null && uiTextReference == null)
        {
            uiTextReference = deliveriesText;
            UpdateUI();
        }
        
        // Initialize PackageManager
        if (!isPackageManagerInitialized && packageCounterText != null && logoImage != null && normalLogo != null && pickupLogo != null)
        {
            PackageManager.Initialize(packageCounterText, logoImage, normalLogo, pickupLogo, 2f);
            isPackageManagerInitialized = true;
            Debug.Log("PackageManager initialized!");
        }
        
        // Subscribe to PackageManager events
        PackageManager.OnPackagePickedUp += ShowAllUndeliveredCactuars;
        
        // Delivery zones: hide assigned cactuar at start
        if (gameObject.CompareTag("DeliveryZone") && myCactuar != null)
        {
            if (!deliveredCactuars.Contains(myCactuar))
            {
                myCactuar.SetActive(false);
                Debug.Log($"{gameObject.name}: Hiding cactuar '{myCactuar.name}'");
            }
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        PackageManager.OnPackagePickedUp -= ShowAllUndeliveredCactuars;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        if (gameObject.CompareTag("Package") && isActive)
        {
            PickupPackage();
        }
        else if (gameObject.CompareTag("DeliveryZone"))
        {
            DeliverPackage();
        }
    }
    
    void PickupPackage()
    {
        if (PackageManager.HasPackage())
        {
            Debug.Log("Already carrying a package!");
            return;
        }
        
        // Notify PackageManager (this will trigger the event)
        PackageManager.PickupPackage();
        
        // Play pickup effects
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, Camera.main.transform.position);
        }
        
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }
        
        // Show undelivered cactuars
        ShowAllUndeliveredCactuars();
        
        // Hide this package
        SetPackageActive(false);
        
        // Respawn
        StartCoroutine(RespawnPackage());
        
        Debug.Log("📦 Package picked up! Cactuars should appear.");
    }
    
    // This method is called when ANY package is picked up (via event)
    void ShowAllUndeliveredCactuars()
    {
        Debug.Log("Showing all undelivered cactuars...");
        
        // Find all SimpleDeliverySystem scripts
        SimpleDeliverySystem[] allSystems = FindObjectsOfType<SimpleDeliverySystem>();
        int shownCount = 0;
        
        foreach (SimpleDeliverySystem system in allSystems)
        {
            // Show cactuars from delivery zones that haven't been delivered
            if (system.gameObject.CompareTag("DeliveryZone") && 
                system.myCactuar != null && 
                !deliveredCactuars.Contains(system.myCactuar))
            {
                system.myCactuar.SetActive(true);
                shownCount++;
                Debug.Log($"🌵 Showing: {system.myCactuar.name} for {system.gameObject.name}");
            }
        }
        
        Debug.Log($"Total cactuars shown: {shownCount}");
        
        // If no cactuars were shown, check if they're all delivered
        if (shownCount == 0 && deliveredCactuars.Count >= 5)
        {
            Debug.Log("All cactuars have been delivered!");
        }
    }
    
    void DeliverPackage()
    {
        // Check if player has package (using PackageManager)
        if (!PackageManager.HasPackage())
        {
            Debug.Log("❌ No package to deliver!");
            return;
        }
        
        // Check if this cactuar exists and hasn't been delivered yet
        if (myCactuar != null)
        {
            // PERMANENTLY mark this cactuar as delivered
            deliveredCactuars.Add(myCactuar);
            
            // Hide this cactuar PERMANENTLY
            myCactuar.SetActive(false);
            Debug.Log($"✅ PERMANENTLY delivered to {myCactuar.name}");
        }
        else
        {
            Debug.LogWarning("No cactuar assigned to this delivery zone!");
            return;
        }
        
        // Update delivery count
        totalDeliveries++;
        
        // Notify PackageManager that package was delivered
        PackageManager.DeliverPackage();
        
        // Play delivery effects
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (deliverySound != null)
        {
            audioSource.PlayOneShot(deliverySound);
        }
        
        if (deliveryEffect != null)
        {
            Instantiate(deliveryEffect, transform.position, Quaternion.identity);
        }
        
        // Update UI
        UpdateUI();
        
        Debug.Log($"🎯 Delivery {totalDeliveries}/5 complete! Cactuar is permanently gone.");
        
        // CHECK FOR WIN CONDITION - NEW CODE
        if (totalDeliveries >= 5)
        {
            Debug.Log("🎉 ALL 5 DELIVERIES COMPLETE! Loading win scene...");
            StartCoroutine(LoadWinSceneAfterDelay(2f)); // Wait 2 seconds then show win screen
        }
    }
    
    // NEW METHOD: Load win scene with delay
    IEnumerator LoadWinSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Optional: Save progress before loading win scene
        SaveProgress();
        
        // Load win scene
        if (!string.IsNullOrEmpty(winSceneName))
        {
            // Check if win scene exists
            if (SceneExists(winSceneName))
            {
                SceneManager.LoadScene(winSceneName);
            }
            else
            {
                Debug.LogError($"Win scene '{winSceneName}' not found! Check Build Settings.");
            }
        }
    }
    
    // Helper method to check if scene exists (you may already have this)
    bool SceneExists(string sceneName)
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
    
    // Optional: Method to save progress before loading win scene
    void SaveProgress()
    {
        // Add your save logic here if needed
        Debug.Log("Saving progress before loading win scene...");
        
        // Example: PlayerPrefs saving
        PlayerPrefs.SetInt("TotalDeliveries", totalDeliveries);
        PlayerPrefs.SetInt("GameCompleted", 1);
        PlayerPrefs.Save();
    }
    
    void SetPackageActive(bool active)
    {
        isActive = active;
        if (spriteRenderer != null) spriteRenderer.enabled = active;
        if (collider2d != null) collider2d.enabled = active;
    }
    
    IEnumerator RespawnPackage()
    {
        yield return new WaitForSeconds(3f);
        
        if (!PackageManager.HasPackage())
        {
            SetPackageActive(true);
            Debug.Log("📦 Package respawned!");
        }
    }
    
    void UpdateUI()
    {
        if (uiTextReference != null)
        {
            uiTextReference.text = $"Deliveries: {totalDeliveries}/5";
        }
    }
    
    [ContextMenu("Debug Current State")]
    void DebugState()
    {
        Debug.Log($"=== {gameObject.name} ===");
        Debug.Log($"Tag: {gameObject.tag}");
        Debug.Log($"My Cactuar: {myCactuar?.name ?? "None"}");
        Debug.Log($"Is Active: {isActive}");
        Debug.Log($"Delivered Cactuars: {deliveredCactuars.Count}");
    }
    
    // Static method to manually show cactuars (for testing)
    public static void ShowAllCactuarsForDebug()
    {
        SimpleDeliverySystem[] allSystems = FindObjectsOfType<SimpleDeliverySystem>();
        foreach (SimpleDeliverySystem system in allSystems)
        {
            if (system.gameObject.CompareTag("DeliveryZone") && system.myCactuar != null)
            {
                system.myCactuar.SetActive(true);
                Debug.Log($"Debug: Showing {system.myCactuar.name}");
            }
        }
    }
    
    // Static method to reset game state (useful for testing)
    public static void ResetGameState()
    {
        totalDeliveries = 0;
        deliveredCactuars.Clear();
        if (uiTextReference != null)
        {
            uiTextReference.text = $"Deliveries: 0/5";
        }
    }
}