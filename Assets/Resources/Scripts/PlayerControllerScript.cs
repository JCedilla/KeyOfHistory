using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KeyOfHistory.Manager;
using KeyOfHistory.UI;


namespace KeyOfHistory.PlayerControl
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Animation Speed")]
        [SerializeField] private float AnimBlendSpeed = 8.9f;
        
        [Header("Camera System")]
        [SerializeField] private Transform CameraRoot;
        [SerializeField] private Transform Camera;
        [SerializeField] private float UpperLimit = -40f;
        [SerializeField] private float BottomLimit = 70f;
        [SerializeField] private float MouseSensitivity = 21.9f;
        [SerializeField] private float CameraHeadHeight = 1.6f;
        [SerializeField] private float CrouchCameraHeight = 0.8f;
        
        [Header("Jump Settings")]
        [SerializeField] private float JumpForce = 12f;
        [SerializeField] private float GroundCheckDistance = 0.5f;
        [SerializeField] private LayerMask GroundLayer;
        [SerializeField] private Transform GroundCheckPoint;
        [SerializeField] private float FallMultiplier = 2.5f;
        [SerializeField] private float LowJumpMultiplier = 2f;
        
        [Header("Crouch Settings")]
        [SerializeField] private float CrouchSpeed = 3f;
        [SerializeField] private float StandingHeight = 2f;
        [SerializeField] private float CrouchHeight = 1f;
        [SerializeField] private float CrouchTransitionSpeed = 10f;
        
        [Header("Stamina System")]
        [SerializeField] private float MaxStamina = 100f;
        [SerializeField] private float StaminaDrainRate = 35f;
        [SerializeField] private float StaminaRegenRate = 15f;
        [SerializeField] private float MinStaminaToRun = 55f;
        [SerializeField] private float CurrentStamina;
        
        [Header("UI References")]
        [SerializeField] private StaminaUIManager StaminaUI;

        [Header("Footstep Audio")]
        [SerializeField] private AudioSource FootstepAudioSource;
        [SerializeField] private AudioClip WalkSound;
        [SerializeField] private AudioClip RunSound;
        [SerializeField] private AudioClip CrouchSound;

        // Private variables
        private float _stepTimer;
        private bool _canRun = true;
        private Rigidbody _playerRigidbody;
        private InputManager _inputManager;
        private Animator _animator;
        private bool _grounded;
        private bool _hasAnimator;
        private int _xVelHash;
        private int _yVelHash;
        private int _jumpHash;
        private int _groundHash;
        private int _fallingHash;
        private int _crouchingHash;
        private float _xRotation;
        private float _groundCheckBuffer = 0f;
        private float _jumpCooldown = 0f; 
        private bool _jumpPressed = false;
        private Vector3 _cameraRecoil = Vector3.zero;
        private float _recoilRecoverySpeed = 5f;
        private Collider[] _groundCheckResults = new Collider[1];
        private CapsuleCollider _capsuleCollider;
        private bool _isCrouching = false;
        private float _currentCameraHeight;

        private const float _walkSpeed = 6f;
        private const float _runSpeed = 14f;
        private Vector2 _currentVelocity;

        private void Start()
        {
            _hasAnimator = TryGetComponent<Animator>(out _animator);
            _playerRigidbody = GetComponent<Rigidbody>();
            _inputManager = GetComponent<InputManager>();
            _capsuleCollider = GetComponent<CapsuleCollider>();

            _xVelHash = Animator.StringToHash("X_Velocity");
            _yVelHash = Animator.StringToHash("Y_Velocity");
            _jumpHash = Animator.StringToHash("Jump");
            _groundHash = Animator.StringToHash("Grounded");
            _fallingHash = Animator.StringToHash("Falling");
            _crouchingHash = Animator.StringToHash("Crouching");

            CurrentStamina = MaxStamina;
            _currentCameraHeight = CameraHeadHeight;
            
            // Store original collider height
            if (_capsuleCollider != null)
            {
                StandingHeight = _capsuleCollider.height;
            }
        }

        private void FixedUpdate()
        {
            SampleGround();
            HandleCrouch();
            Move();
            HandleJump();
            ApplyGravity();
            UpdateStamina();
            HandleFootsteps();
        }

        private void LateUpdate()
        {
            UpdateCameraVerticalFollow();
            CamMovements();
            UpdateCameraRecoil();
        }

        private void HandleCrouch()
        {
            if (!_hasAnimator) return;

            bool wantsToCrouch = _inputManager.Crouch && _grounded;

            // Check if we can stand up (no ceiling above)
            if (_isCrouching && !_inputManager.Crouch)
            {
                if (CanStandUp())
                {
                    _isCrouching = false;
                }
            }
            else if (wantsToCrouch)
            {
                _isCrouching = true;
            }

            // Smoothly adjust collider height
            if (_capsuleCollider != null)
            {
                float targetHeight = _isCrouching ? CrouchHeight : StandingHeight;
                _capsuleCollider.height = Mathf.Lerp(_capsuleCollider.height, targetHeight, CrouchTransitionSpeed * Time.fixedDeltaTime);
                
                // Adjust collider center to keep feet on ground
                Vector3 center = _capsuleCollider.center;
                center.y = _capsuleCollider.height / 2f;
                _capsuleCollider.center = center;
            }

            // Update animator
            _animator.SetBool(_crouchingHash, _isCrouching);
        }

        private bool CanStandUp()
        {
            // Check if there's space above to stand up
            Vector3 checkPosition = transform.position + Vector3.up * CrouchHeight;
            float checkDistance = StandingHeight - CrouchHeight + 0.2f;
            
            return !Physics.Raycast(checkPosition, Vector3.up, checkDistance, GroundLayer);
        }

        private void Move()
        {
            if (!_hasAnimator) return;
            
            Vector3 currentVel = _playerRigidbody.linearVelocity;
            
            // Determine target speed based on state
            float targetSpeed;
            if (_isCrouching)
            {
                targetSpeed = CrouchSpeed; // Slow when crouched
            }
            else if (_inputManager.Run && _canRun)
            {
                targetSpeed = _runSpeed;
            }
            else
            {
                targetSpeed = _walkSpeed;
            }
            
            if (_inputManager.Move == Vector2.zero) targetSpeed = 0.01f;

            _currentVelocity.x = Mathf.Lerp(_currentVelocity.x, _inputManager.Move.x * targetSpeed, AnimBlendSpeed * Time.fixedDeltaTime);
            _currentVelocity.y = Mathf.Lerp(_currentVelocity.y, _inputManager.Move.y * targetSpeed, AnimBlendSpeed * Time.fixedDeltaTime);

            var xVelDifference = _currentVelocity.x - currentVel.x;
            var zVelDifference = _currentVelocity.y - currentVel.z;

            _playerRigidbody.AddForce(transform.TransformVector(new Vector3(xVelDifference, 0, zVelDifference)), ForceMode.VelocityChange);

            _animator.SetFloat(_xVelHash, _currentVelocity.x);
            _animator.SetFloat(_yVelHash, _currentVelocity.y);
        }

        private void CamMovements()
        {
            if (!_hasAnimator) return;

            var Mouse_X = _inputManager.Look.x;
            var Mouse_Y = _inputManager.Look.y;

            // Get sensitivity from SettingsManager if available, otherwise use default
            float sensitivity = Manager.SettingsManager.Instance != null ? 
                Manager.SettingsManager.Instance.GetMouseSensitivity() : MouseSensitivity;

            _xRotation -= Mouse_Y * sensitivity * Time.smoothDeltaTime;
            _xRotation = Mathf.Clamp(_xRotation, UpperLimit, BottomLimit);

            Camera.localRotation = Quaternion.Euler(_xRotation, 0, 0);
            _playerRigidbody.MoveRotation(_playerRigidbody.rotation * Quaternion.Euler(0, Mouse_X * sensitivity * Time.smoothDeltaTime, 0));
        }

        private void UpdateCameraVerticalFollow()
        {
            // Smoothly adjust camera height based on crouch state
            float targetHeight = _isCrouching ? CrouchCameraHeight : CameraHeadHeight;
            _currentCameraHeight = Mathf.Lerp(_currentCameraHeight, targetHeight, CrouchTransitionSpeed * Time.deltaTime);

            // Make camera follow player's Y position smoothly
            Vector3 targetPos = CameraRoot.position;
            targetPos.y = transform.position.y + _currentCameraHeight;
            CameraRoot.position = Vector3.Lerp(CameraRoot.position, targetPos, 8f * Time.deltaTime);
        }

        private void UpdateCameraRecoil()
        {
            // Apply recoil offset to camera
            Camera.localPosition = Vector3.Lerp(Camera.localPosition, _cameraRecoil, 10f * Time.deltaTime);

            // Smoothly recover to zero
            _cameraRecoil = Vector3.Lerp(_cameraRecoil, Vector3.zero, _recoilRecoverySpeed * Time.deltaTime);
        }
        
        public void ResetCameraRotation()
        {
            _xRotation = 0f; // Reset vertical look
        }

        private void HandleJump()
        {
            if (!_hasAnimator) return;
            if (_jumpCooldown > 0f) return;
            if (_isCrouching) return; // Can't jump while crouched
            
            // Only trigger on button DOWN (not held)
            if (_inputManager.Jump && !_jumpPressed)
            {
                _jumpPressed = true;
                
                if (_grounded)
                {
                    // Stop footsteps immediately when jumping
                    if (FootstepAudioSource != null && FootstepAudioSource.isPlaying)
                    {
                        FootstepAudioSource.Stop();
                    }
                    
                    // CRITICAL: Completely reset vertical velocity
                    _playerRigidbody.linearVelocity = new Vector3(
                        _playerRigidbody.linearVelocity.x, 
                        0f, 
                        _playerRigidbody.linearVelocity.z
                    );
                    
                    // Apply jump using VelocityChange for instant effect
                    _playerRigidbody.AddForce(Vector3.up * JumpForce, ForceMode.VelocityChange);
                    
                    _grounded = false;
                    _jumpCooldown = 0.2f;
                    
                    _animator.SetTrigger(_jumpHash);
                    _animator.SetBool(_groundHash, false);
                    _animator.SetBool(_fallingHash, true);
                }
            }
            
            // Reset when button released
            if (!_inputManager.Jump)
            {
                _jumpPressed = false;
            }
        }

        private void ApplyGravity()
        {
            // Make falling feel better
            if (_playerRigidbody.linearVelocity.y < 0)
            {
                // Falling - apply extra gravity
                _playerRigidbody.linearVelocity += Vector3.up * Physics.gravity.y * (FallMultiplier - 1) * Time.fixedDeltaTime;
            }
            else if (_playerRigidbody.linearVelocity.y > 0 && !_inputManager.Jump)
            {
                // Released jump button early - fall faster
                _playerRigidbody.linearVelocity += Vector3.up * Physics.gravity.y * (LowJumpMultiplier - 1) * Time.fixedDeltaTime;
            }
        }

        private void SampleGround()
        {
            if (!_hasAnimator) return;

            // Decrease jump cooldown
            if (_jumpCooldown > 0f)
            {
                _jumpCooldown -= Time.fixedDeltaTime;
            }

            Vector3 checkPosition = GroundCheckPoint != null ? GroundCheckPoint.position : transform.position;
            bool wasGrounded = _grounded;
            
            // Use non-allocating version to avoid memory warnings
            int hitCount = Physics.OverlapSphereNonAlloc(checkPosition, GroundCheckDistance, _groundCheckResults, GroundLayer);
            bool hitGround = hitCount > 0;

            // Add buffer to prevent flickering
            if (hitGround)
            {
                if (!_grounded) // Just landed
                {
                    _cameraRecoil = new Vector3(0, -0.15f, 0); // Landing camera dip
                }
                
                _grounded = true;
                _groundCheckBuffer = 0.1f;
            }
            else
            {
                _groundCheckBuffer -= Time.fixedDeltaTime;
                if (_groundCheckBuffer <= 0f)
                {
                    _grounded = false;
                }
            }

            // Update animator
            _animator.SetBool(_groundHash, _grounded);
            _animator.SetBool(_fallingHash, !_grounded);
        }

        private void UpdateStamina()
        {
            bool isRunning = _inputManager.Run && _inputManager.Move != Vector2.zero && _canRun && !_isCrouching;

            if (isRunning)
            {
                CurrentStamina -= StaminaDrainRate * Time.fixedDeltaTime;
                CurrentStamina = Mathf.Max(CurrentStamina, 0f);

                if (CurrentStamina <= 0f)
                {
                    _canRun = false;
                }
            }
            else
            {
                CurrentStamina += StaminaRegenRate * Time.fixedDeltaTime;
                CurrentStamina = Mathf.Min(CurrentStamina, MaxStamina);

                if (CurrentStamina >= MinStaminaToRun)
                {
                    _canRun = true;
                }
            }

            // Update UI
            if (StaminaUI != null)
            {
                StaminaUI.UpdateStaminaUI(CurrentStamina, MaxStamina);
            }
        }

        private void HandleFootsteps()
        {
            if (FootstepAudioSource == null) return;

            bool isMoving = _inputManager.Move != Vector2.zero;
            bool isRunning = _inputManager.Run && _canRun && !_isCrouching;

            if (isMoving && _grounded) // Only play footsteps when grounded
            {
                AudioClip correctClip;
                
                // Choose correct sound based on movement state
                if (_isCrouching)
                {
                    correctClip = CrouchSound != null ? CrouchSound : WalkSound;
                }
                else if (isRunning)
                {
                    correctClip = RunSound;
                }
                else
                {
                    correctClip = WalkSound;
                }
                
                if (!FootstepAudioSource.isPlaying)
                {
                    FootstepAudioSource.clip = correctClip;
                    FootstepAudioSource.loop = true;
                    FootstepAudioSource.Play();
                }
                else if (FootstepAudioSource.clip != correctClip)
                {
                    // Only switch clip if different to avoid allocations
                    FootstepAudioSource.Stop();
                    FootstepAudioSource.clip = correctClip;
                    FootstepAudioSource.Play();
                }
            }
            else if (FootstepAudioSource.isPlaying)
            {
                FootstepAudioSource.Stop();
            }
        }

        // Debug visualization
        private void OnDrawGizmos()
        {
            if (GroundCheckPoint != null)
            {
                // Draw ground check sphere
                Gizmos.color = _grounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(GroundCheckPoint.position, GroundCheckDistance);
            }

            // Draw ceiling check for standing up
            if (_isCrouching)
            {
                Vector3 checkPosition = transform.position + Vector3.up * CrouchHeight;
                float checkDistance = StandingHeight - CrouchHeight + 0.2f;
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(checkPosition, checkPosition + Vector3.up * checkDistance);
            }
        }
    }
}