using System;
using Modular2DCharacterController.Runtime.Core;
using Modular2DCharacterController.Runtime.Data.FeatureProfiles;
using Modular2DCharacterController.Runtime.Input;
using UnityEngine;

namespace Modular2DCharacterController.Runtime.Features
{
    /// <summary>
    /// A configurable feature that handles player wall jumps.
    /// </summary>
    [RequireComponent(typeof(CharacterController2D))]
    [RequireComponent(typeof(WallDetector))]
    [RequireComponent(typeof(WallSlideFeature))]
    public class WallJumpFeature : MonoBehaviour, ICharacterFeature
    {
        [Header("Default Jump Profile")]

        [Tooltip(
            "The default wall jump profile registered when this feature initializes.")]
        [SerializeField]
        private WallJumpProfile defaultWallJumpProfile;

        private CharacterMotor _motor;
        private WallDetector _wallDetector;
        private WallSlideFeature _wallSlideFeature;
        private ICharacterInput _input;
        private CharacterController2D _controller;
        private ProfileProvider<WallJumpProfile> _wallJumpProfileProvider;

        private float _movementLockTimer;

        private bool _jumpRequested;

        /// <summary>
        /// Invoked when a wall jump is performed.
        /// </summary>
        public event Action WallJumped;

        /// <summary>
        /// Gets a value indicating whether horizontal movement is currently locked.
        /// </summary>
        public bool IsMovementLocked =>
            _movementLockTimer > 0f;

        private void Awake()
        {
            _motor = GetComponent<CharacterMotor>();
            _wallDetector = GetComponent<WallDetector>();
            _wallSlideFeature = GetComponent<WallSlideFeature>();
            _input = GetComponent<ICharacterInput>();
            _controller = GetComponent<CharacterController2D>();

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
            if (_input.JumpPressed)
            {
                _jumpRequested = true;
            }
        }

        public void FixedTick()
        {
            if (_movementLockTimer > 0f)
            {
                _movementLockTimer -= Time.fixedDeltaTime;
            }

            TryWallJump();
        }

        private void TryWallJump()
        {
            if (!_jumpRequested)
                return;

            _jumpRequested = false;

            if (!_wallSlideFeature.IsWallSliding)
                return;

            WallJumpProfile currentWallJumpProfile =
                _wallJumpProfileProvider?.GetCurrentProfile();

            if (currentWallJumpProfile == null)
                return;

            Vector2 wallNormal =
                _wallDetector.WallNormal;

            Vector2 velocity = new Vector2(
                wallNormal.x *
                currentWallJumpProfile.horizontalForce,
                currentWallJumpProfile.verticalForce);

            _motor.SetVelocity(velocity);

            _movementLockTimer =
                currentWallJumpProfile.movementLockDuration;

            WallJumped?.Invoke();
        }
    }
}