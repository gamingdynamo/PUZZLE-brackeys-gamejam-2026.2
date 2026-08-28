using UnityEngine;
using UnityEngine.Events;

namespace GameAssets.Scripts.Interaction
{
    /// <summary>
    /// Handles the pickup logic for the Hammer.
    /// Tracks globally whether the player has picked it up.
    /// </summary>
    public class HammerInteractable : MonoBehaviour, IInteractable
    {
        public static bool HasHammer { get; private set; } = false;

        [Header("Audio")]
        [Tooltip("AudioSource to play the pickup sound.")]
        [SerializeField] private AudioSource audioSource;
        
        [Tooltip("Sound played when the hammer is picked up.")]
        [SerializeField] private AudioClip pickupSound;

        [Header("Events")]
        [Tooltip("Fired when the hammer is successfully picked up.")]
        public UnityEvent OnHammerPickedUp;

        // IInteractable Implementation
        public string InteractionPrompt => "Press [E] to Pick Up Hammer";
        
        public bool CanInteract => gameObject.activeInHierarchy;

        public void OnInteract()
        {
            HasHammer = true;
            Debug.Log("Hammer picked up!");
            
            if (audioSource != null && pickupSound != null)
            {
                // Play sound detached so it doesn't get cut off when the object is disabled
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            OnHammerPickedUp?.Invoke();

            // Disable the hammer in the scene once picked up
            gameObject.SetActive(false);
        }

        // Reset static state when destroyed/level restarted
        private void OnDestroy()
        {
            HasHammer = false;
        }
    }
}
