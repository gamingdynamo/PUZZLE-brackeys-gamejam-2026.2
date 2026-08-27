namespace GameAssets.Scripts.Interaction
{
    /// <summary>
    /// Base interface for any object the player can interact with (doors, drawers, toolbox, etc.).
    /// Attach a MonoBehaviour implementing this to any GameObject with a Collider.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// The prompt text shown on the UI when the player looks at this object.
        /// e.g. "Press E to open" / "Press E to pick up"
        /// </summary>
        string InteractionPrompt { get; }

        /// <summary>
        /// Whether the object can currently be interacted with.
        /// Useful for locked items, cooldowns, or one-time interactions.
        /// </summary>
        bool CanInteract { get; }

        /// <summary>
        /// Called once when the player presses the Interact button while looking at this object.
        /// </summary>
        void OnInteract();
    }
}
