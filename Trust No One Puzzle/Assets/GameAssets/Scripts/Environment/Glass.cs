using UnityEngine;

public class Glass : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject brokenWindow; // Assign your shattered prefab here

    [Header("Settings")]
    [SerializeField] private float breakThreshold = 5f; // Minimum impact velocity to break
    
    [Tooltip("Only objects with this tag can break the glass. Leave empty to allow anything fast enough.")]
    [SerializeField] private string requiredTag = "Hammer";

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip breakSound;

    private void OnCollisionEnter(Collision collision)
    {
        // Check tag if required
        if (!string.IsNullOrEmpty(requiredTag) && !collision.gameObject.CompareTag(requiredTag))
        {
            return;
        }

        // Check if the object hit the glass hard enough
        if (collision.relativeVelocity.magnitude >= breakThreshold)
        {
            BreakGlass(collision.contacts[0].point);
        }
    }

    public void BreakGlass(Vector3 impactPoint)
    {
        if (audioSource != null && breakSound != null)
        {
            // Play detached so it isn't cut off when the original glass is destroyed
            AudioSource.PlayClipAtPoint(breakSound, transform.position);
        }

        brokenWindow.SetActive(true);
        
        // brokenWindow.GetComponent<ShatteredGlass>().ApplyExplosion(impactPoint);

        // Destroy the unbroken glass panel
        Destroy(gameObject);
    }
}
