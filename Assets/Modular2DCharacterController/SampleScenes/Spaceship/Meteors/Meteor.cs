using UnityEngine;

namespace Modular2DCharacterController.SampleScenes.Spaceship.Meteors
{
    public class Meteor : MonoBehaviour
    {
        private float _speed;
        private float _leftLimit;
        private MeteorSpawner _spawner;

        public void Initialize(
            float speed,
            float scale,
            float leftLimit,
            MeteorSpawner spawner)
        {
            _speed = speed;
            _leftLimit = leftLimit;
            _spawner = spawner;

            transform.localScale = Vector3.one * scale;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            transform.Translate(Vector2.left * (_speed * Time.deltaTime), Space.World);

            if (transform.position.x < _leftLimit)
                _spawner.ReturnToPool(this);
        }
    }
}