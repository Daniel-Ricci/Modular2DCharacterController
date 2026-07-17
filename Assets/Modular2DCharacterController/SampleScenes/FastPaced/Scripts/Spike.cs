using UnityEngine;
using UnityEngine.SceneManagement;

namespace Modular2DCharacterController.SampleScenes.FastPaced.Scripts
{
    public class Spike : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
