using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Modular2DCharacterController.SampleScenes.Spaceship.Player
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerCollisionDetector : MonoBehaviour
    {
        public event Action<int> DamageTaken;

        [SerializeField] private float immuneTimeAfterHit = 1.5f;
        [SerializeField] private float flashInterval = 0.1f;
        [SerializeField] private float flashAlpha = 0.3f;

        private SpriteRenderer spriteRenderer;
        private bool immune;
        private int lives = 3;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Meteor")) TryTakeDamage();
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.CompareTag("Meteor")) TryTakeDamage();
        }

        private void TryTakeDamage()
        {
            if (immune)
                return;

            lives--;
            DamageTaken?.Invoke(lives);
            if (lives == 0)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            StartCoroutine(ImmunityCoroutine());
        }

        private IEnumerator ImmunityCoroutine()
        {
            immune = true;

            Color color = spriteRenderer.color;
            float elapsed = 0f;
            bool faded = false;

            while (elapsed < immuneTimeAfterHit)
            {
                faded = !faded;
                color.a = faded ? flashAlpha : 1f;
                spriteRenderer.color = color;

                yield return new WaitForSeconds(flashInterval);
                elapsed += flashInterval;
            }

            color.a = 1f;
            spriteRenderer.color = color;

            immune = false;
        }
    }
}