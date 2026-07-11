using System.Collections;
using UnityEngine;

namespace Modular2DCharacterController.SampleScenes.SimplePlatformer.Player.Scripts
{
    public class PlayerEyesController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody2D rigidbody;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Sprites")]
        [SerializeField] private Sprite eyesStill;
        [SerializeField] private Sprite eyesSad;
        [SerializeField] private Sprite eyesHappy;
        [SerializeField] private Sprite eyesHit;

        [Header("Settings")]
        [SerializeField] private float idleAnimationTime = 3f;
        [SerializeField] private float hitDuration = 0.5f;
        [SerializeField] private float happyDuration = 0.75f;
        [SerializeField] private float movementThreshold = 0.05f;

        private float idleTimer;

        private bool hitOverride;
        private bool happyOverride;

        private void Awake()
        {
            spriteRenderer.sprite = eyesStill;
        }

        private void Update()
        {
            if (hitOverride || happyOverride)
                return;

            float velocity = rigidbody.linearVelocity.magnitude;

            if (velocity > movementThreshold)
            {
                idleTimer = 0f;
            }
            else
            {
                idleTimer += Time.deltaTime;
                spriteRenderer.sprite = idleTimer >= idleAnimationTime ? eyesSad : eyesStill;
            }
        }

        public void OnHit()
        {
            if (!gameObject.activeInHierarchy)
                return;

            StopCoroutineSafe();
            StartCoroutine(HitRoutine());
        }

        public void OnHappy()
        {
            if (!gameObject.activeInHierarchy)
                return;

            StopCoroutineSafe();
            StartCoroutine(HappyRoutine());
        }

        private IEnumerator HitRoutine()
        {
            hitOverride = true;
            happyOverride = false;
            idleTimer = 0f;

            spriteRenderer.sprite = eyesHit;

            yield return new WaitForSeconds(hitDuration);

            hitOverride = false;
        }

        private IEnumerator HappyRoutine()
        {
            happyOverride = true;
            hitOverride = false;
            idleTimer = 0f;

            spriteRenderer.sprite = eyesHappy;

            yield return new WaitForSeconds(happyDuration);

            happyOverride = false;
        }

        private void StopCoroutineSafe()
        {
            StopAllCoroutines();

            hitOverride = false;
            happyOverride = false;
        }
    }
}