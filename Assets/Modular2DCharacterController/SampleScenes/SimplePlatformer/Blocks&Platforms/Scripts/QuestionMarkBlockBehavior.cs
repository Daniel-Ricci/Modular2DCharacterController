using Modular2DCharacterController.Runtime.Core;
using Modular2DCharacterController.SampleScenes.SimplePlatformer.UI;
using UnityEngine;

namespace Modular2DCharacterController.SampleScenes.SimplePlatformer.Blocks_Platforms.Scripts
{
    public class QuestionMarkBlockBehavior : InteractableBlock
    {
        [SerializeField]
        private SpriteRenderer questionMarkRenderer;

        [SerializeField]
        private GameObject objectToShow;
        
        private bool _isHit = false;

        protected override void OnHit(CharacterHitEvent hitEvent, Vector2 direction)
        {
            if (!_isHit)
            {
                _isHit = true;
                base.OnHit(hitEvent, direction);
                questionMarkRenderer.enabled = false;
                objectToShow.SetActive(true);
            }
        }
    }
}
