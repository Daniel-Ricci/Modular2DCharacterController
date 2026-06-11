using Modular2DCharacterController.Runtime.Core;
using Modular2DCharacterController.Runtime.Data;
using Modular2DCharacterController.Runtime.Input;
using UnityEngine;

namespace Modular2DCharacterController.Runtime.Features
{
    /// <summary>
    /// A configurable feature that registers a higher-priority horizontal movement profile while the run input is held.
    /// </summary>
    [RequireComponent(typeof(CharacterController2D))]
    public class RunFeature : MonoBehaviour, ICharacterFeature
    {
        [Header("Run Profile")]
        [SerializeField]
        private HorizontalMovementProfile runMovementProfile;

        [Header("Run Settings")]
        [SerializeField]
        [Range(0f, 1f)]
        private float minimumMoveInput = 0.1f;

        private CharacterController2D _controller;
        private ICharacterInput _input;
        private GroundDetector _groundDetector;
        private ProfileProvider<HorizontalMovementProfile> _profileProvider;

        private void Awake()
        {
            _controller = GetComponent<CharacterController2D>();
            _input = GetComponent<ICharacterInput>();
            _groundDetector = GetComponent<GroundDetector>();

            _profileProvider = _controller.HorizontalMovementProfileProvider;
        }

        public void Tick()
        {
        }

        public void FixedTick()
        {
            UpdateRunState();
        }

        private void UpdateRunState()
        {
            if (runMovementProfile == null || _input == null)
            {
                _profileProvider.UnregisterProfile(runMovementProfile);
                return;
            }

            bool shouldRun = CanRun();

            if (shouldRun)
            {
                _profileProvider.RegisterProfile(runMovementProfile);
            }
            else
            {
                _profileProvider.UnregisterProfile(runMovementProfile);
            }
        }

        private bool CanRun()
        {
            if (!_input.RunHeld)
                return false;

            if (Mathf.Abs(_input.MoveInput) < minimumMoveInput)
            {
                return false;
            }

            if (_groundDetector != null &&
                !_groundDetector.IsGrounded)
            {
                return false;
            }

            return true;
        }
    }
}