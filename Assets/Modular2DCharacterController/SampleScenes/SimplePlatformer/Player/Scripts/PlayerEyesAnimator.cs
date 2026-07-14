using System.Collections;
using Modular2DCharacterController.Runtime.Core;
using UnityEngine;

namespace Modular2DCharacterController.SampleScenes.SimplePlatformer.Player.Scripts
{
    public class PlayerEyesAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody2D rigidbody;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Sprites")]
        [SerializeField] private Sprite eyesStill;
        [SerializeField] private Sprite eyesSad;
        [SerializeField] private Sprite eyesHappy;
        [SerializeField] private Sprite eyesHit;
        [SerializeField] private Sprite eyesClosed;

        [Header("Settings")]
        [SerializeField] private float idleAnimationTime = 3f;
        [SerializeField] private float movementThreshold = 0.05f;
        [SerializeField] private float hitDuration = 0.5f;
        [SerializeField] private float happyDuration = 0.75f;

        [Header("Blink")]
        [SerializeField] private float minBlinkInterval = 3.5f;
        [SerializeField] private float maxBlinkInterval = 5.5f;
        [SerializeField] private float blinkClosedTime = 0.10f;
        [SerializeField] private float blinkGap = 0.08f;

        private CharacterEventDispatcher eventDispatcher;

        private enum EyeState
        {
            Normal,
            Happy,
            Hit
        }

        private EyeState state = EyeState.Normal;

        private bool blinking;
        private bool idleSad;

        private float idleTimer;

        private Coroutine hitRoutine;
        private Coroutine happyRoutine;

        private void Awake()
        {
            eventDispatcher = GetComponent<CharacterEventDispatcher>();

            if (eventDispatcher != null)
                eventDispatcher.CeilingHit += OnHit;
        }

        private void Start()
        {
            StartCoroutine(BlinkRoutine());
        }

        private void OnDisable()
        {
            if (eventDispatcher != null)
                eventDispatcher.CeilingHit -= OnHit;
        }

        private void Update()
        {
            if (state == EyeState.Normal)
            {
                if (rigidbody.linearVelocity.magnitude > movementThreshold)
                {
                    idleTimer = 0f;
                    idleSad = false;
                }
                else
                {
                    idleTimer += Time.deltaTime;
                    idleSad = idleTimer >= idleAnimationTime;
                }
            }

            UpdateExpression();
        }

        private void UpdateExpression()
        {
            if (blinking)
            {
                spriteRenderer.sprite = eyesClosed;
                return;
            }

            switch (state)
            {
                case EyeState.Hit:
                    spriteRenderer.sprite = eyesHit;
                    break;

                case EyeState.Happy:
                    spriteRenderer.sprite = eyesHappy;
                    break;

                default:
                    spriteRenderer.sprite = idleSad ? eyesSad : eyesStill;
                    break;
            }
        }

        public void OnHit(CharacterHitEvent _)
        {
            if (!gameObject.activeInHierarchy)
                return;

            if (happyRoutine != null)
            {
                StopCoroutine(happyRoutine);
                happyRoutine = null;
            }

            if (hitRoutine != null)
                StopCoroutine(hitRoutine);

            hitRoutine = StartCoroutine(HitRoutine());
        }

        public void OnHappy()
        {
            if (!gameObject.activeInHierarchy)
                return;

            if (hitRoutine != null)
            {
                StopCoroutine(hitRoutine);
                hitRoutine = null;
            }

            if (happyRoutine != null)
                StopCoroutine(happyRoutine);

            happyRoutine = StartCoroutine(HappyRoutine());
        }

        private IEnumerator HitRoutine()
        {
            state = EyeState.Hit;
            idleTimer = 0f;
            idleSad = false;

            yield return new WaitForSeconds(hitDuration);

            state = EyeState.Normal;
            hitRoutine = null;
        }

        private IEnumerator HappyRoutine()
        {
            state = EyeState.Happy;
            idleTimer = 0f;
            idleSad = false;

            yield return new WaitForSeconds(happyDuration);

            state = EyeState.Normal;
            happyRoutine = null;
        }

        private IEnumerator BlinkRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(minBlinkInterval, maxBlinkInterval));

                if (state != EyeState.Normal)
                    continue;
                
                blinking = true;

                yield return new WaitForSeconds(blinkClosedTime);

                blinking = false;

                yield return new WaitForSeconds(blinkGap);

                blinking = true;

                yield return new WaitForSeconds(blinkClosedTime);

                blinking = false;
            }
        }
    }
}
