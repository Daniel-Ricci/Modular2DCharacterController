using Modular2DCharacterController.Runtime.Core;
using Modular2DCharacterController.Runtime.Data.FeatureProfiles;
using Modular2DCharacterController.Runtime.Input;
using System;
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
        
        [Tooltip("Run movement profile to register when running")]
        [SerializeField]
        private HorizontalMovementProfile runMovementProfile;

        [Header("Run Settings")]
        
        [Tooltip("Minimum input necessary to start running")]
        [SerializeField]
        [Range(0f, 1f)]
        private float minimumMoveInput = 0.1f;
        
        // True while running
        public bool IsRunning { get; private set; }
        
        // Events for starting and stopping run.
        public event Action StartedRun;
        public event Action StoppedRun;

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
        
        private void OnDisable()
        {
            if(runMovementProfile != null)
                _profileProvider?.UnregisterProfile(runMovementProfile);

            IsRunning = false;
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
                _profileProvider?.UnregisterProfile(runMovementProfile);
                return;
            }

            bool shouldRun = CanRun();

            if (shouldRun)
            {
                _profileProvider?.RegisterProfile(runMovementProfile);
                StartedRun?.Invoke();
                IsRunning = true;
            }
            else
            {
                _profileProvider?.UnregisterProfile(runMovementProfile);
                StoppedRun?.Invoke();
                IsRunning = false;
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