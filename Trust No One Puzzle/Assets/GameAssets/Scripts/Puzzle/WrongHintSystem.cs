using System.Collections.Generic;
using UnityEngine;

namespace GameAssets.Scripts.Puzzle
{
    /// <summary>
    /// Phone / narrator hints. Some are true, some are deliberately wrong
    /// ("Trust No One"). Wrong placements and locked drawers can trigger
    /// extra misleading messages.
    /// </summary>
    public class WrongHintSystem : MonoBehaviour
    {
        [System.Serializable]
        public class HintEntry
        {
            public string id;
            [TextArea] public string text;
            public bool isMisleading;
            [Tooltip("If set, only send after this slot is filled incorrectly.")]
            public string triggerOnWrongSlotId;
            [Tooltip("If set, send when this slot is solved correctly.")]
            public string triggerOnCorrectSlotId;
            [Tooltip("Send once when the matching drawer unlocks.")]
            public string triggerOnDrawerId;
        }

        [Header("Pool")]
        [SerializeField] private List<HintEntry> hints = new List<HintEntry>();

        [Header("Behaviour")]
        [SerializeField] private bool sendRandomWrongHintOnWrongPlacement = true;
        [SerializeField] [Range(0f, 1f)] private float wrongHintChance = 0.7f;
        [SerializeField] private bool neverRepeat = true;

        [Header("Fallback copy")]
        [SerializeField] [TextArea] private string genericWrongHint =
            "Put it in the other drawer. Trust me.";
        [SerializeField] [TextArea] private string genericCorrectHint =
            "That looks right. Keep going.";

        private readonly HashSet<string> _sent = new HashSet<string>();
        private DialogueManager _dialogue;

        private void Awake()
        {
            _dialogue = FindFirstObjectByType<DialogueManager>();
        }

        private void OnEnable()
        {
            PuzzleEvents.WrongPlacement += HandleWrongPlacement;
            PuzzleEvents.CorrectPlacement += HandleCorrectPlacement;
            PuzzleEvents.DrawerUnlocked += HandleDrawerUnlocked;
        }

        private void OnDisable()
        {
            PuzzleEvents.WrongPlacement -= HandleWrongPlacement;
            PuzzleEvents.CorrectPlacement -= HandleCorrectPlacement;
            PuzzleEvents.DrawerUnlocked -= HandleDrawerUnlocked;
        }

        public void SendHintById(string id)
        {
            var entry = hints.Find(h => h.id == id);
            if (entry != null)
                Push(entry);
        }

        public void SendCustom(string text, bool misleading, string sourceId = "custom")
        {
            Push(new HintMessage
            {
                text = text,
                isMisleading = misleading,
                sourceId = sourceId
            });
        }

        private void HandleWrongPlacement(PlacementSlot slot, PlaceableItem item)
        {
            var specific = hints.Find(h =>
                !string.IsNullOrEmpty(h.triggerOnWrongSlotId) &&
                h.triggerOnWrongSlotId == slot.SlotId);

            if (specific != null)
            {
                Push(specific);
                return;
            }

            if (!sendRandomWrongHintOnWrongPlacement || Random.value > wrongHintChance)
                return;

            var pool = hints.FindAll(h => h.isMisleading && string.IsNullOrEmpty(h.triggerOnWrongSlotId)
                                                          && string.IsNullOrEmpty(h.triggerOnCorrectSlotId)
                                                          && string.IsNullOrEmpty(h.triggerOnDrawerId));
            if (pool.Count > 0)
                Push(pool[Random.Range(0, pool.Count)]);
            else
                SendCustom(genericWrongHint, true, "generic-wrong");
        }

        private void HandleCorrectPlacement(PlacementSlot slot, PlaceableItem item)
        {
            var specific = hints.Find(h =>
                !string.IsNullOrEmpty(h.triggerOnCorrectSlotId) &&
                h.triggerOnCorrectSlotId == slot.SlotId);

            if (specific != null)
            {
                Push(specific);
                return;
            }

            SendCustom(genericCorrectHint, false, "generic-correct");
        }

        private void HandleDrawerUnlocked(string drawerId)
        {
            var specific = hints.Find(h =>
                !string.IsNullOrEmpty(h.triggerOnDrawerId) &&
                h.triggerOnDrawerId == drawerId);

            if (specific != null)
                Push(specific);
        }

        private void Push(HintEntry entry)
        {
            if (neverRepeat && _sent.Contains(entry.id))
                return;

            _sent.Add(entry.id);
            Push(new HintMessage
            {
                text = entry.text,
                isMisleading = entry.isMisleading,
                sourceId = entry.id
            });
        }

        private void Push(HintMessage message)
        {
            PuzzleEvents.RaiseHint(message);

            if (_dialogue != null)
            {
                var prefix = message.isMisleading ? "[???] " : "[hint] ";
                _dialogue.SendChatMessage(prefix + message.text);
            }
            else
            {
                Debug.Log($"[Hint{(message.isMisleading ? " WRONG" : "")}] {message.text}");
            }
        }
    }
}
