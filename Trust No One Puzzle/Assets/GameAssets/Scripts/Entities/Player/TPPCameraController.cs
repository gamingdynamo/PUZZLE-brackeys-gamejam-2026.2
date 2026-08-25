using UnityEngine;
using UnityEngine.InputSystem;

namespace GameAssets.Scripts.Entities.Player
{
    public class TPPCameraController : MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionReference lookAction;

        [Header("Camera settings")] 
        [SerializeField] private float sensitivity;
        [SerializeField] private Vector2 pitchLimits;
        [SerializeField] private Vector3 offset;
        [SerializeField] private float collisionApplySpeed;

        [Header("Collision")] 
        [SerializeField] private float distance; //maybe for zoom too
        [SerializeField] private Transform target;
        [SerializeField] private float collisionOffset;
        [SerializeField] private LayerMask collisionLayers;

        private Vector2 _input;
        private float _pitch, _yaw;
        private Vector3 _cameraPosition;

        private Transform _cam;
    
        private void OnEnable()
        {
            lookAction.action.Enable();
        
            lookAction.action.performed += ActionOnLook;
            lookAction.action.canceled += ActionOnLook;

            _cam = GetComponentInChildren<Camera>().transform;
        
            LockCursor();
        }

        private void OnDisable()
        {
            lookAction.action.performed -= ActionOnLook;
            lookAction.action.canceled -= ActionOnLook;
        
            lookAction.action.Disable();
        }

        private void LockCursor(bool value = true)
        {
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !value;
        }

        private void Update()
        {
            RotationHandler();
            CollisionHandler();
            PositionHandler();
        }

        private void RotationHandler()
        {
            _yaw += _input.x * sensitivity;
            _pitch -= _input.y * sensitivity;
            _pitch = Mathf.Clamp(_pitch, pitchLimits.x, pitchLimits.y);

            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0);
        }

        private void CollisionHandler()
        {
            var desiredPosition = transform.position - transform.forward * distance;

            if (Physics.SphereCast(transform.position, 0.2f, -transform.forward,
                    out RaycastHit hit, distance + collisionOffset, collisionLayers.value))
            {
                _cameraPosition = transform.position - transform.forward * (hit.distance - collisionOffset);
            }
            else
            {
                _cameraPosition = desiredPosition;
            }
        }

        private void PositionHandler()
        {
            transform.position = target.position + offset;
        
            _cam.position = Vector3.Lerp(_cam.position, _cameraPosition, Time.deltaTime * collisionApplySpeed);
        }

        private void ActionOnLook(InputAction.CallbackContext ctx) =>
            _input = ctx.ReadValue<Vector2>();
    }
}
