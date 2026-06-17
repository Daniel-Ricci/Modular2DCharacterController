using Modular2DCharacterController.Runtime.Core;
using Modular2DCharacterController.Runtime.Input;
using Modular2DCharacterController.Runtime.Data.FeatureProfiles;
using System;
using UnityEngine;

namespace Modular2DCharacterController.Runtime.Features
{
    /// <summary>
    /// A configurable feature that handles player jumps.
    /// Gravity itself is owned by CharacterMotor; this feature only modifies gravity
    /// when jump-specific behavior is needed.
    /// </summary>
    [RequireComponent(typeof(CharacterController2D))]
    public class JumpFeature : MonoBehaviour, ICharacterFeature
    {
        [Header("Default Jump Profile")]

        [Tooltip(
            "The default jump profile used when no higher-priority jump profile is active.")]
        [SerializeField]
        private JumpProfile defaultJumpProfile;

        [Header("Gameplay")]

        [Tooltip(
            "The maximum number of jumps that can be performed before landing. " +
            "A value of 1 allows a single jump, 2 enables double jump, etc.")]
        [SerializeField]
        [Min(1)]
        private int maxJumpCount = 2;

        [Tooltip(
            "Allows jumping shortly after leaving the ground, making jumps feel more forgiving.")]
        [SerializeField]
        [Min(0f)]
        private float coyoteTime = 0.1f;

        [Tooltip(
            "Allows a jump input pressed slightly before landing to be buffered and executed automatically.")]
        [SerializeField]
        [Min(0f)]
        private float jumpBufferTime = 0.1f;

        [Header("Jump Type")]

        [Tooltip(
            "If enabled, all jumps reach the same height regardless of how long the jump button is held.")]
        [SerializeField]
        private bool fixedJumpHeight = false;

        [Tooltip(
            "Additional gravity applied when the jump button is released early. " +
            "Higher values produce shorter jumps.")]
        [SerializeField]
        [Min(1f)]
        private float jumpReleaseGravityMultiplier = 3f;

        [Header("Jump Hang Time")]

        [Tooltip(
            "If enabled, gravity is reduced near the top of a jump to create a floatier apex.")]
        [SerializeField]
        private bool enableJumpHangTime = true;

        [Tooltip(
            "Maximum upward velocity at which jump hang time begins to take effect.")]
        [SerializeField]
        [Min(0f)]
        private float jumpHangVelocityThreshold = 1f;

        [Tooltip(
            "Gravity multiplier applied during jump hang time. " +
            "Lower values create a longer, floatier apex.")]
        [SerializeField]
        [Range(0.1f, 1f)]
        private float jumpHangGravityMultiplier = 0.35f;

        public event Action<float> Jumped;

        private CharacterMotor _motor;
        private ICharacterInput _input;
        private GroundDetector _groundDetector;
        private WallDetector _wallDetector;
        private CharacterController2D _controller;
        private DashFeature _dashFeature;
        private WallSlideFeature _wallSlideFeature;
        private WallJumpFeature _wallJumpFeature;
        private ProfileProvider<JumpProfile> _jumpProfileProvider;

        private float _jumpVelocity;

        private float _coyoteTimer;
        private float _jumpBufferTimer;

        private int _remainingJumps;

        private bool _jumpRequested;

        private void Awake()
        {
            _motor = GetComponent<CharacterMotor>();
            _input = GetComponent<ICharacterInput>();
            _groundDetector = GetComponent<GroundDetector>();
            _wallDetector = GetComponent<WallDetector>();
            _controller = GetComponent<CharacterController2D>();
            _dashFeature = GetComponent<DashFeature>();
            _wallSlideFeature = GetComponent<WallSlideFeature>();
            _wallJumpFeature = GetComponent<WallJumpFeature>();
            _jumpProfileProvider = _controller.JumpProfileProvider;

            _remainingJumps = maxJumpCount;
        }

        private void OnEnable()
        {
            if (defaultJumpProfile != null)
            {
                _jumpProfileProvider?.RegisterProfile(defaultJumpProfile);
            }
        }

        private void OnDisable()
        {
            if (defaultJumpProfile != null)
            {
                _jumpProfileProvider?.UnregisterProfile(defaultJumpProfile);
            }
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
            JumpProfile currentJumpProfile =
                _jumpProfileProvider?.GetCurrentProfile();

            if (!currentJumpProfile)
                return;

            CalculateJumpValues(currentJumpProfile);
            UpdateTimers();
            TryJump();
            ApplyJumpGravityModifiers(currentJumpProfile);
        }

        private void CalculateJumpValues(JumpProfile currentJumpProfile)
        {
            float gravity =
                Mathf.Abs(_motor.GravityAcceleration);

            _jumpVelocity =
                Mathf.Sqrt(2f * gravity * currentJumpProfile.jumpHeight);
        }

        private void UpdateTimers()
        {
            if (_groundDetector.IsGrounded)
            {
                _coyoteTimer = coyoteTime;
                _remainingJumps = maxJumpCount;
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
            if (_dashFeature != null && _dashFeature.IsDashing)
                return;

            if (_wallSlideFeature != null && _wallJumpFeature != null)
            {
                if (_wallSlideFeature.IsWallSliding) return;
                if (_wallJumpFeature.IsMovementLocked) return;
            }
            
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

        private void ApplyJumpGravityModifiers(JumpProfile currentJumpProfile)
        {
            float gravityMultiplier = 1f;

            if (_motor.VerticalVelocity < 0f)
            {
                gravityMultiplier =
                    currentJumpProfile.fallGravityMultiplier;
            }
            else if (!fixedJumpHeight &&
                     _motor.VerticalVelocity > 0f &&
                     !_input.JumpHeld)
            {
                gravityMultiplier =
                    jumpReleaseGravityMultiplier;
            }

            if (enableJumpHangTime &&
                _motor.VerticalVelocity > 0f &&
                _motor.VerticalVelocity <= jumpHangVelocityThreshold)
            {
                gravityMultiplier *=
                    jumpHangGravityMultiplier;
            }

            _motor.AddGravityMultiplier(gravityMultiplier);
        }
    }
}