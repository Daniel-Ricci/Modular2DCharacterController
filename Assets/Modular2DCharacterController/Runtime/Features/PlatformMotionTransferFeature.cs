using Modular2DCharacterController.Runtime.Core;
using UnityEngine;

namespace Modular2DCharacterController.Runtime.Features
{
    /// <summary>
    /// Adds the current ground/platform velocity as a separate motor velocity layer.
    /// This lets the character walk normally while still being carried by moving platforms.
    /// </summary>
    [RequireComponent(typeof(CharacterController2D))]
    public class PlatformMotionTransferFeature : MonoBehaviour, ICharacterFeature
    {
        private GroundDetector _groundDetector;
        private CharacterMotor _motor;

        private void Awake()
        {
            _groundDetector = GetComponent<GroundDetector>();
            _motor = GetComponent<CharacterMotor>();
        }

        public void Tick()
        {
        }

        public void FixedTick()
        {
            if (!_groundDetector.IsGrounded)
                return;

            _motor.SetExternalVelocity(_groundDetector.GroundVelocity);
        }
    }
}