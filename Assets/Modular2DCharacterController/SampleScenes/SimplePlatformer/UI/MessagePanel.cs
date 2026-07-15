using System.Collections;
using UnityEngine;

namespace Modular2DCharacterController.SampleScenes.SimplePlatformer.UI
{
    public class MessagePanel : MonoBehaviour
    {
        private void OnEnable()
        {
            Time.timeScale = 0f;
            StartCoroutine(ScaleInRoutine());
        }

        private void Update()
        {
            if (Input.anyKeyDown)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
        }
        
        private IEnumerator ScaleInRoutine()
        {
            transform.localScale = Vector3.zero;

            float duration = 0.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(elapsed / duration);
                transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);

                yield return null;
            }

            transform.localScale = Vector3.one;
        }
    }
}