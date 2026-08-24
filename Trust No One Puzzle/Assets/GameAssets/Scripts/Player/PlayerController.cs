using UnityEngine;
using UnityEngine.InputSystem;

enum RunType
{
    Button,
    Hold
}

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference runAction;

    [Header("Player Settings")]
    [SerializeField] private float walkSpeed, runSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float acceleration, deceleration;
    [SerializeField] private RunType runType;
        
    [Header("References")]
    [SerializeField] private Transform cam;

    private Vector2 _input;
    private bool _isRunning;
    
    private Vector3 _currentVelocity;
    private float _verticalVelocity;

    private const float Gravity = -9.81f;
    
    private CharacterController _controller;

    private void OnEnable()
    {
        moveAction.action.Enable();
        runAction.action.Enable();
        
        moveAction.action.performed += ActionOnMove;
        moveAction.action.canceled += ActionOnMove;
        
        runAction.action.started += ActionOnRun;
        if(runType == RunType.Hold)
            runAction.action.canceled += ActionOnRun;

        _controller = GetComponent<CharacterController>();
    }

    private void OnDisable()
    {
        moveAction.action.performed -= ActionOnMove;
        moveAction.action.canceled -= ActionOnMove;

        runAction.action.started -= ActionOnRun;
        if(runType == RunType.Hold)
            runAction.action.canceled -= ActionOnRun;
        
        moveAction.action.Disable();
        runAction.action.Disable();
    }

    private void Update()
    {
        ApplyGravity();
        MovementHandler();
        RotationHandler();
    }

    private void RotationHandler()
    {
        if (_input.sqrMagnitude > 0.1f)
        {
            var inputDirection = new Vector3(_input.x, 0, _input.y).normalized;
            var cameraForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
            var moveDirection = inputDirection.x * cam.right + inputDirection.z * cameraForward;
            
            if (moveDirection != Vector3.zero)
            {
                var targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation =
                    Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void MovementHandler()
    {
        var targetSpeed = _isRunning ? runSpeed : walkSpeed;
        var inputDirection = new Vector3(_input.x, 0, _input.y).normalized;
        var cameraForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
        var targetDirection = inputDirection.x * cam.right + inputDirection.z * cameraForward;

        if (targetDirection.sqrMagnitude > 0.1f)
        {
            _currentVelocity = Vector3.MoveTowards(
                _currentVelocity,
                targetDirection * targetSpeed,
                acceleration * Time.deltaTime);
        }
        else
        {
            _currentVelocity = Vector3.MoveTowards(
                _currentVelocity,
                Vector3.zero,
                deceleration * Time.deltaTime);
        }

        _controller.Move((_currentVelocity + Vector3.up * _verticalVelocity) * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (!_controller.isGrounded)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
        else
        {
            _verticalVelocity = Gravity;
        }
    }

    private void ActionOnMove(InputAction.CallbackContext ctx) =>
        _input = ctx.ReadValue<Vector2>();

    private void ActionOnRun(InputAction.CallbackContext ctx) =>
        _isRunning = !_isRunning;
}
