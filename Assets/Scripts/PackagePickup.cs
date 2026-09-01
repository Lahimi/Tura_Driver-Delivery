using UnityEngine;

public class PackagePickup : MonoBehaviour
{
    [Header("Package Settings")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private GameObject pickupEffect;
    [SerializeField] private float soundVolume = 1f;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        // Check if player can pick up (using PackageManager)
        if (PackageManager.HasPackage())
        {
            Debug.Log("Already carrying a package!");
            return;
        }
        
        Pickup();
    }
    
    void Pickup()
    {
        // Notify PackageManager
        PackageManager.PickupPackage();
        
        // Play sound
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, Camera.main.transform.position, soundVolume);
        }
        
        // Play visual effect
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }
        
        // Destroy this package (it will respawn via SimpleDeliverySystem)
        Destroy(gameObject);
        
        Debug.Log("Package picked up!");
    }
}