using System;
using Modular2DCharacterController.Runtime.Core;
using Modular2DCharacterController.Runtime.Data.FeatureProfiles;
using Modular2DCharacterController.Runtime.Input;
using UnityEngine;

namespace Modular2DCharacterController.Runtime.Features
{
    /// <summary>
    /// A configurable feature that handles player wall jumps.
    ///
    /// Can require an active wall slide, or only require touching a wall,
    /// depending on the feature settings.
    /// </summary>
    [RequireComponent(typeof(CharacterController2D))]
    [RequireComponent(typeof(WallDetector))]
    public class WallJumpFeature : MonoBehaviour, ICharacterFeature
    {
        [Header("Default Wall Jump Profile")]

        [Tooltip(
            "The default wall jump profile registered when this feature initializes.")]
        [SerializeField]
        private WallJumpProfile defaultWallJumpProfile;

        [Header("Wall Jump Conditions")]

        [Tooltip(
            "If enabled, the character must be wall sliding to wall jump. " +
            "If disabled, simply touching a valid wall is enough.")]
        [SerializeField]
        private bool requireWallSlide = true;

        [Header("Forgiveness")]

        [Tooltip(
            "Allows wall jumping shortly after leaving a valid wall jump contact.")]
        [SerializeField]
        [Min(0f)]
        private float wallJumpCoyoteTime = 0.1f;

        [Tooltip(
            "Allows a jump input pressed shortly before touching a valid wall jump contact to be buffered " +
            "and executed automatically.")]
        [SerializeField]
        [Min(0f)]
        private float wallJumpBufferTime = 0.1f;

        // Components used by this feature.
        private CharacterMotor _motor;
        private GroundDetector _groundDetector;
        private WallDetector _wallDetector;
        private WallSlideFeature _wallSlideFeature;
        private ICharacterInput _input;
        private CharacterController2D _controller;
        private DashFeature _dashFeature;
        private ProfileProvider<WallJumpProfile> _wallJumpProfileProvider;

        private float _controlInfluenceTimer;
        private float _wallJumpCoyoteTimer;
        private float _wallJumpBufferTimer;

        private bool _wallJumpRequested;
        private Vector2 _lastWallJumpNormal;
        private float _wallJumpImpulseX;
        
        // Invoked when a wall jump is performed.
        public event Action WallJumped;
        
        // Gets a boolean indicating whether wall jump movement is currently
        // applying special control influence.
        public bool IsControlInfluenceActive =>
            _controlInfluenceTimer > 0f;

        public WallJumpProfile CurrentWallJumpProfile =>
            _wallJumpProfileProvider?.GetCurrentProfile();

        public float ControlInfluenceTimer => _controlInfluenceTimer;

        public float WallJumpCoyoteTimer => _wallJumpCoyoteTimer;

        public float WallJumpBufferTimer => _wallJumpBufferTimer;

        private void Awake()
        {
            _motor = GetComponent<CharacterMotor>();
            _groundDetector = GetComponent<GroundDetector>();
            _wallDetector = GetComponent<WallDetector>();
            _wallSlideFeature = GetComponent<WallSlideFeature>();
            _input = GetComponent<ICharacterInput>();
            _controller = GetComponent<CharacterController2D>();
            _dashFeature = GetComponent<DashFeature>();

            _wallJumpProfileProvider =
                _controller.WallJumpProfileProvider;
        }

        private void OnEnable()
        {
            if (defaultWallJumpProfile != null)
            {
                _wallJumpProfileProvider?.RegisterProfile(
                    defaultWallJumpProfile);
            }
        }

        private void OnDisable()
        {
            if (defaultWallJumpProfile != null)
            {
                _wallJumpProfileProvider?.UnregisterProfile(
                    defaultWallJumpProfile);
            }
        }

        public void Tick()
        {
            if (_input.JumpPressed && !_groundDetector.IsGrounded)
            {
                _wallJumpRequested = true;
            }
        }

        public void FixedTick()
        {
            WallJumpProfile currentWallJumpProfile =
                _wallJumpProfileProvider?.GetCurrentProfile();

            if (currentWallJumpProfile == null)
                return;

            UpdateTimers();
            TryWallJump(currentWallJumpProfile);
            ApplyControlInfluence(currentWallJumpProfile);
        }

        private void UpdateTimers()
        {
            if (_controlInfluenceTimer > 0f)
            {
                _controlInfluenceTimer -= Time.fixedDeltaTime;
            }

            if (IsWallJumpContactActive())
            {
                _wallJumpCoyoteTimer =
                    Mathf.Max(wallJumpCoyoteTime, Time.fixedDeltaTime);

                _lastWallJumpNormal = _wallDetector.WallNormal;
            }
            else
            {
                _wallJumpCoyoteTimer =
                    Mathf.Max(
                        0f,
                        _wallJumpCoyoteTimer - Time.fixedDeltaTime);
            }

            if (_wallJumpRequested)
            {
                _wallJumpBufferTimer =
                    Mathf.Max(wallJumpBufferTime, Time.fixedDeltaTime);

                _wallJumpRequested = false;
            }
            else
            {
                _wallJumpBufferTimer =
                    Mathf.Max(
                        0f,
                        _wallJumpBufferTimer - Time.fixedDeltaTime);
            }
        }

        private void TryWallJump(
            WallJumpProfile currentWallJumpProfile)
        {
            if (_wallJumpBufferTimer <= 0f)
                return;

            if (_wallJumpCoyoteTimer <= 0f)
                return;

            Vector2 wallNormal =
                IsWallJumpContactActive()
                    ? _wallDetector.WallNormal
                    : _lastWallJumpNormal;

            if (wallNormal == Vector2.zero)
                return;

            if (_dashFeature != null &&
                _dashFeature.IsDashing &&
                !_dashFeature.TryInterruptDash())
            {
                return;
            }

            _wallJumpImpulseX =
                wallNormal.x *
                currentWallJumpProfile.horizontalForce;

            float influencedVelocityX =
                CalculateInfluencedHorizontalVelocity(
                    currentWallJumpProfile);

            Vector2 velocity = new Vector2(
                influencedVelocityX,
                currentWallJumpProfile.verticalForce);

            _motor.SetSelfVelocity(velocity);

            _controlInfluenceTimer =
                currentWallJumpProfile.controlInfluenceDuration;

            _wallJumpBufferTimer = 0f;
            _wallJumpCoyoteTimer = 0f;

            WallJumped?.Invoke();
        }

        private void ApplyControlInfluence(
            WallJumpProfile currentWallJumpProfile)
        {
            if (_controlInfluenceTimer <= 0f)
                return;

            float influencedVelocityX =
                CalculateInfluencedHorizontalVelocity(
                    currentWallJumpProfile);

            _motor.SetHorizontalSelfVelocity(influencedVelocityX);
        }

        private float CalculateInfluencedHorizontalVelocity(
            WallJumpProfile currentWallJumpProfile)
        {
            float inputX =
                Mathf.Clamp(_input.HorizontalMoveInput, -1f, 1f);

            if (Mathf.Abs(inputX) < 0.01f)
            {
                inputX = 0f;
            }

            float inputVelocityX =
                inputX *
                currentWallJumpProfile.horizontalForce;

            float influencedVelocityX =
                Mathf.Lerp(
                    _wallJumpImpulseX,
                    inputVelocityX,
                    currentWallJumpProfile.horizontalInputInfluence);

            return influencedVelocityX;
        }

        private bool IsWallJumpContactActive()
        {
            if (requireWallSlide)
            {
                return _wallSlideFeature != null &&
                       _wallSlideFeature.IsWallSliding;
            }

            return _wallDetector != null &&
                   _wallDetector.IsTouchingWall;
        }
    }
}
