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

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? gameObject.name : displayName;
    }
}
