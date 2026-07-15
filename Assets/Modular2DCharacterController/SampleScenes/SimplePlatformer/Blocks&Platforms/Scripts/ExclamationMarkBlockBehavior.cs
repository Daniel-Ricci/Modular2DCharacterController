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

        private RedBlueState _redBlueState;

        private void OnEnable()
        {
            _redBlueState = FindAnyObjectByType<RedBlueState>();
            if (_redBlueState != null)
            {
                _redBlueState.OnStateChanged += UpdateVisuals;
                UpdateVisuals(_redBlueState.isBlue);
            }
        }
        
        protected override void OnHit(CharacterHitEvent hitEvent, Vector2 direction)
        {
            base.OnHit(hitEvent, direction);
            _redBlueState.ChangeState();
        }

        private void UpdateVisuals(bool isBlue)
        {
            if (isBlue)
            {
                blockRenderer.material = blueMaterial;
                exclamationMarkRenderer.material = blueMaterial;
            }
            else
            {
                blockRenderer.material = redMaterial;
                exclamationMarkRenderer.material = redMaterial;
            }
        }
    }
}