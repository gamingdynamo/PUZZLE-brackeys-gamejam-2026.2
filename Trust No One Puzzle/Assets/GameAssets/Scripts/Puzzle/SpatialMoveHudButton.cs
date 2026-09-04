using UnityEngine;
using UnityEngine.EventSystems;

namespace GameAssets.Scripts.Puzzle
{
    /// <summary>
    /// Hold-to-shove control for mobile. Put on a UI Button; it stays pressed
    /// while the finger is down (same as MMB on desktop).
    /// </summary>
    public class SpatialMoveHudButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private bool toggleInsteadOfHold;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (PlayerCarry.Instance == null)
                return;

            if (toggleInsteadOfHold)
                PlayerCarry.Instance.ToggleSpatialModeFromHud();
            else
                PlayerCarry.Instance.SetSpatialModeFromHud(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (toggleInsteadOfHold)
                return;
            PlayerCarry.Instance?.SetSpatialModeFromHud(false);
        }
    }
}
