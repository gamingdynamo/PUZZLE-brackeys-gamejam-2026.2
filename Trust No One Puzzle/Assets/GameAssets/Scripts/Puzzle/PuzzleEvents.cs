using System;

namespace GameAssets.Scripts.Puzzle
{
    /// <summary>
    /// Decoupled events so hints, drawers, and UI don't need hard scene references.
    /// </summary>
    public static class PuzzleEvents
    {
        public static event Action<PlacementSlot, PlaceableItem> CorrectPlacement;
        public static event Action<PlacementSlot, PlaceableItem> WrongPlacement;
        public static event Action<PlacementSlot> SlotChanged;
        public static event Action<string> DrawerUnlocked;
        public static event Action<HintMessage> HintRequested;

        public static void RaiseCorrectPlacement(PlacementSlot slot, PlaceableItem item) =>
            CorrectPlacement?.Invoke(slot, item);

        public static void RaiseWrongPlacement(PlacementSlot slot, PlaceableItem item) =>
            WrongPlacement?.Invoke(slot, item);

        public static void RaiseSlotChanged(PlacementSlot slot) =>
            SlotChanged?.Invoke(slot);

        public static void RaiseDrawerUnlocked(string drawerId) =>
            DrawerUnlocked?.Invoke(drawerId);

        public static void RaiseHint(HintMessage hint) =>
            HintRequested?.Invoke(hint);
    }

    [Serializable]
    public struct HintMessage
    {
        public string text;
        public bool isMisleading;
        public string sourceId;
    }
}
