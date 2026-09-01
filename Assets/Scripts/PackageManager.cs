using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public static class PackageManager
{
    // Events for package state changes
    public delegate void PackageAction();
    public static event PackageAction OnPackagePickedUp;
    public static event PackageAction OnPackageDelivered;
    
    // Package state
    private static bool hasPackage = false;
    private static TMP_Text packageCounterText;
    private static Image logoImage;
    private static Sprite normalLogo;
    private static Sprite pickupLogo;
    private static float logoChangeDuration = 2f;
    
    // Logo change coroutine
    private static Coroutine logoChangeCoroutine;
    
    // Setup
    public static void Initialize(TMP_Text counterText, Image logoImg, Sprite normal, Sprite pickup, float duration = 2f)
    {
        packageCounterText = counterText;
        logoImage = logoImg;
        normalLogo = normal;
        pickupLogo = pickup;
        logoChangeDuration = duration;
        
        // Set initial logo
        if (logoImage != null && normalLogo != null)
        {
            logoImage.sprite = normalLogo;
        }
        
        UpdateUI();
    }
    
    // Pick up package
    public static void PickupPackage()
    {
        if (hasPackage) return;
        
        hasPackage = true;
        UpdateUI();
        
        // Trigger logo change
        StartLogoChange();
        
        // Trigger event
        OnPackagePickedUp?.Invoke();
        
        Debug.Log("PackageManager: Package picked up!");
    }
    
    // Deliver package
    public static void DeliverPackage()
    {
        if (!hasPackage) return;
        
        hasPackage = false;
        UpdateUI();
        
        // Notify delivery
        OnPackageDelivered?.Invoke();
        
        Debug.Log("PackageManager: Package delivered!");
    }
    
    // Update UI
    static void UpdateUI()
    {
        if (packageCounterText != null)
        {
            packageCounterText.text = $"Packages: {(hasPackage ? "1" : "0")}/1";
        }
    }
    
    // Logo change
    static void StartLogoChange()
    {
        if (logoImage == null || pickupLogo == null || normalLogo == null)
        {
            Debug.LogWarning("PackageManager: Missing logo references!");
            return;
        }
        
        // Get or create MonoBehaviour on logoImage
        MonoBehaviour behaviour = logoImage.GetComponent<MonoBehaviour>();
        if (behaviour == null)
        {
            behaviour = logoImage.gameObject.AddComponent<EmptyMonoBehaviour>();
        }
        
        // Stop existing coroutine
        if (logoChangeCoroutine != null)
        {
            behaviour.StopCoroutine(logoChangeCoroutine);
        }
        
        // Start new coroutine
        logoChangeCoroutine = behaviour.StartCoroutine(LogoChangeRoutine());
    }
    
    static IEnumerator LogoChangeRoutine()
    {
        if (logoImage == null || pickupLogo == null || normalLogo == null)
        {
            Debug.LogError("PackageManager: Missing logo references in coroutine!");
            yield break;
        }
        
        Debug.Log("PackageManager: Changing logo to pickup version");
        
        // Change to pickup logo
        logoImage.sprite = pickupLogo;
        
        // Wait for duration
        yield return new WaitForSeconds(logoChangeDuration);
        
        // Change back to normal logo
        logoImage.sprite = normalLogo;
        
        Debug.Log("PackageManager: Logo changed back to normal");
        
        logoChangeCoroutine = null;
    }
    
    // Check state
    public static bool HasPackage() => hasPackage;
}

// Helper class for coroutines
public class EmptyMonoBehaviour : MonoBehaviour { }