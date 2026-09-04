using GameAssets.Scripts.Interaction;
using UnityEngine;

namespace GameAssets.Scripts.Puzzle
{
    /// <summary>
    /// Pickup that can be carried, shoved in tight spaces, and dropped into a <see cref="PlacementSlot"/>.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PlaceableItem : MonoBehaviour, IInteractable
    {
        public enum CarryStyle
        {
            /// <summary>Small items: parented to the hold point, rotate with the camera/player.</summary>
            Handheld,
            /// <summary>Keep world rotation while following the hold position (crates, furniture).</summary>
            WorldStable,
            /// <summary>World-stable plus scroll distance and MMB / HUD spatial shoving.</summary>
            Spatial
        }

        [SerializeField] private string itemId = "item";
        [SerializeField] private string displayName = "Object";
        [SerializeField] private CarryStyle carryStyle = CarryStyle.Handheld;
        [SerializeField] private Rigidbody body;
        [SerializeField] private Collider[] colliders;

        [Header("Spatial (boxes)")]
        [SerializeField] private float defaultHoldDistance = 1.6f;
        [SerializeField] private float minHoldDistance = 0.6f;
        [SerializeField] private float maxHoldDistance = 4f;

        public string ItemId => itemId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? gameObject.name : displayName;
        public CarryStyle Style => carryStyle;
        public Rigidbody Body => body;
        public float DefaultHoldDistance => defaultHoldDistance;
        public float MinHoldDistance => minHoldDistance;
        public float MaxHoldDistance => maxHoldDistance;
        public bool UsesSpatialCarry => carryStyle == CarryStyle.Spatial;
        public bool RotateWithHolder => carryStyle == CarryStyle.Handheld;

        public bool IsHeld { get; private set; }
        public PlacementSlot OccupyingSlot { get; private set; }

        public string InteractionPrompt => IsHeld ? "" : $"Press [E] to Pick Up {DisplayName}";
        public bool CanInteract => !IsHeld && OccupyingSlot == null;

        private void Awake()
        {
            if (body == null)
                body = GetComponent<Rigidbody>();
            if (colliders == null || colliders.Length == 0)
                colliders = GetComponentsInChildren<Collider>();
        }

        public void OnInteract()
        {
            var carrier = PlayerCarry.Instance;
            if (carrier != null)
                carrier.TryPickUp(this);
        }

        public void SetHeld(bool held, Transform holdPoint)
        {
            IsHeld = held;
            OccupyingSlot = null;

            var hideColliders = held && carryStyle == CarryStyle.Handheld;
            foreach (var c in colliders)
            {
                if (c != null)
                    c.enabled = !hideColliders;
            }

            if (body != null)
            {
                body.isKinematic = held;
                body.useGravity = !held;
                body.detectCollisions = true;
            }

            if (held && holdPoint != null && carryStyle == CarryStyle.Handheld)
            {
                transform.SetParent(holdPoint);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
            else
            {
                transform.SetParent(null);
            }
        }

        public void SnapToSlot(PlacementSlot slot)
        {
            OccupyingSlot = slot;
            IsHeld = false;

            foreach (var c in colliders)
            {
                if (c != null)
                    c.enabled = false;
            }

            if (body != null)
            {
                body.isKinematic = true;
                body.useGravity = false;
            }

            transform.SetParent(slot.SnapPoint);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        public void ReleaseFromSlot()
        {
            OccupyingSlot = null;
            foreach (var c in colliders)
            {
                if (c != null)
                    c.enabled = true;
            }

            if (body != null)
            {
                body.isKinematic = false;
                body.useGravity = true;
            }

            transform.SetParent(null);
        }
    }
}
