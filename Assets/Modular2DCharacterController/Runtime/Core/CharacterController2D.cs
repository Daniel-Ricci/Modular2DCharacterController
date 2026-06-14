using Modular2DCharacterController.Runtime.Data;
using Modular2DCharacterController.Runtime.Data.FeatureProfiles;
using Modular2DCharacterController.Runtime.Features;
using UnityEngine;

namespace Modular2DCharacterController.Runtime.Core
{
    /// <summary>
    /// Coordinates character features and stores profile providers.
    /// </summary>
    [RequireComponent(typeof(GroundDetector))]
    [RequireComponent(typeof(CharacterMotor))]
    public class CharacterController2D : MonoBehaviour
    {
        private ICharacterFeature[] _features;
        private CharacterMotor _motor;

        public ProfileProvider<HorizontalMovementProfile> HorizontalMovementProfileProvider { get; }
            = new();

        public ProfileProvider<JumpProfile> JumpProfileProvider { get; }
            = new();

        public ProfileProvider<DashProfile> DashProfileProvider { get; }
            = new();

        private void Awake()
        {
            _features = GetComponents<ICharacterFeature>();
            _motor = GetComponent<CharacterMotor>();
        }

        private void Update()
        {
            foreach (ICharacterFeature feature in _features)
            {
                feature.Tick();
            }
        }

        private void FixedUpdate()
        {
            _motor.BeginVelocityStep();

            foreach (ICharacterFeature feature in _features)
            {
                feature.FixedTick();
            }

            _motor.ApplyVelocity();
        }
    }
}