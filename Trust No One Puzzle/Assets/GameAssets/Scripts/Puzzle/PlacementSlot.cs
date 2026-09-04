using GameAssets.Scripts.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace GameAssets.Scripts.Puzzle
{
    /// <summary>
    /// Validates whether a carried object belongs here.
    /// Correct placement can unlock drawers via <see cref="DrawerUnlockTrigger"/>.
    /// Wrong placement fires events the hint system can use for deceptive messages.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PlacementSlot : MonoBehaviour, IInteractable
    {
        [Header("Identity")]
        [SerializeField] private string slotId = "slot";
        [Tooltip("ItemId that is considered correct for this slot.")]
        [SerializeField] private string requiredItemId;

        [Header("Snap")]
        [SerializeField] private Transform snapPoint;
        [SerializeField] private bool allowWrongItems = true;
        [SerializeField] private bool lockWhenCorrect = true;

        [Header("Events")]
        public UnityEvent<PlaceableItem> OnCorrectPlacement;
        public UnityEvent<PlaceableItem> OnWrongPlacement;
        public UnityEvent<PlaceableItem> OnItemRemoved;
        public UnityEvent OnSlotSolved;

        public string SlotId => slotId;
        public string RequiredItemId => requiredItemId;
        public Transform SnapPoint => snapPoint != null ? snapPoint : transform;
        public PlaceableItem Occupant { get; private set; }
        public bool IsCorrectlyFilled => Occupant != null && Occupant.ItemId == requiredItemId;
        public bool IsOccupied => Occupant != null;

        public string InteractionPrompt
        {
            get
            {
                var carry = PlayerCarry.Instance;
                if (carry != null && carry.IsCarrying)
                {
                    if (IsOccupied)
                        return "Slot occupied";
                    return $"Press [E] to Place {carry.HeldItem.DisplayName}";
                }

                if (IsOccupied && !(lockWhenCorrect && IsCorrectlyFilled))
                    return $"Press [E] to Take {Occupant.DisplayName}";

                return "";
            }
        }

        public bool CanInteract
        {
            get
            {
                var carry = PlayerCarry.Instance;
                if (carry != null && carry.IsCarrying)
                    return !IsOccupied;
                if (IsOccupied && !(lockWhenCorrect && IsCorrectlyFilled))
                    return true;
                return false;
            }
        }

        public void OnInteract()
        {
            var carry = PlayerCarry.Instance;
            if (carry == null)
                return;

            if (carry.IsCarrying && !IsOccupied)
            {
                TryPlace(carry.TakeHeldItem());
                return;
            }

            if (IsOccupied && !carry.IsCarrying && !(lockWhenCorrect && IsCorrectlyFilled))
            {
                var item = Occupant;
                Occupant = null;
                item.ReleaseFromSlot();
                carry.TryPickUp(item);
                OnItemRemoved?.Invoke(item);
                PuzzleEvents.RaiseSlotChanged(this);
            }
        }

        public bool TryPlace(PlaceableItem item)
        {
            if (item == null || IsOccupied)
                return false;

            Occupant = item;
            item.SnapToSlot(this);

            var correct = item.ItemId == requiredItemId;
            if (correct)
            {
                OnCorrectPlacement?.Invoke(item);
                OnSlotSolved?.Invoke();
                PuzzleEvents.RaiseCorrectPlacement(this, item);
            }
            else
            {
                OnWrongPlacement?.Invoke(item);
                PuzzleEvents.RaiseWrongPlacement(this, item);
            }

            PuzzleEvents.RaiseSlotChanged(this);
            return true;
        }
    }
}
