using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameAssets.Scripts.Entities.Player
{
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
        [SerializeField] private RunType runBehaviour;
    
        [Header("Audio Settings")]
        [SerializeField] private AudioSource source;
        [SerializeField] private AudioClip[] footstepClips;
        [SerializeField] private float walkDelaySteps, runDelaySteps;
        
        [Header("References")]
        [SerializeField] private Transform cam;
        private CharacterController _controller;
        private AnimatorController _animatorController;
        
        private Vector2 _input;
        private bool _isRunning;
    
        private Vector3 _currentVelocity;
        private float _verticalVelocity;

        private const float Gravity = -9.81f;
        

        private void OnEnable()
        {
            moveAction.action.Enable();
            runAction.action.Enable();
        
            moveAction.action.performed += ActionOnMove;
            moveAction.action.canceled += ActionOnMove;
        
            runAction.action.started += ActionOnRun;
            if(runBehaviour == RunType.Hold)
                runAction.action.canceled += ActionOnRun;

            _controller = GetComponent<CharacterController>();
            _animatorController = GetComponentInChildren<AnimatorController>();

            StartCoroutine(FootstepsRoutine());
        }

        private void OnDisable()
        {
            moveAction.action.performed -= ActionOnMove;
            moveAction.action.canceled -= ActionOnMove;

            runAction.action.started -= ActionOnRun;
            if(runBehaviour == RunType.Hold)
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

            if (_currentVelocity != Vector3.zero)
            {
                _animatorController.MotionValue = _isRunning ? 2 : 1;
            }
            else
            {
                _animatorController.MotionValue = 0;
            }
            
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

        private IEnumerator FootstepsRoutine()
        {
            while (true)
            {
                if (_currentVelocity.sqrMagnitude > 0.1f && _controller.isGrounded)
                {
                    source.Play();
                }

                var delay = _isRunning ? runDelaySteps : walkDelaySteps;
                yield return new WaitForSeconds(delay);
            }
        }

        private void ActionOnMove(InputAction.CallbackContext ctx) =>
            _input = ctx.ReadValue<Vector2>();

        private void ActionOnRun(InputAction.CallbackContext ctx) =>
            _isRunning = !_isRunning;
    }
}