using UnityEngine;
using UnityEngine.InputSystem;

namespace GameAssets.Scripts.Puzzle
{
    /// <summary>
    /// Carry + tight-storage shoving.
    /// Handheld items parent to the hold point.
    /// WorldStable / Spatial items keep world rotation (do not spin with look).
    /// Spatial: mouse wheel moves along the camera ray (down = toward cam, up = away);
    /// hold MMB or a mobile HUD button to slide in player space
    /// (screen X → player up / local Y, screen Y → player forward / local Z).
    /// Object yaw stays locked to the player body while shoved.
    /// </summary>
    public class PlayerCarry : MonoBehaviour
    {
        public static PlayerCarry Instance { get; private set; }

        [Header("Hold")]
        [SerializeField] private Transform holdPoint;
        [SerializeField] private Transform playerBody;
        [SerializeField] private Camera viewCamera;
        [SerializeField] private InputActionReference dropAction;
        [SerializeField] private InputActionReference spatialModeAction;
        [SerializeField] private float dropForward = 0.8f;

        [Header("Spatial move")]
        [SerializeField] private float scrollMetersPerNotch = 0.35f;
        [SerializeField] private float planeDragSensitivity = 0.008f;
        [SerializeField] private LayerMask obstructionMask = ~0;
        [SerializeField] private float collisionSkin = 0.02f;

        public PlaceableItem HeldItem { get; private set; }
        public bool IsCarrying => HeldItem != null;
        public bool SpatialModeActive { get; private set; }

        private float _holdDistance;
        private Vector3 _planarOffset;
        private Quaternion _yawOffsetFromPlayer;
        private bool _hudSpatialHeld;

        private void Awake()
        {
            Instance = this;
            if (playerBody == null)
                playerBody = transform;
            if (viewCamera == null)
                viewCamera = Camera.main;
            if (holdPoint == null)
            {
                var go = new GameObject("HoldPoint");
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(0.4f, 1.2f, 0.6f);
                holdPoint = go.transform;
            }
        }

        private void OnEnable()
        {
            Bind(dropAction, OnDrop, true);
            Bind(spatialModeAction, OnSpatialStarted, true);
            Bind(spatialModeAction, OnSpatialCanceled, false);
        }

        private void OnDisable()
        {
            Unbind(dropAction, OnDrop, true);
            Unbind(spatialModeAction, OnSpatialStarted, true);
            Unbind(spatialModeAction, OnSpatialCanceled, false);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            if (!IsCarrying || HeldItem.RotateWithHolder)
                return;

            UpdateSpatialInput();
            DriveHeldTransform();
        }

        public bool TryPickUp(PlaceableItem item)
        {
            if (item == null || IsCarrying)
                return false;

            HeldItem = item;
            item.SetHeld(true, holdPoint);

            var cam = Cam;
            var from = cam != null ? cam.transform.position : holdPoint.position;
            _holdDistance = Mathf.Clamp(
                Vector3.Distance(from, item.transform.position),
                item.MinHoldDistance,
                item.MaxHoldDistance);
            if (_holdDistance < 0.05f)
                _holdDistance = item.DefaultHoldDistance;

            _planarOffset = Vector3.zero;
            var playerYaw = YawRotation(playerBody.rotation);
            _yawOffsetFromPlayer = Quaternion.Inverse(playerYaw) * item.transform.rotation;
            return true;
        }

        public PlaceableItem TakeHeldItem()
        {
            var item = HeldItem;
            HeldItem = null;
            SpatialModeActive = false;
            return item;
        }

        public void DropInWorld()
        {
            if (!IsCarrying)
                return;

            var item = HeldItem;
            HeldItem = null;
            SpatialModeActive = false;
            item.SetHeld(false, null);
            if (item.Style == PlaceableItem.CarryStyle.Handheld)
                item.transform.position = holdPoint.position + transform.forward * dropForward;
        }

        /// <summary>Mobile HUD: pointer down / up on a hold-to-move button.</summary>
        public void SetSpatialModeFromHud(bool held)
        {
            _hudSpatialHeld = held;
            RefreshSpatialMode();
        }

        public void ToggleSpatialModeFromHud()
        {
            _hudSpatialHeld = !_hudSpatialHeld;
            RefreshSpatialMode();
        }

        private void UpdateSpatialInput()
        {
            if (!HeldItem.UsesSpatialCarry)
                return;

            var mouse = Mouse.current;
            if (mouse != null)
            {
                var scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    // Wheel up = away from camera, wheel down = toward camera.
                    var notches = scroll > 0f ? 1f : -1f;
                    if (Mathf.Abs(scroll) > 120f)
                        notches = scroll / 120f;
                    _holdDistance = Mathf.Clamp(
                        _holdDistance + notches * scrollMetersPerNotch,
                        HeldItem.MinHoldDistance,
                        HeldItem.MaxHoldDistance);
                }

                RefreshSpatialMode();

                if (SpatialModeActive)
                {
                    var delta = mouse.delta.ReadValue();
                    // Screen X → player local Y (up). Screen Y → player local Z (forward).
                    _planarOffset += playerBody.up * (delta.x * planeDragSensitivity);
                    _planarOffset += playerBody.forward * (delta.y * planeDragSensitivity);
                }
            }
        }

        private void DriveHeldTransform()
        {
            var item = HeldItem;
            var targetPos = ComputeTargetPosition();
            var targetRot = item.RotateWithHolder
                ? holdPoint.rotation
                : YawRotation(playerBody.rotation) * _yawOffsetFromPlayer;

            if (item.UsesSpatialCarry)
                targetPos = SweepTo(item, item.transform.position, targetPos);

            if (item.Body != null && item.Body.isKinematic)
            {
                item.Body.MovePosition(targetPos);
                item.Body.MoveRotation(targetRot);
            }
            else
            {
                item.transform.SetPositionAndRotation(targetPos, targetRot);
            }
        }

        private Vector3 ComputeTargetPosition()
        {
            var cam = Cam;
            var origin = cam != null ? cam.transform.position : holdPoint.position;
            var alongCam = cam != null ? cam.transform.forward : playerBody.forward;
            return origin + alongCam * _holdDistance + _planarOffset;
        }

        private Vector3 SweepTo(PlaceableItem item, Vector3 from, Vector3 to)
        {
            var delta = to - from;
            var dist = delta.magnitude;
            if (dist < 0.0001f)
                return from;

            var dir = delta / dist;
            var col = item.GetComponent<Collider>();
            if (col == null)
                return to;

            var extents = col.bounds.extents - Vector3.one * collisionSkin;
            extents = Vector3.Max(extents, Vector3.one * 0.05f);

            if (Physics.BoxCast(from, extents, dir, out var hit, item.transform.rotation, dist,
                    obstructionMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider != null && hit.collider.transform.IsChildOf(item.transform))
                    return to;
                return from + dir * Mathf.Max(0f, hit.distance - collisionSkin);
            }

            return to;
        }

        private void RefreshSpatialMode()
        {
            var mmb = Mouse.current != null && Mouse.current.middleButton.isPressed;
            SpatialModeActive = HeldItem != null && HeldItem.UsesSpatialCarry && (mmb || _hudSpatialHeld);
        }

        private Camera Cam => viewCamera != null ? viewCamera : Camera.main;

        private static Quaternion YawRotation(Quaternion rot)
        {
            var e = rot.eulerAngles;
            return Quaternion.Euler(0f, e.y, 0f);
        }

        private void OnDrop(InputAction.CallbackContext ctx) => DropInWorld();

        private void OnSpatialStarted(InputAction.CallbackContext ctx)
        {
            if (HeldItem != null && HeldItem.UsesSpatialCarry)
                SpatialModeActive = true;
        }

        private void OnSpatialCanceled(InputAction.CallbackContext ctx)
        {
            if (!_hudSpatialHeld)
                SpatialModeActive = false;
        }

        private static void Bind(InputActionReference reference, System.Action<InputAction.CallbackContext> cb, bool started)
        {
            if (reference == null) return;
            reference.action.Enable();
            if (started) reference.action.started += cb;
            else reference.action.canceled += cb;
        }

        private static void Unbind(InputActionReference reference, System.Action<InputAction.CallbackContext> cb, bool started)
        {
            if (reference == null) return;
            if (started) reference.action.started -= cb;
            else reference.action.canceled -= cb;
        }
    }
}
