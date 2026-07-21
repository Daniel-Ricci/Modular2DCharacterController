using Modular2DCharacterController.Runtime.Data.FeatureProfiles;
using Modular2DCharacterController.Runtime.Features;
using Modular2DCharacterController.Runtime.Input;
using UnityEngine;

namespace Modular2DCharacterController.Runtime.Core
{
    /// <summary>
    /// Coordinates character features and stores profile providers.
    /// </summary>
    [RequireComponent(typeof(CharacterMotor))]
    [RequireComponent(typeof(ICharacterInput))]
    public class CharacterController2D : MonoBehaviour
    {
        private ICharacterFeature[] _features;

        public ProfileProvider<HorizontalMovementProfile> HorizontalMovementProfileProvider { get; }
            = new();

        public ProfileProvider<JumpProfile> JumpProfileProvider { get; }
            = new();

        public ProfileProvider<DashProfile> DashProfileProvider { get; }
            = new();

        public ProfileProvider<RollProfile> RollProfileProvider { get; }
            = new();

        public ProfileProvider<WallJumpProfile> WallJumpProfileProvider { get; }
            = new();
        
        public ProfileProvider<GlideProfile> GlideProfileProvider { get; }
            = new();

        public ProfileProvider<GroundPoundProfile> GroundPoundProfileProvider { get; }
            = new();

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
