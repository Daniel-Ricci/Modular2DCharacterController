using UnityEngine;
using Modular2DCharacterController.Core;
using Modular2DCharacterController.Input;

namespace Modular2DCharacterController.Features
{
    /// <summary>
    /// Handles horizontal character movement.
    /// </summary>
    public class HorizontalMovementFeature : MonoBehaviour, ICharacterFeature
    {
        [SerializeField] private float moveSpeed;

        private CharacterMotor _motor;
        private ICharacterInput _input;

        private void Awake()
        {
            _motor = GetComponent<CharacterMotor>();
            _input = GetComponent<ICharacterInput>();
        }

        public void Tick()
        {
        }

        public void FixedTick()
        {
            _motor.SetHorizontalVelocity(_input.MoveInput * moveSpeed);
        }
    }
}