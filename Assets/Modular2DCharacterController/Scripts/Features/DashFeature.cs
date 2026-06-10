using Modular2DCharacterController.Scripts.Core;
using Modular2DCharacterController.Scripts.Data;
using Modular2DCharacterController.Scripts.Input;
using UnityEngine;

namespace Modular2DCharacterController.Scripts.Features
{
    /// <summary>
    /// A configurable 2D dash feature using DashProfile data.
    /// </summary>
    [RequireComponent(typeof(CharacterController2D))]
    public class DashFeature : MonoBehaviour, ICharacterFeature
    {
        [Header("Default Dash Profile")]
        // Default dash profile registered when this feature wakes up.
        [SerializeField]
        private DashProfile defaultDashProfile;

        // True while the dash is actively controlling velocity.
        // Other features can read this to skip movement or gravity during dash.
        public bool IsDashing { get; private set; }

        private CharacterController2D _controller;
        private CharacterMotor _motor;
        private ICharacterInput _input;
        private GroundDetector _groundDetector;
        private HorizontalMovementFeature _horizontalMovementFeature;
        private ProfileProvider<DashProfile> _dashProfileProvider;

        // Direction chosen when the dash starts.
        private Vector2 _dashDirection;

        // Last known character facing direction.
        // Used when the player presses dash without holding movement input.
        private FacingDirection _lastFacingDirection;

        // Remaining time for the active dash.
        private float _dashTimer;

        // Remaining time before another dash is allowed.
        private float _cooldownTimer;

        // Number of dashes still available before reset.
        private int _remainingDashes;

        // Buffered dash input.
        // DashPressed is frame-based, so it is captured in Tick and consumed in FixedTick.
        private bool _dashRequested;

        private void Awake()
        {
            _controller = GetComponent<CharacterController2D>();
            _motor = GetComponent<CharacterMotor>();
            _input = GetComponent<ICharacterInput>();
            _groundDetector = GetComponent<GroundDetector>();
            _horizontalMovementFeature = GetComponent<HorizontalMovementFeature>();

            _dashProfileProvider = _controller.DashProfileProvider;

            // Register the default profile so this feature has dash tuning data.
            if (defaultDashProfile != null)
            {
                _dashProfileProvider.RegisterProfile(defaultDashProfile);
                _remainingDashes = defaultDashProfile.maxDashCount;
            }
            else
            {
                _remainingDashes = 0;
            }

            // Initialize facing from the character's actual facing state if available.
            _lastFacingDirection =
                _horizontalMovementFeature != null
                    ? _horizontalMovementFeature.FacingDirection
                    : FacingDirection.Right;
        }

        public void Tick()
        {
            // Capture dash input during Update.
            if (_input != null && _input.DashPressed)
            {
                _dashRequested = true;
            }
        }

        public void FixedTick()
        {
            DashProfile currentProfile =
                _dashProfileProvider.GetCurrentProfile();

            if (currentProfile == null)
                return;

            UpdateFacingMemory();
            UpdateTimers();
            ResetDashCountIfGrounded(currentProfile);

            // While dashing, keep applying dash velocity and skip start checks.
            if (IsDashing)
            {
                ContinueDash(currentProfile);
                return;
            }

            TryStartDash(currentProfile);
        }

        private void UpdateFacingMemory()
        {
            // Prefer the facing direction calculated by HorizontalMovementFeature.
            if (_horizontalMovementFeature != null)
            {
                _lastFacingDirection = _horizontalMovementFeature.FacingDirection;
                return;
            }

            // Fallback for characters without HorizontalMovementFeature.
            if (_input != null && Mathf.Abs(_input.MoveInput) > 0.01f)
            {
                _lastFacingDirection =
                    _input.MoveInput > 0f
                        ? FacingDirection.Right
                        : FacingDirection.Left;
            }
        }

        private void UpdateTimers()
        {
            // Cooldown counts down whether grounded or airborne.
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.fixedDeltaTime;
            }

            // Dash duration only counts down while a dash is active.
            if (IsDashing)
            {
                _dashTimer -= Time.fixedDeltaTime;
            }
        }

        private void ResetDashCountIfGrounded(DashProfile currentProfile)
        {
            if (!currentProfile.resetDashCountOnGround)
                return;

            // Restore dash charges when grounded, but not during the dash itself.
            if (_groundDetector != null &&
                _groundDetector.IsGrounded &&
                !IsDashing)
            {
                _remainingDashes = currentProfile.maxDashCount;
            }
        }

        private void TryStartDash(DashProfile currentProfile)
        {
            // Consume buffered dash input.
            if (!_dashRequested)
                return;

            _dashRequested = false;

            // Do not start a dash during cooldown.
            if (_cooldownTimer > 0f)
                return;

            // Do not start if no dash charges remain.
            if (_remainingDashes <= 0)
                return;

            bool isGrounded =
                _groundDetector != null && _groundDetector.IsGrounded;

            // Respect profile permissions for ground and air dashing.
            if (isGrounded && !currentProfile.allowGroundDash)
                return;

            if (!isGrounded && !currentProfile.allowAirDash)
                return;

            Vector2 direction = GetDashDirection(currentProfile);

            // If the profile does not allow any valid direction, cancel dash.
            if (direction == Vector2.zero)
                return;

            _remainingDashes--;
            _dashDirection = direction.normalized;
            _dashTimer = currentProfile.dashDuration;
            IsDashing = true;

            // Set velocity immediately so dash starts on this physics tick.
            _motor.SetVelocity(_dashDirection * currentProfile.dashSpeed);
        }

        private Vector2 GetDashDirection(DashProfile currentProfile)
        {
            // Use current movement input when allowed and available.
            if (currentProfile.useInputDirection &&
                _input != null &&
                Mathf.Abs(_input.MoveInput) > 0.01f)
            {
                return new Vector2(Mathf.Sign(_input.MoveInput), 0f);
            }

            // If no movement input is held, dash toward the last known facing direction.
            if (currentProfile.fallbackToFacingDirection)
            {
                return new Vector2((int)_lastFacingDirection, 0f);
            }

            // No valid direction.
            return Vector2.zero;
        }

        private void ContinueDash(DashProfile currentProfile)
        {
            // Clamp minimum duration so it cannot exceed total dash duration.
            float minimumDashDuration =
                Mathf.Min(
                    currentProfile.minimumDashDuration,
                    currentProfile.dashDuration);

            // Once this is true, the dash is allowed to end early if configured.
            bool minimumDurationComplete =
                _dashTimer <= currentProfile.dashDuration - minimumDashDuration;

            // Variable dash length:
            // after the minimum duration, releasing the button ends the dash early.
            if (
                currentProfile.variableDashLength &&
                minimumDurationComplete &&
                _input != null &&
                !_input.DashHeld)
            {
                EndDash(currentProfile);
                return;
            }

            // Fixed or fully-held dash ends when its timer expires.
            if (_dashTimer <= 0f)
            {
                EndDash(currentProfile);
                return;
            }

            // Reapply dash velocity every physics tick.
            // This is what lets dash override normal movement and gravity.
            _motor.SetVelocity(_dashDirection * currentProfile.dashSpeed);
        }

        private void EndDash(DashProfile currentProfile)
        {
            IsDashing = false;
            _cooldownTimer = currentProfile.dashCooldown;

            Vector2 exitVelocity = _motor.Velocity;

            // Optionally keep part of the dash speed after dash ends.
            if (currentProfile.preserveDashMomentum)
            {
                exitVelocity.x =
                    _dashDirection.x *
                    currentProfile.dashSpeed *
                    currentProfile.endingMomentumMultiplier;
            }
            else
            {
                exitVelocity.x = 0f;
            }

            // Optional vertical cleanup for flat platformer dashes.
            if (currentProfile.clearVerticalVelocityOnEnd)
            {
                exitVelocity.y = 0f;
            }

            _motor.SetVelocity(exitVelocity);
        }
    }
}