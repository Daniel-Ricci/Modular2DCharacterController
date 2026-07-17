using Modular2DCharacterController.Runtime.Core;
using Modular2DCharacterController.Runtime.Features;
using UnityEngine;

namespace Modular2DCharacterController.SampleScenes.FastPaced.Scripts
{
    public class AutoMoveFeature : MonoBehaviour, ICharacterFeature
    {
        [SerializeField]
        private float moveSpeed;
        
        private CharacterMotor _motor;
        private WallJumpFeature _wallJumpFeature;

        private void Awake()
        {
            _motor = GetComponent<CharacterMotor>();
            _wallJumpFeature = GetComponent<WallJumpFeature>();
        }
        
        public void Tick()
        {
        }

        public void FixedTick()
        {
            if (_wallJumpFeature != null && _wallJumpFeature.IsControlInfluenceActive)
                return;
            
            _motor?.SetHorizontalSelfVelocity(moveSpeed);
        }
    }
}
