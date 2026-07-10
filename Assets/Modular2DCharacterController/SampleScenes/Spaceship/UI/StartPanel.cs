using UnityEngine;

namespace Modular2DCharacterController.SampleScenes.Spaceship.UI
{
    public class StartPanel : MonoBehaviour
    {
        private void Awake()
        {
            Time.timeScale = 0f;
        }

        private void Update()
        {
            if (Input.anyKeyDown)
            {
                Time.timeScale = 1f;
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
        }
    }
}