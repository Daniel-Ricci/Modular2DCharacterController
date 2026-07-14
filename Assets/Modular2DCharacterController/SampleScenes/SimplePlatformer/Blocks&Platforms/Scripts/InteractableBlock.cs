using System.Collections;
using Modular2DCharacterController.Runtime.Core;
using UnityEngine;

namespace Modular2DCharacterController.SampleScenes.SimplePlatformer.Blocks_Platforms.Scripts
{
    public abstract class InteractableBlock: MonoBehaviour, ICeilingHitReceiver, IDashHitReceiver, IGroundPoundHitReceiver
    {
        [SerializeField] private float bounceHeight = 0.2f;
        [SerializeField] private float bounceDuration = 0.1f;
        
        public void OnCeilingHit(CharacterHitEvent hitEvent)
        {
            OnHit(hitEvent, Vector2.up);
        }

        public void OnDashHit(CharacterHitEvent hitEvent)
        {
            OnHit(hitEvent, hitEvent.Velocity.x > 0 ? Vector2.right : Vector2.left);
        }

        public void OnGroundPoundHit(CharacterHitEvent hitEvent)
        {
            OnHit(hitEvent, Vector2.down);
        }
        
        protected virtual void OnHit(CharacterHitEvent hitEvent, Vector2 direction)
        {
            StartCoroutine(BounceRoutine(direction));
        }
        
        private IEnumerator BounceRoutine(Vector2 direction)
        {
            Vector2 startPosition = transform.localPosition;
            Vector2 endPosition = startPosition + direction * bounceHeight;

            float elapsed = 0f;
            while (elapsed < bounceDuration)
            {
                elapsed += Time.deltaTime;
                transform.localPosition = Vector2.Lerp(startPosition, endPosition, elapsed / bounceDuration);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < bounceDuration)
            {
                elapsed += Time.deltaTime;
                transform.localPosition = Vector2.Lerp(endPosition, startPosition, elapsed / bounceDuration);
                yield return null;
            }

            transform.localPosition = startPosition;
        }
    }
}