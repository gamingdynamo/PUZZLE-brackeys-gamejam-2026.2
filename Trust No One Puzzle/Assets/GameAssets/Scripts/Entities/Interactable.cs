using UnityEngine;

namespace GameAssets.Scripts.Entities
{
    /// <summary>
    /// Marks an object as a valid target for the first-person interaction ray.
    /// Add this to the root of every object that can be picked up or interacted with.
    /// </summary>
    public class Interactable : MonoBehaviour
    {
        [SerializeField] private string displayName;

        [Header("Audio")]
        [Tooltip("Sound played when this object is picked up.")]
        public AudioClip pickupSound;
        
        [Tooltip("Sound played when this object is dropped.")]
        public AudioClip dropSound;

        [Tooltip("Sound played when this object is thrown.")]
        public AudioClip throwSound;

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? gameObject.name : displayName;
    }
}
