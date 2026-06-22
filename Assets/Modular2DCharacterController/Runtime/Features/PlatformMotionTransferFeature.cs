using Modular2DCharacterController.Runtime.Core;
using UnityEngine;

namespace Modular2DCharacterController.Runtime.Features
{
    /// <summary>
    /// A configurable feature that allows moveable ground/platforms to transfer
    /// its movement to the character.
    /// 
    /// Adds the current ground/platform velocity as a separate motor velocity layer.
    /// This lets the character walk normally while still being carried by moving platforms.
    ///
    /// Ground detector is responsible for retrieving the velocity of the ground/platform.
    /// </summary>
    [RequireComponent(typeof(CharacterController2D))]
    public class PlatformMotionTransferFeature : MonoBehaviour, ICharacterFeature
    {
        // Components used by this feature.
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

            _motor.AddExternalVelocity(_groundDetector.GroundVelocity);
        }
    }
}