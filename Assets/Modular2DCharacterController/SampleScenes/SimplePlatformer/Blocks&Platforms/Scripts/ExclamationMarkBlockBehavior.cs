using Modular2DCharacterController.Runtime.Core;
using UnityEngine;

namespace Modular2DCharacterController.SampleScenes.SimplePlatformer.Blocks_Platforms.Scripts
{
    public class ExclamationMarkBlockBehavior : InteractableBlock
    {
        [SerializeField]
        private SpriteRenderer blockRenderer;
        [SerializeField]
        private SpriteRenderer exclamationMarkRenderer;
        
        [SerializeField]
        private Material redMaterial;
        [SerializeField]
        private Material blueMaterial;

        private bool _isBlue = true;
        
        protected override void OnHit(CharacterHitEvent hitEvent, Vector2 direction)
        {
            base.OnHit(hitEvent, direction);
            if (_isBlue)
            {
                _isBlue = false;
                blockRenderer.material = redMaterial;
                exclamationMarkRenderer.material = redMaterial;
            }
            else
            {
                _isBlue = true;
                blockRenderer.material = blueMaterial;
                exclamationMarkRenderer.material = blueMaterial;
            }
        }
    }
}