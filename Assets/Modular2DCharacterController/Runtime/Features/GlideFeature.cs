using System;
using Modular2DCharacterController.Runtime.Core;
using Modular2DCharacterController.Runtime.Data.FeatureProfiles;
using Modular2DCharacterController.Runtime.Input;
using UnityEngine;

namespace Modular2DCharacterController.Runtime.Features
{
    /// <summary>
    /// A configurable feature that allows the character to glide while airborne,
    /// reducing fall speed through either gravity scaling or velocity clamping.
    /// </summary>
    [RequireComponent(typeof(CharacterController2D))]
    public class GlideFeature : MonoBehaviour, ICharacterFeature
    {
        [Header("Default Glide Profile")]

        [Tooltip(
            "The default glide profile registered when this feature initializes.")]
        [SerializeField]
        private GlideProfile defaultGlideProfile;

        /// <summary>
        /// Invoked when gliding begins.
        /// </summary>
        public event Action GlideStarted;

        /// <summary>
        /// Invoked when gliding ends.
        /// </summary>
        public event Action GlideEnded;

        /// <summary>
        /// Gets a value indicating whether the character is currently gliding.
        /// </summary>
        public bool IsGliding { get; private set; }

        private CharacterMotor _motor;
        private GroundDetector _groundDetector;
        private ICharacterInput _input;
        private CharacterController2D _controller;
        private ProfileProvider<GlideProfile> _glideProfileProvider;

        private void Awake()
        {
            _motor = GetComponent<CharacterMotor>();
            _groundDetector = GetComponent<GroundDetector>();
            _input = GetComponent<ICharacterInput>();
            _controller = GetComponent<CharacterController2D>();

            _glideProfileProvider =
                _controller.GlideProfileProvider;
        }

        private void OnEnable()
        {
            if (defaultGlideProfile != null)
            {
                _glideProfileProvider?.RegisterProfile(
                    defaultGlideProfile);
            }
        }

        private void OnDisable()
        {
            if (defaultGlideProfile != null)
            {
                _glideProfileProvider?.UnregisterProfile(
                    defaultGlideProfile);
            }
        }

        public void Tick()
        {
        }

        public void FixedTick()
        {
            GlideProfile currentProfile =
                _glideProfileProvider?.GetCurrentProfile();

            if (currentProfile == null)
                return;

            bool wasGliding = IsGliding;

            UpdateGlideState();

            if (!wasGliding && IsGliding)
            {
                GlideStarted?.Invoke();
            }
            else if (wasGliding && !IsGliding)
            {
                GlideEnded?.Invoke();
            }

            if (!IsGliding)
                return;

            ApplyGlide(currentProfile);
        }

        private void UpdateGlideState()
        {
            IsGliding =
                !_groundDetector.IsGrounded &&
                _motor.CurrentSelfVelocity.y < 0f &&
                _input.RunHeld;
        }

        private void ApplyGlide(GlideProfile currentProfile)
        {
            if (currentProfile.useGravityDuringGlide)
            {
                _motor.AddGravityMultiplier(
                    currentProfile.gravityFactor);
            }
            else
            {
                _motor.SetVerticalSelfVelocity(
                    -currentProfile.fallSpeed);
                _motor.SuppressGravityThisFrame();
            }
        }
    }
}