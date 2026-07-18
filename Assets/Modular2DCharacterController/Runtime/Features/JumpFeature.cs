using Modular2DCharacterController.Runtime.Core;
using Modular2DCharacterController.Runtime.Input;
using Modular2DCharacterController.Runtime.Data.FeatureProfiles;
using System;
using UnityEngine;

namespace Modular2DCharacterController.Runtime.Features
{
    /// <summary>
    /// A configurable feature that handles character jumps.
    /// 
    /// Gravity itself is owned by CharacterMotor; this feature only modifies gravity
    /// while it is actively managing a jump arc.
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
            "The maximum number of air jumps that can be performed before landing, " +
            "excluding the first, grounded jump. " +
            "A value of 0 allows a single grounded jump, 1 enables double jump, etc.")]
        [SerializeField]
        [Min(0)]
        private int maxAirJumpCount = 1;

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

        [Header("Jump Arc")]

        [Tooltip(
            "If enabled, JumpProfile Time To Apex is used to calculate an ascent gravity multiplier. " +
            "If disabled, the motor's base gravity is used as the source of truth.")]
        [SerializeField]
        private bool useTimeToApex = false;

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
        
        [Header("Air Dash Jump")]

        [Tooltip(
            "If enabled, the character can jump shortly after an dash ends " +
            "without consuming an air jump.")]
        [SerializeField]
        private bool allowJumpAfterDash = true;

        [Tooltip(
            "How long after an dash ends the free jump window remains available.")]
        [SerializeField]
        [Min(0f)]
        private float jumpAfterDashTime = 0.15f;

        [Header("Air Roll Jump")]

        [Tooltip(
            "If enabled, the character can jump shortly after an edge-continuing roll ends in the air " +
            "without consuming an air jump.")]
        [SerializeField]
        private bool allowJumpAfterAirRoll = true;

        [Tooltip(
            "How long after an edge-continuing roll ends in the air the free jump window remains available.")]
        [SerializeField]
        [Min(0f)]
        private float jumpAfterAirRollTime = 0.15f;

        // Event triggered when the character jumps.
        public event Action<float> Jumped;

        public JumpProfile CurrentJumpProfile =>
            _jumpProfileProvider?.GetCurrentProfile();

        public int RemainingAirJumps => _remainingJumps;

        public float CoyoteTimer => _coyoteTimer;

        public float JumpBufferTimer => _jumpBufferTimer;

        public float JumpAfterDashTimer => _jumpAfterDashTimer;

        public float JumpAfterAirRollTimer => _jumpAfterAirRollTimer;

        public float JumpVelocity => _jumpVelocity;

        public float AscentGravityMultiplier => _ascentGravityMultiplier;

        public bool IsJumpActive => _isJumpActive;

        public bool IsJumpAscending => _isJumpAscending;

        // Components used by this feature.
        private CharacterMotor _motor;
        private ICharacterInput _input;
        private GroundDetector _groundDetector;
        private CharacterController2D _controller;
        private DashFeature _dashFeature;
        private RollFeature _rollFeature;
        private WallSlideFeature _wallSlideFeature;
        private WallJumpFeature _wallJumpFeature;
        private GroundPoundFeature _groundPoundFeature;
        private ProfileProvider<JumpProfile> _jumpProfileProvider;

        // Keeps track of jump velocity and gravity multiplier
        // to be applied.
        private float _jumpVelocity;
        private float _ascentGravityMultiplier = 1f;

        // Timers used for coyote jump and input buffer.
        private float _coyoteTimer;
        private float _jumpBufferTimer;
        private float _jumpAfterDashTimer;
        private float _jumpAfterAirRollTimer;

        // Keeps track of remaining jumps.
        private int _remainingJumps;

        // Jump state variables.
        private bool _jumpRequested;
        private bool _isJumpActive;
        private bool _isJumpAscending;

        private void Awake()
        {
            _motor = GetComponent<CharacterMotor>();
            _input = GetComponent<ICharacterInput>();
            _groundDetector = GetComponent<GroundDetector>();
            _controller = GetComponent<CharacterController2D>();
            _dashFeature = GetComponent<DashFeature>();
            _rollFeature = GetComponent<RollFeature>();
            _wallSlideFeature = GetComponent<WallSlideFeature>();
            _wallJumpFeature = GetComponent<WallJumpFeature>();
            _groundPoundFeature = GetComponent<GroundPoundFeature>();
            _jumpProfileProvider = _controller.JumpProfileProvider;

            _remainingJumps = maxAirJumpCount;
        }

        private void OnEnable()
        {
            if (defaultJumpProfile != null)
            {
                _jumpProfileProvider?.RegisterProfile(defaultJumpProfile);
            }
            
            if (_dashFeature != null)
            {
                _dashFeature.DashEnded += OnAirDashEnded;
            }

            if (_rollFeature != null)
            {
                _rollFeature.RollEnded += OnRollEnded;
            }
        }

        private void OnDisable()
        {
            if (defaultJumpProfile != null)
            {
                _jumpProfileProvider?.UnregisterProfile(defaultJumpProfile);
            }
            
            if (_dashFeature != null)
            {
                _dashFeature.DashEnded -= OnAirDashEnded;
            }

            if (_rollFeature != null)
            {
                _rollFeature.RollEnded -= OnRollEnded;
            }
        }

        public void Tick()
        {
            if (_groundPoundFeature != null &&
                _groundPoundFeature.IsRecoveryActive &&
                !_groundPoundFeature.CanJumpDuringRecovery)
            {
                _jumpRequested = false;
                return;
            }

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

            UpdateTimers();
            UpdateJumpState();

            if (_jumpBufferTimer > 0f)
            {
                CalculateJumpValues(currentJumpProfile);
                TryJump();
            }

            ApplyJumpGravityModifiers(currentJumpProfile);
        }

        private void UpdateTimers()
        {
            if (_groundDetector.IsGrounded)
            {
                _coyoteTimer = coyoteTime;
                _remainingJumps = maxAirJumpCount;
                _jumpAfterDashTimer = 0f;
                _jumpAfterAirRollTimer = 0f;
            }
            else
            {
                _coyoteTimer -= Time.fixedDeltaTime;
                
                if (_jumpAfterDashTimer > 0f)
                {
                    _jumpAfterDashTimer -= Time.fixedDeltaTime;
                }

                if (_jumpAfterAirRollTimer > 0f)
                {
                    _jumpAfterAirRollTimer -= Time.fixedDeltaTime;
                }
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

        private void UpdateJumpState()
        {
            Vector2 currentVelocity =
                _motor.CurrentSelfVelocity;

            if (_groundDetector.IsGrounded)
            {
                _isJumpActive = false;
                _isJumpAscending = false;
                return;
            }

            if (_dashFeature != null && _dashFeature.IsDashing)
            {
                _isJumpActive = false;
                _isJumpAscending = false;
                return;
            }

            if (_wallJumpFeature != null && _wallJumpFeature.IsControlInfluenceActive)
            {
                _isJumpActive = false;
                _isJumpAscending = false;
                return;
            }

            if (_groundPoundFeature != null &&
                _groundPoundFeature.IsGroundPounding)
            {
                _isJumpActive = false;
                _isJumpAscending = false;
                return;
            }

            if (_isJumpAscending && currentVelocity.y <= 0f)
            {
                _isJumpAscending = false;
            }

            if (_isJumpActive && currentVelocity.y <= 0f)
            {
                _isJumpAscending = false;
            }
        }

        private void CalculateJumpValues(JumpProfile currentJumpProfile)
        {
            float baseGravity =
                Mathf.Abs(_motor.GravityAcceleration);

            if (baseGravity <= 0.0001f)
            {
                _jumpVelocity = 0f;
                _ascentGravityMultiplier = 1f;
                return;
            }

            if (useTimeToApex)
            {
                float safeTimeToApex =
                    Mathf.Max(currentJumpProfile.timeToApex, 0.0001f);

                float requiredGravity =
                    (2f * currentJumpProfile.jumpHeight) /
                    (safeTimeToApex * safeTimeToApex);

                _ascentGravityMultiplier =
                    requiredGravity / baseGravity;

                _jumpVelocity =
                    requiredGravity * safeTimeToApex;

                return;
            }

            _ascentGravityMultiplier = 1f;

            _jumpVelocity =
                Mathf.Sqrt(
                    2f *
                    baseGravity *
                    currentJumpProfile.jumpHeight);
        }

        private void TryJump()
        {
            if (_dashFeature != null && _dashFeature.IsDashing)
            {
                if (!_dashFeature.TryInterruptDash())
                    return;
            }

            if (_rollFeature != null && _rollFeature.IsRolling)
            {
                if (!_rollFeature.TryInterruptRoll())
                    return;
            }

            if (_groundPoundFeature != null &&
                _groundPoundFeature.IsRecoveryActive)
            {
                if (!_groundPoundFeature.CanJumpDuringRecovery)
                {
                    _jumpBufferTimer = 0f;
                    return;
                }
            }

            if (_groundPoundFeature != null &&
                _groundPoundFeature.IsGroundPounding)
            {
                if (!_groundPoundFeature.TryInterruptGroundPound())
                    return;
            }

            if (_wallSlideFeature != null &&
                _wallJumpFeature != null)
            {
                if (_wallSlideFeature.IsWallSliding)
                    return;

                if (_wallJumpFeature.IsControlInfluenceActive)
                    return;
            }

            bool canGroundJump =
                _groundDetector.IsGrounded ||
                _coyoteTimer > 0f;
            
            bool canJumpAfterAirDash =
                !canGroundJump &&
                allowJumpAfterDash &&
                _jumpAfterDashTimer > 0f;

            bool canJumpAfterAirRoll =
                !canGroundJump &&
                allowJumpAfterAirRoll &&
                _jumpAfterAirRollTimer > 0f;

            bool canAirJump =
                !canGroundJump &&
                !canJumpAfterAirDash &&
                !canJumpAfterAirRoll &&
                _remainingJumps > 0;

            if (!canGroundJump &&
                !canAirJump &&
                !canJumpAfterAirDash &&
                !canJumpAfterAirRoll)
            {
                return;
            }

            if (_groundPoundFeature != null &&
                _groundPoundFeature.IsRecoveryActive &&
                !_groundPoundFeature.TryConsumeRecoveryJump())
            {
                _jumpBufferTimer = 0f;
                return;
            }

            if (canGroundJump)
            {
                _remainingJumps = maxAirJumpCount;
            }
            else if (canJumpAfterAirDash)
            {
                _jumpAfterDashTimer = 0f;
            }
            else if (canJumpAfterAirRoll)
            {
                _jumpAfterAirRollTimer = 0f;
            }
            else
            {
                _remainingJumps--;
            }

            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;

            _motor.SetVerticalSelfVelocity(_jumpVelocity);

            _isJumpActive = true;
            _isJumpAscending = true;

            Jumped?.Invoke(_jumpVelocity);
        }

        private void ApplyJumpGravityModifiers(
            JumpProfile currentJumpProfile)
        {
            if (!_isJumpActive)
                return;

            if (_dashFeature != null && _dashFeature.IsDashing)
                return;

            if (_wallJumpFeature != null &&
                _wallJumpFeature.IsControlInfluenceActive)
            {
                return;
            }

            Vector2 currentVelocity =
                _motor.CurrentSelfVelocity;

            float finalGravityMultiplier = 1f;

            if (currentVelocity.y < 0f)
            {
                finalGravityMultiplier *=
                    currentJumpProfile.fallGravityMultiplier;

                _isJumpAscending = false;
            }
            else if (_isJumpAscending)
            {
                finalGravityMultiplier *=
                    _ascentGravityMultiplier;

                if (!fixedJumpHeight && !_input.JumpHeld)
                {
                    finalGravityMultiplier *=
                        jumpReleaseGravityMultiplier;
                }

                if (enableJumpHangTime &&
                    currentVelocity.y <= jumpHangVelocityThreshold)
                {
                    finalGravityMultiplier *=
                        jumpHangGravityMultiplier;
                }
            }

            _motor.AddGravityMultiplier(finalGravityMultiplier);
        }
        
        private void OnAirDashEnded()
        {
            if (!allowJumpAfterDash)
                return;

            _jumpAfterDashTimer = jumpAfterDashTime;
        }

        private void OnRollEnded()
        {
            if (!allowJumpAfterAirRoll)
                return;

            if (_groundDetector == null ||
                _groundDetector.IsGrounded)
            {
                return;
            }

            if (_rollFeature == null ||
                _rollFeature.StopWhenLeavingGround)
            {
                return;
            }

            _jumpAfterAirRollTimer = jumpAfterAirRollTime;
        }
    }
}
