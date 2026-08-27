using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using GameAssets.Scripts.Entities.Player;

namespace GameAssets.Scripts.Environment
{
    /// <summary>
    /// Opens a door by rotating its pivot or opens a drawer by sliding it.
    /// The object holding this component needs a collider that can be hit by the player's reticle ray.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class OpenableFurniture : MonoBehaviour
    {
        private enum OpenMode
        {
            Rotate,
            Slide
        }

        [Header("References")]
        [SerializeField] private Transform movingPart;
        [SerializeField] private TMP_Text interactionPrompt;
        [SerializeField] private FPPCameraController playerCameraController;

        [Header("Raycast")]
        [SerializeField, Min(0.1f)] private float interactionDistance = 3f;
        [SerializeField] private LayerMask interactionLayers = ~0;

        [Header("Opening")]
        [SerializeField] private OpenMode openMode = OpenMode.Rotate;
        [SerializeField] private Vector3 openRotation = new Vector3(0f, 90f, 0f);
        [SerializeField] private Vector3 openOffset = new Vector3(0f, 0f, 0.4f);
        [SerializeField, Min(0.1f)] private float openSpeed = 8f;

        private Quaternion _closedRotation;
        private Quaternion _openRotation;
        private Vector3 _closedPosition;
        private Vector3 _openPosition;
        private bool _isOpen;

        private void Awake()
        {
            if (movingPart == null)
            {
                movingPart = transform;
            }

            _closedRotation = movingPart.localRotation;
            _openRotation = _closedRotation * Quaternion.Euler(openRotation);
            _closedPosition = movingPart.localPosition;
            _openPosition = _closedPosition + openOffset;

            if (playerCameraController == null)
            {
                playerCameraController = FindFirstObjectByType<FPPCameraController>();
            }

            SetPromptVisible(false);
        }

        private void Update()
        {
            AnimateMovingPart();

            var isTargeted = IsTargetedByReticle();
            SetPromptVisible(isTargeted);

            if (isTargeted)
            {
                UpdatePromptText();

                if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                {
                    _isOpen = !_isOpen;
                    UpdatePromptText();
                }
            }
        }

        private bool IsTargetedByReticle()
        {
            if (playerCameraController == null)
            {
                playerCameraController = FindFirstObjectByType<FPPCameraController>();
            }

            if (playerCameraController == null ||
                !Physics.Raycast(playerCameraController.ReticleRay, out var hit, interactionDistance, interactionLayers,
                    QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            return hit.collider.GetComponentInParent<OpenableFurniture>() == this;
        }

        private void AnimateMovingPart()
        {
            var lerpFactor = 1f - Mathf.Exp(-openSpeed * Time.deltaTime);

            if (openMode == OpenMode.Rotate)
            {
                movingPart.localRotation = Quaternion.Slerp(
                    movingPart.localRotation,
                    _isOpen ? _openRotation : _closedRotation,
                    lerpFactor);
            }
            else
            {
                movingPart.localPosition = Vector3.Lerp(
                    movingPart.localPosition,
                    _isOpen ? _openPosition : _closedPosition,
                    lerpFactor);
            }
        }

        private void UpdatePromptText()
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.text = _isOpen ? "Press E To Close" : "Press E To Open";
            }
        }

        private void SetPromptVisible(bool visible)
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.gameObject.SetActive(visible);
            }
        }
    }
}
