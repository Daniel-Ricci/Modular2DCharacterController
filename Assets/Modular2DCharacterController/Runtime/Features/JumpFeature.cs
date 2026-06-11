using Modular2DCharacterController.Runtime.Core;
using Modular2DCharacterController.Runtime.Input;
using Modular2DCharacterController.Runtime.Data.FeatureProfiles;
using System;
using UnityEngine;

namespace Modular2DCharacterController.Runtime.Features
{
    /// <summary>
    /// A configurable feature that handles player jumps and air movement.
    /// </summary>
    [RequireComponent(typeof(CharacterController2D))]
    public class JumpFeature : MonoBehaviour, ICharacterFeature
    {
        [Header("Default Jump Profile")]
        [SerializeField]
        private JumpProfile defaultJumpProfile;

        [Header("Gameplay")]
        [SerializeField]
        [Min(1)]
        private int maxJumpCount = 2;

        [SerializeField]
        [Min(0f)]
        private float coyoteTime = 0.1f;

        [SerializeField]
        [Min(0f)]
        private float jumpBufferTime = 0.1f;

        [Header("Jump Type")]
        [SerializeField]
        private bool fixedJumpHeight = false;

        [SerializeField]
        [Min(1f)]
        private float jumpReleaseGravityMultiplier = 3f;

        [Header("Jump Hang Time")]
        [SerializeField]
        private bool enableJumpHangTime = true;

        [SerializeField]
        [Min(0f)]
        private float jumpHangVelocityThreshold = 1f;

        [SerializeField]
        [Range(0.1f, 1f)]
        private float jumpHangGravityMultiplier = 0.35f;
        
        // Event for jumping.
        // Uses the jump's velocity as parameter.
        public event Action<float> Jumped;

        private CharacterMotor _motor;
        private ICharacterInput _input;
        private GroundDetector _groundDetector;
        private Rigidbody2D _rigidbody;
        private CharacterController2D _controller;
        private DashFeature _dashFeature;
        private ProfileProvider<JumpProfile> _jumpProfileProvider;

        private float _gravity;
        private float _jumpVelocity;

        private float _coyoteTimer;
        private float _jumpBufferTimer;

        private int _remainingJumps;

        private bool _wasGrounded;
        private bool _jumpRequested;

        private void Awake()
        {
            _motor = GetComponent<CharacterMotor>();
            _input = GetComponent<ICharacterInput>();
            _groundDetector = GetComponent<GroundDetector>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _controller = GetComponent<CharacterController2D>();
            _dashFeature = GetComponent<DashFeature>();
            _jumpProfileProvider = _controller.JumpProfileProvider;

            if (defaultJumpProfile != null)
            {
                _jumpProfileProvider?.RegisterProfile(defaultJumpProfile);
            }

            _rigidbody.gravityScale = 0f;
            _remainingJumps = maxJumpCount;
        }

        private void OnEnable()
        {
            _groundDetector.Landed += ResetJumps;
        }

        private void OnDisable()
        {
            _groundDetector.Landed -= ResetJumps;
        }

        public void Tick()
        {
            if (_input.JumpPressed)
            {
                _jumpRequested = true;
            }
        }

        public void FixedTick()
        {
            JumpProfile currentJumpProfile = _jumpProfileProvider?.GetCurrentProfile();
            CalculateJumpValues(currentJumpProfile);
            UpdateTimers();
            TryJump();
            ApplyCustomGravity(currentJumpProfile);
        }

        private void CalculateJumpValues(JumpProfile currentJumpProfile)
        {
            _gravity =
                -(2f * currentJumpProfile.jumpHeight) /
                (currentJumpProfile.timeToApex * currentJumpProfile.timeToApex);

            _jumpVelocity =
                Mathf.Abs(_gravity) *
                currentJumpProfile.timeToApex;
        }

        private void UpdateTimers()
        {
            if (_groundDetector.IsGrounded)
            {
                _coyoteTimer = coyoteTime;
            }
            else
            {
                _coyoteTimer -= Time.fixedDeltaTime;
            }

            if (_jumpRequested)
            {
                _jumpBufferTimer = jumpBufferTime;
                _jumpRequested = false;
            }
            else
            {
                _jumpBufferTimer -= Time.fixedDeltaTime;
            }
        }

        private void TryJump()
        {
            if (_jumpBufferTimer <= 0f)
            {
                return;
            }

            bool canGroundJump =
                _groundDetector.IsGrounded ||
                _coyoteTimer > 0f;

            bool canAirJump =
                !canGroundJump &&
                _remainingJumps > 0;

            if (!canGroundJump && !canAirJump)
            {
                return;
            }

            if (canGroundJump)
            {
                _remainingJumps = maxJumpCount - 1;
            }
            else
            {
                _remainingJumps--;
            }

            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;

            _motor.SetVerticalVelocity(_jumpVelocity);
            Jumped?.Invoke(_jumpVelocity);
        }

        private void ApplyCustomGravity(JumpProfile currentJumpProfile)
        {
            if (_dashFeature != null && _dashFeature.IsDashing)
                return;
            
            float gravityMultiplier = 1f;

            // Falling
            if (_motor.VerticalVelocity < 0f)
            {
                gravityMultiplier =
                    currentJumpProfile.fallGravityMultiplier;
            }
            // Variable jump height
            else if (
                !fixedJumpHeight &&
                _motor.VerticalVelocity > 0f &&
                !_input.JumpHeld)
            {
                gravityMultiplier =
                    jumpReleaseGravityMultiplier;
            }
            
            // Jump hang time (only while ascending near apex)
            if (
                enableJumpHangTime &&
                _motor.VerticalVelocity > 0f &&
                _motor.VerticalVelocity <= jumpHangVelocityThreshold)
            {
                gravityMultiplier *=
                    jumpHangGravityMultiplier;
            }

            _rigidbody.AddForce(
                Vector2.up * (_gravity * gravityMultiplier * _rigidbody.mass),
                ForceMode2D.Force);
        }

        private void ResetJumps(Vector2 unused)
        {
            _remainingJumps = maxJumpCount;
        }
    }
}