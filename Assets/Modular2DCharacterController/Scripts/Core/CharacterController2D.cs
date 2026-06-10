using Modular2DCharacterController.Scripts.Data;
using Modular2DCharacterController.Scripts.Features;
using UnityEngine;

namespace Modular2DCharacterController.Scripts.Core
{
    /// <summary>
    /// Coordinates character features and stores profile providers.
    /// </summary>
    [RequireComponent(typeof(GroundDetector))]
    [RequireComponent(typeof(CharacterMotor))]
    public class CharacterController2D : MonoBehaviour
    {
        private ICharacterFeature[] _features;
        
        // Profile providers
        public ProfileProvider<HorizontalMovementProfile> HorizontalMovementProfileProvider { get; }
            = new();
        public ProfileProvider<JumpProfile> JumpProfileProvider { get; } = new();
        public ProfileProvider<DashProfile> DashProfileProvider { get; } = new();

        private void Awake()
        {
            _features = GetComponents<ICharacterFeature>();
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
            foreach (ICharacterFeature feature in _features)
            {
                feature.FixedTick();
            }
        }
    }
}