using Modular2DCharacterController.Runtime.Core;
using Modular2DCharacterController.Runtime.Data.FeatureProfiles;
using Modular2DCharacterController.Runtime.Input;
using System;
using UnityEngine;

namespace Modular2DCharacterController.Runtime.Features
{
    /// <summary>
    /// A configurable feature that allows the character to dash.
    ///
    /// Uses the Dash Profile data to calculate dash force.
    /// </summary>
    [RequireComponent(typeof(CharacterController2D))]
    [RequireComponent(typeof(Collider2D))]
    public class DashFeature : MonoBehaviour, ICharacterFeature
    {
        [Header("Default Dash Profile")]
        
        [Tooltip("Default dash profile registered when this feature initializes.")]
        [SerializeField]
        private DashProfile defaultDashProfile;
        
        [Header("Dash Count")]
        [Tooltip(
            "The maximum number of consecutive dashes that can be performed " +
            "before the dash count must be reset.")]
        [Min(1)]
        public int maxDashCount = 1;
        
        [Tooltip("Should reset dash count when grounded?.")]
        [SerializeField]
        private bool resetDashCountOnGrounded;
        
        [Tooltip("Should reset dash count when wall jump?.")]
        [SerializeField]
        private bool resetDashCountOnWallJump;
        
        [Header("Direction")]

        [Tooltip("If enabled, the dash direction is determined from the current movement input.")]
        [SerializeField]
        private bool useInputDirection = true;

        [Tooltip(
            "If enabled and no valid input direction is available, the dash will use " +
            "the character's facing direction instead.")]
        [SerializeField]
        private bool fallbackToFacingDirection = true;

        [Header("Interruptions")]

        [Tooltip(
            "If enabled, other features can interrupt an active dash.")]
        [SerializeField]
        private bool canBeInterrupted = true;

        [Header("Dash Hit Detection")]

        [Tooltip("Layers that can be reported by DashHit.")]
        [SerializeField]
        private LayerMask dashHitLayers = ~0;

        [Tooltip(
            "Extra cast distance added to dash hit detection. " +
            "Small values help catch contacts at high speed.")]
        [SerializeField]
        [Min(0f)]
        private float dashHitSkin = 0.02f;

        // True while the dash is actively controlling velocity.
        // Other features can read this to skip movement or gravity during dash.
        public bool IsDashing { get; private set; }
        
        // Event for dashing.
        // Uses the dash's velocity as parameter.
        public event Action<float> Dashed;
        
        // Event for dash collision.
        // Uses the dash's collision data as parameter.
        public event Action<CharacterHitEvent> DashHit;
        
        // Event for end of dash.
        public event Action DashEnded;

        public DashProfile CurrentDashProfile =>
            _dashProfileProvider?.GetCurrentProfile();

        public Vector2 DashDirection => _dashDirection;

        public float DashTimer => _dashTimer;

        public float CooldownTimer => _cooldownTimer;

        public int RemainingDashes => _remainingDashes;

        public bool CanInterruptCurrentDash
        {
            get
            {
                return IsDashing &&
                       canBeInterrupted;
            }
        }

        public bool TryInterruptDash()
        {
            if (!CanInterruptCurrentDash)
                return false;

            DashProfile currentProfile =
                _dashProfileProvider.GetCurrentProfile();

            EndDash(currentProfile, false);
            return true;
        }

        // Components used by this feature.
        private CharacterController2D _controller;
        private CharacterMotor _motor;
        private Collider2D _collider;
        private ICharacterInput _input;
        private GroundDetector _groundDetector;
        private HorizontalMovementFeature _horizontalMovementFeature;
        private RollFeature _rollFeature;
        private WallJumpFeature _wallJumpFeature;
        private GroundPoundFeature _groundPoundFeature;
        private ProfileProvider<DashProfile> _dashProfileProvider;
        private ContactFilter2D _dashHitFilter;

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
        private Collider2D _lastDashHitCollider;

        private readonly RaycastHit2D[] _dashHitResults =
            new RaycastHit2D[8];

        private void Awake()
        {
            _controller = GetComponent<CharacterController2D>();
            _motor = GetComponent<CharacterMotor>();
            _collider = GetComponent<Collider2D>();
            _input = GetComponent<ICharacterInput>();
            _groundDetector = GetComponent<GroundDetector>();
            _horizontalMovementFeature = GetComponent<HorizontalMovementFeature>();
            _rollFeature = GetComponent<RollFeature>();
            _wallJumpFeature = GetComponent<WallJumpFeature>();
            _groundPoundFeature = GetComponent<GroundPoundFeature>();
            _dashProfileProvider = _controller.DashProfileProvider;

            _dashHitFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = dashHitLayers,
                useTriggers = false
            };

            _remainingDashes = maxDashCount;

            // Initialize facing from the character's actual facing state if available.
            _lastFacingDirection =
                _horizontalMovementFeature != null
                    ? _horizontalMovementFeature.FacingDirection
                    : FacingDirection.Right;
        }
        
        private void OnEnable()
        {
            // Register the default profile so this feature has dash tuning data.
            if (defaultDashProfile != null)
            {
                _dashProfileProvider.RegisterProfile(defaultDashProfile);
            }
            else
            {
                _remainingDashes = 0;
            }

            if(_wallJumpFeature  != null)
                _wallJumpFeature.WallJumped += ResetDashCount;
        }

        private void OnDisable()
        {
            DashProfile currentProfile = _dashProfileProvider.GetCurrentProfile();
            _dashProfileProvider?.UnregisterProfile(defaultDashProfile);

            if (IsDashing)
            {
                EndDash(currentProfile, true);
            }
            
            if(_wallJumpFeature != null)
                _wallJumpFeature.WallJumped -= ResetDashCount;
        }

        public void Tick()
        {
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
            ResetDashCountIfGrounded();

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
            if (_input != null && Mathf.Abs(_input.HorizontalMoveInput) > 0.01f)
            {
                _lastFacingDirection =
                    _input.HorizontalMoveInput > 0f
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

        private void ResetDashCountIfGrounded()
        {
            if (!resetDashCountOnGrounded)
                return;

            // Restore dash charges when grounded, but not during the dash itself.
            if (_groundDetector != null &&
                _groundDetector.IsGrounded &&
                !IsDashing)
            {
                _remainingDashes = maxDashCount;
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

            if (_groundPoundFeature != null &&
                _groundPoundFeature.IsRecoveryActive)
            {
                return;
            }

            if (_groundPoundFeature != null &&
                _groundPoundFeature.IsGroundPounding &&
                !_groundPoundFeature.TryInterruptGroundPound())
            {
                return;
            }

            if (_rollFeature != null &&
                _rollFeature.IsRolling &&
                !_rollFeature.TryInterruptRoll())
            {
                return;
            }

            bool isGrounded =
                _groundDetector != null && _groundDetector.IsGrounded;

            // Respect profile permissions for ground and air dashing.
            if (isGrounded && !currentProfile.allowGroundDash)
                return;

            if (!isGrounded && !currentProfile.allowAirDash)
                return;

            Vector2 direction = GetDashDirection();

            // If the profile does not allow any valid direction, cancel dash.
            if (direction == Vector2.zero)
                return;

            _remainingDashes--;
            _dashDirection = direction.normalized;
            _dashTimer = currentProfile.dashDuration;
            IsDashing = true;
            _lastDashHitCollider = null;

            // Set velocity immediately so dash starts on this physics tick.
            _motor.SetSelfVelocity(_dashDirection * currentProfile.dashSpeed);
            CheckDashHit(currentProfile);
            Dashed?.Invoke(currentProfile.dashSpeed);
        }

        private Vector2 GetDashDirection()
        {
            // Use current movement input when allowed and available.
            if (useInputDirection &&
                _input != null &&
                Mathf.Abs(_input.HorizontalMoveInput) > 0.01f)
            {
                return new Vector2(Mathf.Sign(_input.HorizontalMoveInput), 0f);
            }

            // If no movement input is held, dash toward the last known facing direction.
            if (fallbackToFacingDirection)
            {
                return new Vector2((int)_lastFacingDirection, 0f);
            }

            // No valid direction.
            return Vector2.zero;
        }

        private void ContinueDash(DashProfile currentProfile)
        {
            CheckDashHit(currentProfile);

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
                EndDash(currentProfile, true);
                return;
            }

            // Fixed or fully-held dash ends when its timer expires.
            if (_dashTimer <= 0f)
            {
                EndDash(currentProfile, true);
                return;
            }

            // Reapply dash velocity every physics tick.
            if (currentProfile.applyGravity)
            {
                _motor.SetHorizontalSelfVelocity(_dashDirection.x > 0 ?
                    currentProfile.dashSpeed : -currentProfile.dashSpeed);
            }
            else
            {
                _motor.SetSelfVelocity(_dashDirection * currentProfile.dashSpeed);
                _motor.SuppressGravityThisFrame();
            }
        }

        private void EndDash(DashProfile currentProfile, bool applyExitVelocity)
        {
            IsDashing = false;
            _lastDashHitCollider = null;

            _cooldownTimer =
                currentProfile != null
                    ? currentProfile.dashCooldown
                    : 0f;

            if (!applyExitVelocity || currentProfile == null)
            {
                DashEnded?.Invoke();
                return;
            }

            Vector2 exitVelocity = _motor.CurrentSelfVelocity;

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

            _motor.SetSelfVelocity(exitVelocity);
            DashEnded?.Invoke();
        }

        private void CheckDashHit(DashProfile currentProfile)
        {
            if (_collider == null)
                return;

            if (_dashDirection == Vector2.zero)
                return;

            float castDistance =
                currentProfile.dashSpeed *
                Time.fixedDeltaTime +
                dashHitSkin;

            int hitCount =
                _collider.Cast(
                    _dashDirection,
                    _dashHitFilter,
                    _dashHitResults,
                    castDistance);

            RaycastHit2D bestHit = default;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D hit =
                    _dashHitResults[i];

                if (!IsValidDashHit(hit))
                    continue;

                if (bestHit.collider == null ||
                    hit.distance < bestHit.distance)
                {
                    bestHit = hit;
                }
            }

            if (bestHit.collider == null)
                return;

            if (bestHit.collider == _lastDashHitCollider)
                return;

            _lastDashHitCollider = bestHit.collider;

            CharacterHitEvent hitEvent =
                CreateDashHitEvent(bestHit);

            DashHit?.Invoke(hitEvent);

            if (bestHit.collider.TryGetComponent(out IDashHitReceiver receiver))
            {
                receiver.OnDashHit(hitEvent);
            }
        }

        private bool IsValidDashHit(RaycastHit2D hit)
        {
            if (hit.collider == null)
                return false;

            if (hit.collider == _collider)
                return false;

            if (hit.collider.transform == transform ||
                hit.collider.transform.IsChildOf(transform))
            {
                return false;
            }

            return true;
        }

        private CharacterHitEvent CreateDashHitEvent(RaycastHit2D hit)
        {
            return new CharacterHitEvent(
                hit.collider != null ? hit.collider.gameObject : null,
                hit.point,
                hit.normal,
                hit.collider,
                hit.rigidbody,
                gameObject,
                _motor != null ? _motor.CurrentSelfVelocity : Vector2.zero);
        }

        private void ResetDashCount()
        {
            _remainingDashes = maxDashCount;
        }
    }
}
