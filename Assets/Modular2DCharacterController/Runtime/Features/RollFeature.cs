using Modular2DCharacterController.Runtime.Core;
using Modular2DCharacterController.Runtime.Data.FeatureProfiles;
using Modular2DCharacterController.Runtime.Input;
using System;
using UnityEngine;

namespace Modular2DCharacterController.Runtime.Features
{
    /// <summary>
    /// A configurable feature that allows the character to roll when grounded.
    ///
    /// Uses the Roll Profile data to calculate roll force.
    /// </summary>
    [RequireComponent(typeof(CharacterController2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(GroundDetector))]
    [RequireComponent(typeof(LedgeDetector))]
    public class RollFeature : MonoBehaviour, ICharacterFeature
    {
        [Header("Default Roll Profile")]

        [Tooltip("Default roll profile registered when this feature initializes.")]
        [SerializeField]
        private RollProfile defaultRollProfile;

        [Header("Direction")]

        [Tooltip("If enabled, the roll direction is determined from the current movement input.")]
        [SerializeField]
        private bool useInputDirection = true;

        [Tooltip(
            "If enabled and no valid input direction is available, the roll will use " +
            "the character's facing direction instead.")]
        [SerializeField]
        private bool fallbackToFacingDirection = true;

        [Header("Interruptions")]

        [Tooltip(
            "If enabled, other features can interrupt an active roll.")]
        [SerializeField]
        private bool canBeInterrupted = true;

        [Header("Roll Hit Detection")]

        [Tooltip("Layers that can be reported by RollHit.")]
        [SerializeField]
        private LayerMask rollHitLayers = ~0;

        [Tooltip(
            "Extra cast distance added to roll hit detection. " +
            "Small values help catch contacts at high speed.")]
        [SerializeField]
        [Min(0f)]
        private float rollHitSkin = 0.02f;

        [Header("Ground Edge Detection")]

        [Tooltip(
            "If enabled, the roll ends immediately when the character leaves the ground. " +
            "If disabled, the roll continues over edges until its duration ends or it is interrupted.")]
        [SerializeField]
        private bool stopWhenLeavingGround = true;

        public bool IsRolling { get; private set; }

        public event Action<float> Rolled;
        public event Action<CharacterHitEvent> RollHit;
        public event Action RollEnded;

        public RollProfile CurrentRollProfile =>
            _rollProfileProvider?.GetCurrentProfile();

        public Vector2 RollDirection => _rollDirection;

        public float RollTimer => _rollTimer;

        public float CooldownTimer => _cooldownTimer;

        public bool StopWhenLeavingGround => stopWhenLeavingGround;

        public bool CanInterruptCurrentRoll =>
            IsRolling &&
            canBeInterrupted;

        private CharacterController2D _controller;
        private CharacterMotor _motor;
        private Collider2D _collider;
        private ICharacterInput _input;
        private GroundDetector _groundDetector;
        private LedgeDetector _ledgeDetector;
        private HorizontalMovementFeature _horizontalMovementFeature;
        private DashFeature _dashFeature;
        private GroundPoundFeature _groundPoundFeature;
        private ProfileProvider<RollProfile> _rollProfileProvider;
        private ContactFilter2D _rollHitFilter;

        private Vector2 _rollDirection;
        private FacingDirection _lastFacingDirection;
        private float _rollImpulseX;
        private float _rollTimer;
        private float _cooldownTimer;
        private bool _rollRequested;
        private Collider2D _lastRollHitCollider;

        private readonly RaycastHit2D[] _rollHitResults =
            new RaycastHit2D[8];

        private void Awake()
        {
            _controller = GetComponent<CharacterController2D>();
            _motor = GetComponent<CharacterMotor>();
            _collider = GetComponent<Collider2D>();
            _input = GetComponent<ICharacterInput>();
            _groundDetector = GetComponent<GroundDetector>();
            _ledgeDetector = GetComponent<LedgeDetector>();
            _horizontalMovementFeature = GetComponent<HorizontalMovementFeature>();
            _dashFeature = GetComponent<DashFeature>();
            _groundPoundFeature = GetComponent<GroundPoundFeature>();
            _rollProfileProvider = _controller.RollProfileProvider;

            _rollHitFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = rollHitLayers,
                useTriggers = false
            };

            _lastFacingDirection =
                _horizontalMovementFeature != null
                    ? _horizontalMovementFeature.FacingDirection
                    : FacingDirection.Right;
        }

        private void OnEnable()
        {
            if (defaultRollProfile != null)
            {
                _rollProfileProvider.RegisterProfile(defaultRollProfile);
            }
        }

        private void OnDisable()
        {
            RollProfile currentProfile =
                _rollProfileProvider.GetCurrentProfile();

            _rollProfileProvider?.UnregisterProfile(defaultRollProfile);

            if (IsRolling)
            {
                EndRoll(currentProfile, true);
            }
        }

        public void Tick()
        {
            if (_input != null && _input.RollPressed)
            {
                _rollRequested = true;
            }
        }

        public void FixedTick()
        {
            RollProfile currentProfile =
                _rollProfileProvider.GetCurrentProfile();

            if (currentProfile == null)
                return;

            UpdateFacingMemory();
            UpdateTimers();

            if (IsRolling)
            {
                ContinueRoll(currentProfile);
                return;
            }

            TryStartRoll(currentProfile);
        }

        private void UpdateFacingMemory()
        {
            if (_horizontalMovementFeature != null)
            {
                _lastFacingDirection = _horizontalMovementFeature.FacingDirection;
                return;
            }

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
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.fixedDeltaTime;
            }

            if (IsRolling)
            {
                _rollTimer -= Time.fixedDeltaTime;
            }
        }

        private void TryStartRoll(RollProfile currentProfile)
        {
            if (!_rollRequested)
                return;

            _rollRequested = false;

            if (_cooldownTimer > 0f)
                return;

            if (_groundDetector == null ||
                !_groundDetector.IsGrounded)
            {
                return;
            }

            if (_groundPoundFeature != null &&
                _groundPoundFeature.IsRecoveryActive)
            {
                return;
            }

            if (_dashFeature != null &&
                _dashFeature.IsDashing &&
                !_dashFeature.TryInterruptDash())
            {
                return;
            }

            Vector2 direction =
                GetRollDirection();

            if (direction == Vector2.zero)
                return;

            _rollDirection = direction.normalized;
            _rollImpulseX =
                _rollDirection.x *
                currentProfile.rollSpeed;

            _rollTimer = currentProfile.rollDuration;
            IsRolling = true;
            _lastRollHitCollider = null;

            _motor.SetSelfVelocity(new Vector2(_rollImpulseX, 0f));
            CheckRollHit(currentProfile, _rollImpulseX);
            Rolled?.Invoke(currentProfile.rollSpeed);
        }
        
        public bool TryInterruptRoll()
        {
            if (!CanInterruptCurrentRoll)
                return false;

            RollProfile currentProfile =
                _rollProfileProvider.GetCurrentProfile();

            EndRoll(currentProfile, false);
            return true;
        }

        private Vector2 GetRollDirection()
        {
            if (useInputDirection &&
                _input != null &&
                Mathf.Abs(_input.HorizontalMoveInput) > 0.01f)
            {
                return new Vector2(Mathf.Sign(_input.HorizontalMoveInput), 0f);
            }

            if (fallbackToFacingDirection)
            {
                return new Vector2((int)_lastFacingDirection, 0f);
            }

            return Vector2.zero;
        }

        private void ContinueRoll(RollProfile currentProfile)
        {
            bool isGrounded =
                _groundDetector != null &&
                _groundDetector.IsGrounded;

            if (_groundDetector == null ||
                (!isGrounded && stopWhenLeavingGround))
            {
                EndRoll(currentProfile, true);
                return;
            }

            if (isGrounded &&
                stopWhenLeavingGround &&
                ShouldStopBeforeGroundEdge())
            {
                EndRollAtEdge(currentProfile);
                return;
            }

            CheckRollHit(currentProfile, _rollImpulseX);

            float minimumRollDuration =
                Mathf.Min(
                    currentProfile.minimumRollDuration,
                    currentProfile.rollDuration);

            bool minimumDurationComplete =
                _rollTimer <= currentProfile.rollDuration - minimumRollDuration;

            if (currentProfile.variableRollLength &&
                minimumDurationComplete &&
                _input != null &&
                !_input.RollHeld)
            {
                EndRoll(currentProfile, true);
                return;
            }

            if (_rollTimer <= 0f)
            {
                EndRoll(currentProfile, true);
                return;
            }

            if (isGrounded || currentProfile.applyGravity)
            {
                _motor.SetHorizontalSelfVelocity(_rollImpulseX);
            }
            else
            {
                _motor.SetSelfVelocity(new Vector2(_rollImpulseX, 0f));
                _motor.SuppressGravityThisFrame();
            }
        }

        private void EndRoll(RollProfile currentProfile, bool applyExitVelocity)
        {
            IsRolling = false;
            _lastRollHitCollider = null;

            _cooldownTimer =
                currentProfile != null
                    ? currentProfile.rollCooldown
                    : 0f;

            if (!applyExitVelocity || currentProfile == null)
            {
                RollEnded?.Invoke();
                return;
            }

            Vector2 exitVelocity =
                _motor.CurrentSelfVelocity;

            if (currentProfile.preserveRollMomentum)
            {
                exitVelocity.x =
                    _rollDirection.x *
                    currentProfile.rollSpeed *
                    currentProfile.endingMomentumMultiplier;
            }
            else
            {
                exitVelocity.x = 0f;
            }

            if (currentProfile.clearVerticalVelocityOnEnd)
            {
                exitVelocity.y = 0f;
            }

            _motor.SetSelfVelocity(exitVelocity);
            RollEnded?.Invoke();
        }

        private bool ShouldStopBeforeGroundEdge()
        {
            if (_ledgeDetector == null)
                return false;

            if (Mathf.Abs(_rollImpulseX) < 0.01f)
                return false;

            return _ledgeDetector.IsOnGroundEdge;
        }

        private void EndRollAtEdge(RollProfile currentProfile)
        {
            IsRolling = false;
            _lastRollHitCollider = null;

            _cooldownTimer =
                currentProfile != null
                    ? currentProfile.rollCooldown
                    : 0f;

            _motor.SetHorizontalSelfVelocity(0f);
            RollEnded?.Invoke();
        }

        private void CheckRollHit(
            RollProfile currentProfile,
            float horizontalVelocity)
        {
            if (_collider == null)
                return;

            if (Mathf.Abs(horizontalVelocity) < 0.01f)
                return;

            Vector2 castDirection =
                new(Mathf.Sign(horizontalVelocity), 0f);

            float castDistance =
                Mathf.Abs(horizontalVelocity) *
                Time.fixedDeltaTime +
                rollHitSkin;

            int hitCount =
                _collider.Cast(
                    castDirection,
                    _rollHitFilter,
                    _rollHitResults,
                    castDistance);

            RaycastHit2D bestHit = default;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D hit =
                    _rollHitResults[i];

                if (!IsValidRollHit(hit))
                    continue;

                if (bestHit.collider == null ||
                    hit.distance < bestHit.distance)
                {
                    bestHit = hit;
                }
            }

            if (bestHit.collider == null)
                return;

            if (bestHit.collider == _lastRollHitCollider)
                return;

            _lastRollHitCollider = bestHit.collider;

            CharacterHitEvent hitEvent =
                CreateRollHitEvent(bestHit);

            RollHit?.Invoke(hitEvent);

            if (bestHit.collider.TryGetComponent(out IRollHitReceiver receiver))
            {
                receiver.OnRollHit(hitEvent);
            }
        }

        private bool IsValidRollHit(RaycastHit2D hit)
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

        private CharacterHitEvent CreateRollHitEvent(RaycastHit2D hit)
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

    }
}
