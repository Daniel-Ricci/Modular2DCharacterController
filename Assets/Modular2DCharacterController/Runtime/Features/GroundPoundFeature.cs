using System;
using Modular2DCharacterController.Runtime.Core;
using Modular2DCharacterController.Runtime.Data.FeatureProfiles;
using Modular2DCharacterController.Runtime.Input;
using UnityEngine;

namespace Modular2DCharacterController.Runtime.Features
{
    /// <summary>
    /// A configurable feature that lets the character slam downward while airborne.
    /// </summary>
    [RequireComponent(typeof(CharacterController2D))]
    [RequireComponent(typeof(GroundDetector))]
    public class GroundPoundFeature : MonoBehaviour, ICharacterFeature
    {
        private enum GroundPoundState
        {
            None,
            StillInAir,
            Descending
        }

        [Header("Default Ground Pound Profile")]

        [Tooltip(
            "The default ground pound profile registered when this feature initializes.")]
        [SerializeField]
        private GroundPoundProfile defaultGroundPoundProfile;

        [Header("Interruptions")]

        [Tooltip(
            "If enabled, jump, dash, or glide can interrupt the ground pound.")]
        [SerializeField]
        private bool canBeInterrupted = true;

        [Header("Recovery Jump")]

        [Tooltip(
            "If enabled, jump can be pressed during ground pound recovery. " +
            "The recovery jump profile is registered temporarily while recovery is active.")]
        [SerializeField]
        private bool allowJumpDuringRecovery = false;

        [Tooltip(
            "Jump profile used only during ground pound recovery. " +
            "Give this profile a higher priority than the normal jump profile.")]
        [SerializeField]
        private JumpProfile recoveryJumpProfile;

        [Header("Input Settings")]
        
        [Tooltip(
            "If enabled, use the down vertical input to ground pound and " +
            "ignores what is set at the GroundPound input mapping.")]
        [SerializeField]
        private bool useDownInput;
        
        [Tooltip(
            "Minimum vertical input needed to trigger a ground pound, if using down input.")]
        [SerializeField]
        private float minimumInput = 0.9f;

        // Invoked when the ground pound starts.
        public event Action GroundPoundStarted;

        // Invoked when the ground pound ends without hitting the ground.
        public event Action GroundPoundInterrupted;

        // Invoked when the ground pound finishes by hitting the ground.
        // The hit object is the grounded object detected by GroundDetector.
        public event Action<CharacterHitEvent> GroundPoundFinished;

        public bool IsGroundPounding { get; private set; }

        public bool IsRecoveryActive =>
            _recoveryTimer > 0f;

        public GroundPoundProfile CurrentGroundPoundProfile =>
            _groundPoundProfileProvider?.GetCurrentProfile();

        public bool CanInterruptCurrentGroundPound =>
            IsGroundPounding &&
            canBeInterrupted;

        public bool CanJumpDuringRecovery =>
            IsRecoveryActive &&
            allowJumpDuringRecovery &&
            recoveryJumpProfile != null;

        public bool TryInterruptGroundPound()
        {
            if (!CanInterruptCurrentGroundPound)
                return false;

            EndGroundPound(default, false);
            return true;
        }

        public bool TryConsumeRecoveryJump()
        {
            if (!CanJumpDuringRecovery)
                return false;

            _recoveryTimer = 0f;
            UnregisterRecoveryJumpProfile();
            return true;
        }

        public float StillInAirTimer => _stillInAirTimer;

        public float DescendTimer => _descendTimer;

        public float RecoveryTimer => _recoveryTimer;

        private CharacterMotor _motor;
        private GroundDetector _groundDetector;
        private CharacterController2D _controller;
        private ICharacterInput _input;
        private DashFeature _dashFeature;
        private ProfileProvider<GroundPoundProfile> _groundPoundProfileProvider;
        private ProfileProvider<JumpProfile> _jumpProfileProvider;

        private GroundPoundState _state;
        private float _stillInAirTimer;
        private float _descendTimer;
        private float _recoveryTimer;

        private bool _groundPoundRequested;
        private bool _jumpInterruptRequested;
        private bool _dashInterruptRequested;
        private bool _glideInterruptRequested;
        private bool _wasGlideInputHeld;
        private bool _recoveryJumpProfileRegistered;

        private void Awake()
        {
            _motor = GetComponent<CharacterMotor>();
            _groundDetector = GetComponent<GroundDetector>();
            _controller = GetComponent<CharacterController2D>();
            _input = GetComponent<ICharacterInput>();
            _dashFeature = GetComponent<DashFeature>();

            _groundPoundProfileProvider =
                _controller.GroundPoundProfileProvider;

            _jumpProfileProvider =
                _controller.JumpProfileProvider;
        }

        private void OnEnable()
        {
            if (defaultGroundPoundProfile != null)
            {
                _groundPoundProfileProvider?.RegisterProfile(
                    defaultGroundPoundProfile);
            }
        }

        private void OnDisable()
        {
            if (defaultGroundPoundProfile != null)
            {
                _groundPoundProfileProvider?.UnregisterProfile(
                    defaultGroundPoundProfile);
            }

            if (IsGroundPounding)
            {
                EndGroundPound(default, false);
            }

            UnregisterRecoveryJumpProfile();
        }

        public void Tick()
        {
            if (useDownInput)
            {
                if (_input.VerticalMoveInput <= -minimumInput)
                {
                    _groundPoundRequested = true;
                }
            }
            else if (_input.GroundPoundPressed)
            {
                _groundPoundRequested = true;
            }

            if (_input.JumpPressed)
            {
                _jumpInterruptRequested = true;
            }

            if (_input.DashPressed)
            {
                _dashInterruptRequested = true;
            }

            bool glideInputHeld =
                _input.RunHeld;

            if (glideInputHeld && !_wasGlideInputHeld)
            {
                _glideInterruptRequested = true;
            }

            _wasGlideInputHeld =
                glideInputHeld;
        }

        public void FixedTick()
        {
            GroundPoundProfile currentProfile =
                CurrentGroundPoundProfile;

            if (currentProfile == null)
                return;

            UpdateRecoveryTimer();

            if (!IsGroundPounding)
            {
                TryStartGroundPound(currentProfile);
                ClearInterruptRequests();
                return;
            }

            if (ShouldInterrupt())
            {
                EndGroundPound(default, false);
                ClearInterruptRequests();
                return;
            }

            UpdateGroundPound(currentProfile);
            ClearInterruptRequests();
        }

        private void UpdateRecoveryTimer()
        {
            if (_recoveryTimer <= 0f)
                return;

            _recoveryTimer =
                Mathf.Max(
                    0f,
                    _recoveryTimer - Time.fixedDeltaTime);

            if (_recoveryTimer <= 0f)
            {
                UnregisterRecoveryJumpProfile();
            }
        }

        private void TryStartGroundPound(
            GroundPoundProfile currentProfile)
        {
            if (!_groundPoundRequested)
                return;

            _groundPoundRequested = false;

            if (IsRecoveryActive)
                return;

            if (_groundDetector.IsGrounded)
                return;

            if (_dashFeature != null &&
                _dashFeature.IsDashing &&
                !_dashFeature.TryInterruptDash())
            {
                return;
            }

            StartGroundPound(currentProfile);
        }

        private void StartGroundPound(
            GroundPoundProfile currentProfile)
        {
            IsGroundPounding = true;
            _recoveryTimer = 0f;
            UnregisterRecoveryJumpProfile();

            _stillInAirTimer =
                currentProfile.stillTimeBeforeDescending;

            _descendTimer =
                currentProfile.descendTime;

            _state =
                _stillInAirTimer > 0f
                    ? GroundPoundState.StillInAir
                    : GroundPoundState.Descending;

            Vector2 startingVelocity =
                _state == GroundPoundState.Descending
                    ? new Vector2(0f, -currentProfile.descendSpeed)
                    : Vector2.zero;

            _motor.SetSelfVelocity(startingVelocity);
            _motor.SuppressGravityThisFrame();

            GroundPoundStarted?.Invoke();
        }

        private void UpdateGroundPound(
            GroundPoundProfile currentProfile)
        {
            if (_groundDetector.IsGrounded)
            {
                CharacterHitEvent hitEvent =
                    CreateGroundPoundHitEvent();

                EndGroundPound(hitEvent, true);
                return;
            }

            switch (_state)
            {
                case GroundPoundState.StillInAir:
                    UpdateStillInAirState();
                    break;

                case GroundPoundState.Descending:
                    UpdateDescendingState(currentProfile);
                    break;
            }
        }

        private void UpdateStillInAirState()
        {
            _stillInAirTimer -= Time.fixedDeltaTime;

            _motor.SetSelfVelocity(Vector2.zero);
            _motor.SuppressGravityThisFrame();

            if (_stillInAirTimer <= 0f)
            {
                _state = GroundPoundState.Descending;
            }
        }

        private void UpdateDescendingState(
            GroundPoundProfile currentProfile)
        {
            if (currentProfile.descendTime > 0f)
            {
                _descendTimer -= Time.fixedDeltaTime;

                if (_descendTimer <= 0f)
                {
                    EndGroundPound(default, false);
                    return;
                }
            }

            _motor.SetVerticalSelfVelocity(
                -currentProfile.descendSpeed);

            _motor.SuppressGravityThisFrame();
        }

        private bool ShouldInterrupt()
        {
            if (!canBeInterrupted)
                return false;

            return _jumpInterruptRequested ||
                   _dashInterruptRequested ||
                   _glideInterruptRequested;
        }

        private void EndGroundPound(
            CharacterHitEvent hitEvent,
            bool hitGround)
        {
            GroundPoundProfile currentProfile =
                CurrentGroundPoundProfile;

            IsGroundPounding = false;
            _state = GroundPoundState.None;
            _stillInAirTimer = 0f;
            _descendTimer = 0f;
            _groundPoundRequested = false;

            if (hitGround && currentProfile != null)
            {
                _recoveryTimer =
                    currentProfile.timeBeforeCanMoveAgainIfHitGround;

                RegisterRecoveryJumpProfile();
            }
            else
            {
                _recoveryTimer = 0f;
                UnregisterRecoveryJumpProfile();
            }

            if (hitGround)
            {
                GroundPoundFinished?.Invoke(hitEvent);

                if (hitEvent.HitCollider != null &&
                    hitEvent.HitCollider.TryGetComponent(out IGroundPoundHitReceiver receiver))
                {
                    receiver.OnGroundPoundHit(hitEvent);
                }
            }
            else
            {
                GroundPoundInterrupted?.Invoke();
            }
        }

        private CharacterHitEvent CreateGroundPoundHitEvent()
        {
            Collider2D hitCollider =
                _groundDetector.CurrentGroundCollider;

            Rigidbody2D hitRigidbody =
                hitCollider != null
                    ? hitCollider.attachedRigidbody
                    : null;

            GameObject hitObject =
                hitCollider != null
                    ? hitCollider.gameObject
                    : _groundDetector.CurrentGroundTransform != null
                        ? _groundDetector.CurrentGroundTransform.gameObject
                        : null;

            return new CharacterHitEvent(
                hitObject,
                _groundDetector.GroundPoint,
                _groundDetector.GroundNormal,
                hitCollider,
                hitRigidbody,
                gameObject,
                _motor != null ? _motor.CurrentSelfVelocity : Vector2.zero);
        }

        private void ClearInterruptRequests()
        {
            _jumpInterruptRequested = false;
            _dashInterruptRequested = false;
            _glideInterruptRequested = false;
        }

        private void RegisterRecoveryJumpProfile()
        {
            if (_recoveryTimer <= 0f ||
                !allowJumpDuringRecovery ||
                recoveryJumpProfile == null ||
                _recoveryJumpProfileRegistered)
            {
                return;
            }

            _jumpProfileProvider?.RegisterProfile(recoveryJumpProfile);
            _recoveryJumpProfileRegistered = true;
        }

        private void UnregisterRecoveryJumpProfile()
        {
            if (!_recoveryJumpProfileRegistered)
                return;

            _jumpProfileProvider?.UnregisterProfile(recoveryJumpProfile);
            _recoveryJumpProfileRegistered = false;
        }
    }
}
