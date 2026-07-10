using Modular2DCharacterController.SampleScenes.Spaceship.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Modular2DCharacterController.SampleScenes.Spaceship.UI
{
    public class LivesCounter : MonoBehaviour
    {
        [SerializeField]
        private PlayerCollisionDetector playerCollisionDetector;

        [SerializeField]
        private Image livesIcon1;
        [SerializeField]
        private Image livesIcon2;
        [SerializeField]
        private Image livesIcon3;

        private void OnEnable()
        {
            playerCollisionDetector.DamageTaken += OnDamageTaken;
        }

        private void OnDisable()
        {
            playerCollisionDetector.DamageTaken -= OnDamageTaken;
        }

        private void OnDamageTaken(int livesLeft)
        {
            switch (livesLeft)
            {
                case 2:
                    livesIcon3.enabled = false;
                    break;
                case 1:
                    livesIcon2.enabled = false;
                    break;
                case 0:
                    livesIcon1.enabled = false;
                    break;
            }
        }
        
    }
}
