using UnityEngine;
using UnityEngine.InputSystem;

namespace GameAssets.Scripts.Entities.Player
{
    public class FPPCameraController : MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionReference lookAction;

        [Header("Camera settings")] 
        [SerializeField] private float sensitivity;
        [SerializeField] private Vector2 pitchLimits;

        private Vector2 _input;
        private float _pitch, _yaw;
    
        private void OnEnable()
        {
            lookAction.action.Enable();
        
            lookAction.action.performed += ActionOnLook;
            lookAction.action.canceled += ActionOnLook;
        
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
        }

        private void RotationHandler()
        {
            _yaw += _input.x * sensitivity;
            _pitch -= _input.y * sensitivity;
            _pitch = Mathf.Clamp(_pitch, pitchLimits.x, pitchLimits.y);

            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0);
        }
        
        private void ActionOnLook(InputAction.CallbackContext ctx) =>
            _input = ctx.ReadValue<Vector2>();
    }
}
