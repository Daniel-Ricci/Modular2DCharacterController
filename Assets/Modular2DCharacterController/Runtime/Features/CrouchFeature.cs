using Modular2DCharacterController.Runtime.Core;
using Modular2DCharacterController.Runtime.Data.FeatureProfiles;
using Modular2DCharacterController.Runtime.Input;
using System;
using UnityEngine;

namespace Modular2DCharacterController.Runtime.Features
{
    public enum CrouchMode
    {
        Hold,
        Toggle
    }

    /// <summary>
    /// A configurable feature that registers a higher-priority horizontal movement profile while crouching.
    /// </summary>
    [RequireComponent(typeof(CharacterController2D))]
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

        // True while the player is crouching
        public bool IsCrouching { get; private set; }

        // Events for starting and stopping run.
        public event Action CrouchStarted;
        public event Action CrouchEnded;
        
        private CharacterController2D _controller;
        private ICharacterInput _input;
        private GroundDetector _groundDetector;
        private ProfileProvider<HorizontalMovementProfile> _profileProvider;

        // Internal state used when operating in Toggle mode.
        private bool _toggleCrouchState;
        
        private void Awake()
        {
            _controller = GetComponent<CharacterController2D>();
            _input = GetComponent<ICharacterInput>();
            _groundDetector = GetComponent<GroundDetector>();

            _profileProvider = _controller.HorizontalMovementProfileProvider;
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
                IsCrouching = false;
                CrouchEnded?.Invoke();
            }
        }
        
        public void Tick()
        {
            UpdateToggleInput();
        }
        
        public void FixedTick()
        {
            UpdateCrouchState();
        }
        
        private void UpdateToggleInput()
        {
            if (_input == null)
                return;

            if (crouchMode != CrouchMode.Toggle)
                return;

            if (_input.CrouchPressed)
            {
                _toggleCrouchState = !_toggleCrouchState;
            }
        }
        
        private void UpdateCrouchState()
        {
            bool shouldCrouch = CanCrouch();

            // Nothing changed, so no work is needed.
            if (shouldCrouch == IsCrouching)
                return;

            IsCrouching = shouldCrouch;

            if (IsCrouching)
            {
                // Apply crouch movement profile.
                if (crouchMovementProfile != null)
                {
                    _profileProvider.RegisterProfile(crouchMovementProfile);
                }
                CrouchStarted?.Invoke();
            }
            else
            {
                // Remove crouch movement profile.
                if (crouchMovementProfile != null)
                {
                    _profileProvider.UnregisterProfile(crouchMovementProfile);
                }
                CrouchEnded?.Invoke();
            }
        }
        
        private bool CanCrouch()
        {
            if (_input == null)
                return false;

            // Determine whether the player is requesting crouch.
            bool crouchRequested = crouchMode switch
            {
                CrouchMode.Hold => _input.CrouchHeld,
                CrouchMode.Toggle => _toggleCrouchState,
                _ => false
            };

            if (!crouchRequested)
                return false;

            // Optional grounded-only restriction.
            if (groundedOnly &&
                _groundDetector != null &&
                !_groundDetector.IsGrounded)
            {
                return false;
            }

            // Optional minimum movement input requirement.
            if (requireMovementInput &&
                Mathf.Abs(_input.MoveInput) < minimumMoveInput)
            {
                return false;
            }

            return true;
        }
        
        private void OnLeftGround(Vector2 unused)
        {
            if (clearToggleWhenLeavingGround)
            {
                _toggleCrouchState = false;
            }
        }
    }
}