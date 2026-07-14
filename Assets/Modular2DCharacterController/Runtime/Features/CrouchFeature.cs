using Modular2DCharacterController.Runtime.Core;
using Modular2DCharacterController.Runtime.Data.FeatureProfiles;
using Modular2DCharacterController.Runtime.Input;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modular2DCharacterController.Runtime.Features
{
    public enum CrouchMode
    {
        Hold,
        Toggle
    }

    /// <summary>
    /// A configurable feature that handles crouch state, crouch movement,
    /// collider resizing, and stand-up obstruction checks.
    /// </summary>
    [RequireComponent(typeof(CharacterController2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(CeilingDetector))]
    public class CrouchFeature : MonoBehaviour, ICharacterFeature
    {
        [Header("Crouch Profile")]

        [Tooltip(
            "Movement profile that overrides other movement profiles while " +
            "crouching. Useful for reducing speed, acceleration, etc.")]
        [SerializeField]
        private HorizontalMovementProfile crouchMovementProfile;

        [Header("Crouch Settings")]

        [Tooltip(
            "Determines whether crouching behaves as a hold action " +
            "or a toggle action.")]
        [SerializeField]
        private CrouchMode crouchMode = CrouchMode.Hold;

        [Tooltip(
            "If enabled, crouching can only occur while grounded.")]
        [SerializeField]
        private bool groundedOnly = true;

        [Tooltip(
            "If enabled, crouching requires a minimum amount of movement input.")]
        [SerializeField]
        private bool requireMovementInput = false;

        [Tooltip(
            "Minimum absolute movement input required when " +
            "'Require Movement Input' is enabled.")]
        [SerializeField]
        [Range(0f, 1f)]
        private float minimumMoveInput = 0.1f;

        [Tooltip(
            "When using Toggle mode, automatically clears the toggle state " +
            "when the character leaves the ground.")]
        [SerializeField]
        private bool clearToggleWhenLeavingGround;

        [Header("Collider Resize")]

        [Tooltip(
            "If enabled, this feature resizes the character's main collider while crouching.")]
        [SerializeField]
        private bool resizeCollider = true;

        [Tooltip(
            "The collider height multiplier used while crouching. " +
            "A value of 0.5 means the crouched collider is half as tall.")]
        [SerializeField]
        [Range(0.1f, 1f)]
        private float crouchedHeightMultiplier = 0.5f;

        [Tooltip(
            "If enabled, the bottom of the collider stays in the same position " +
            "when crouching and standing.")]
        [SerializeField]
        private bool preserveColliderBottom = true;

        // True while the player is crouching
        public bool IsCrouching { get; private set; }

        public bool IsStandBlocked { get; private set; }

        // Events for starting and stopping crouch.
        public event Action CrouchStarted;
        public event Action CrouchEnded;

        // Invoked when the character tries to stand but there is not enough room.
        public event Action CrouchStandBlocked;

        // Invoked after the collider changes shape. Parameter is true while crouching.
        public event Action<bool> CrouchColliderChanged;
        
        private CharacterController2D _controller;
        private ICharacterInput _input;
        private GroundDetector _groundDetector;
        private CeilingDetector _ceilingDetector;
        private GroundPoundFeature _groundPoundFeature;
        private ProfileProvider<HorizontalMovementProfile> _profileProvider;
        private Collider2D _collider;
        private CapsuleCollider2D _capsuleCollider;
        private BoxCollider2D _boxCollider;

        // Internal state used when operating in Toggle mode.
        private bool _toggleCrouchState;
        private Vector2 _standingSize;
        private Vector2 _standingOffset;
        private Vector2 _crouchedSize;
        private Vector2 _crouchedOffset;
        private int _pendingPassThroughOneWayCount;
        private readonly Collider2D[] _pendingPassThroughOneWayPlatforms = new Collider2D[8];
        private readonly List<Collider2D> _temporarilyIgnoredOneWayPlatforms = new();
        
        private void Awake()
        {
            _controller = GetComponent<CharacterController2D>();
            _input = GetComponent<ICharacterInput>();
            _groundDetector = GetComponent<GroundDetector>();
            _ceilingDetector = GetComponent<CeilingDetector>();
            _groundPoundFeature = GetComponent<GroundPoundFeature>();
            _collider = GetComponent<Collider2D>();
            _capsuleCollider = _collider as CapsuleCollider2D;
            _boxCollider = _collider as BoxCollider2D;

            _profileProvider = _controller.HorizontalMovementProfileProvider;

            CacheColliderShapes();
        }
        
        private void OnEnable()
        {
            if (_groundDetector != null)
            {
                _groundDetector.LeftGround += OnLeftGround;
            }
        }
        
        private void OnDisable()
        {
            if (_groundDetector != null)
            {
                _groundDetector.LeftGround -= OnLeftGround;
            }
            
            _profileProvider?.UnregisterProfile(crouchMovementProfile);

            // If the feature is disabled while crouching,
            // transition back to a non-crouching state.
            if (IsCrouching)
            {
                ApplyStandingCollider();
                IsCrouching = false;
                IsStandBlocked = false;
                CrouchEnded?.Invoke();
            }

            RestoreTemporaryOneWayPlatformIgnores();
        }
        
        public void Tick()
        {
            UpdateToggleInput();
        }
        
        public void FixedTick()
        {
            UpdateCrouchState();
            UpdateTemporaryOneWayPlatformIgnores();
        }
        
        private void UpdateToggleInput()
        {
            if (_input == null)
                return;

            if (_groundPoundFeature != null &&
                (_groundPoundFeature.IsGroundPounding ||
                 _groundPoundFeature.IsRecoveryActive))
            {
                _toggleCrouchState = false;
                return;
            }

            if (crouchMode != CrouchMode.Toggle)
                return;

            if (_input.CrouchPressed)
            {
                _toggleCrouchState = !_toggleCrouchState;
            }
        }
        
        private void UpdateCrouchState()
        {
            bool wantsToCrouch =
                WantsToCrouch();

            if (wantsToCrouch)
            {
                if (!IsCrouching && CanStartCrouch())
                {
                    EnterCrouch();
                }

                return;
            }

            if (!IsCrouching)
                return;

            bool wasStandBlocked =
                IsStandBlocked;

            if (CanStand())
            {
                ExitCrouch();
                return;
            }

            if (crouchMode == CrouchMode.Toggle)
            {
                _toggleCrouchState = true;
            }

            if (!wasStandBlocked)
            {
                CrouchStandBlocked?.Invoke();
            }
        }

        private bool WantsToCrouch()
        {
            if (_input == null)
                return false;

            if (_groundPoundFeature != null &&
                (_groundPoundFeature.IsGroundPounding ||
                 _groundPoundFeature.IsRecoveryActive))
            {
                return false;
            }

            return crouchMode switch
            {
                CrouchMode.Hold => _input.CrouchHeld,
                CrouchMode.Toggle => _toggleCrouchState,
                _ => false
            };
        }

        private bool CanStartCrouch()
        {
            if (_input == null)
                return false;

            if (_groundPoundFeature != null &&
                (_groundPoundFeature.IsGroundPounding ||
                 _groundPoundFeature.IsRecoveryActive))
            {
                return false;
            }

            // Optional grounded-only restriction.
            if (groundedOnly &&
                _groundDetector != null &&
                !_groundDetector.IsGrounded)
            {
                return false;
            }

            // Optional minimum movement input requirement.
            if (requireMovementInput &&
                Mathf.Abs(_input.HorizontalMoveInput) < minimumMoveInput)
            {
                return false;
            }

            return true;
        }

        private bool CanStand()
        {
            if (!resizeCollider)
            {
                IsStandBlocked = false;
                return true;
            }

            bool hasRoomToStand =
                HasRoomForStandingCollider();

            IsStandBlocked =
                !hasRoomToStand;

            return hasRoomToStand;
        }

        private void EnterCrouch()
        {
            IsCrouching = true;
            IsStandBlocked = false;

            ApplyCrouchedCollider();

            if (crouchMovementProfile != null)
            {
                _profileProvider.RegisterProfile(crouchMovementProfile);
            }

            CrouchStarted?.Invoke();
        }

        private void ExitCrouch()
        {
            IgnorePendingPassThroughOneWayPlatforms();

            ApplyStandingCollider();

            IsCrouching = false;
            IsStandBlocked = false;

            if (crouchMovementProfile != null)
            {
                _profileProvider.UnregisterProfile(crouchMovementProfile);
            }

            CrouchEnded?.Invoke();
        }

        private void CacheColliderShapes()
        {
            if (_capsuleCollider != null)
            {
                _standingSize = _capsuleCollider.size;
                _standingOffset = _capsuleCollider.offset;
            }
            else if (_boxCollider != null)
            {
                _standingSize = _boxCollider.size;
                _standingOffset = _boxCollider.offset;
            }
            else
            {
                return;
            }

            float crouchedHeight =
                Mathf.Max(
                    _standingSize.y * crouchedHeightMultiplier,
                    0.01f);

            _crouchedSize =
                new Vector2(
                    _standingSize.x,
                    crouchedHeight);

            _crouchedOffset =
                _standingOffset;

            if (preserveColliderBottom)
            {
                float heightDifference =
                    _standingSize.y - crouchedHeight;

                _crouchedOffset.y -=
                    heightDifference * 0.5f;
            }
        }

        private void ApplyCrouchedCollider()
        {
            if (!resizeCollider)
                return;

            ApplyColliderShape(
                _crouchedSize,
                _crouchedOffset);

            CrouchColliderChanged?.Invoke(true);
        }

        private void ApplyStandingCollider()
        {
            if (!resizeCollider)
                return;

            ApplyColliderShape(
                _standingSize,
                _standingOffset);

            CrouchColliderChanged?.Invoke(false);
        }

        private void ApplyColliderShape(
            Vector2 size,
            Vector2 offset)
        {
            if (_capsuleCollider != null)
            {
                _capsuleCollider.size = size;
                _capsuleCollider.offset = offset;
                return;
            }

            if (_boxCollider != null)
            {
                _boxCollider.size = size;
                _boxCollider.offset = offset;
            }
        }

        private bool HasRoomForStandingCollider()
        {
            if (_capsuleCollider == null &&
                _boxCollider == null)
                return true;

            Vector2 currentSize;
            Vector2 currentOffset;

            if (_capsuleCollider != null)
            {
                currentSize = _capsuleCollider.size;
                currentOffset = _capsuleCollider.offset;
            }
            else
            {
                currentSize = _boxCollider.size;
                currentOffset = _boxCollider.offset;
            }

            float currentTop =
                currentOffset.y +
                currentSize.y * 0.5f;

            float standingTop =
                _standingOffset.y +
                _standingSize.y * 0.5f;

            float addedHeight =
                standingTop - currentTop;

            float standCheckSkin =
                _ceilingDetector != null
                    ? _ceilingDetector.StandCheckSkin
                    : 0f;

            if (addedHeight <= standCheckSkin)
                return true;

            Vector2 checkOffset =
                new(
                    _standingOffset.x,
                    currentTop + addedHeight * 0.5f);

            Vector2 checkSize =
                new(
                    Mathf.Max(0.01f, _standingSize.x),
                    Mathf.Max(0.01f, addedHeight));

            if (_ceilingDetector == null)
                return true;

            _pendingPassThroughOneWayCount = 0;

            return !_ceilingDetector.HasBlockingCeilingInBox(
                transform.TransformPoint(checkOffset),
                checkSize,
                transform.eulerAngles.z,
                _pendingPassThroughOneWayPlatforms,
                out _pendingPassThroughOneWayCount);
        }

        private void IgnorePendingPassThroughOneWayPlatforms()
        {
            for (int i = 0; i < _pendingPassThroughOneWayCount; i++)
            {
                Collider2D platform =
                    _pendingPassThroughOneWayPlatforms[i];

                _pendingPassThroughOneWayPlatforms[i] = null;

                if (platform == null)
                    continue;

                if (_temporarilyIgnoredOneWayPlatforms.Contains(platform))
                    continue;

                Physics2D.IgnoreCollision(
                    _collider,
                    platform,
                    true);

                _temporarilyIgnoredOneWayPlatforms.Add(platform);
            }

            _pendingPassThroughOneWayCount = 0;
        }

        private void UpdateTemporaryOneWayPlatformIgnores()
        {
            for (int i = _temporarilyIgnoredOneWayPlatforms.Count - 1; i >= 0; i--)
            {
                Collider2D platform =
                    _temporarilyIgnoredOneWayPlatforms[i];

                if (platform == null ||
                    !_collider.Distance(platform).isOverlapped)
                {
                    if (platform != null)
                    {
                        Physics2D.IgnoreCollision(
                            _collider,
                            platform,
                            false);
                    }

                    _temporarilyIgnoredOneWayPlatforms.RemoveAt(i);
                }
            }
        }

        private void RestoreTemporaryOneWayPlatformIgnores()
        {
            for (int i = 0; i < _temporarilyIgnoredOneWayPlatforms.Count; i++)
            {
                Collider2D platform =
                    _temporarilyIgnoredOneWayPlatforms[i];

                if (platform == null)
                    continue;

                Physics2D.IgnoreCollision(
                    _collider,
                    platform,
                    false);
            }

            _temporarilyIgnoredOneWayPlatforms.Clear();
            _pendingPassThroughOneWayCount = 0;
        }
        
        private void OnLeftGround(Vector2 _)
        {
            if (clearToggleWhenLeavingGround)
            {
                _toggleCrouchState = false;
            }
        }
    }
}
