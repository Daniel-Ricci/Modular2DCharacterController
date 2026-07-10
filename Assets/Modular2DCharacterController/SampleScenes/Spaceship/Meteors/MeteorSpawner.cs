using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Modular2DCharacterController.SampleScenes.Spaceship.Meteors
{
    public class MeteorSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Meteor meteorPrefab;

        [Header("Pooling")]
        [SerializeField] private int poolSize = 40;

        [Header("Spawn")]
        [SerializeField] private float minSpawnDelay = 0.25f;
        [SerializeField] private float maxSpawnDelay = 0.8f;

        [Header("Scale")]
        [SerializeField] private float minScale = 0.4f;
        [SerializeField] private float maxScale = 1.5f;

        [Header("Speed")]
        [SerializeField] private float minSpeed = 4f;
        [SerializeField] private float maxSpeed = 12f;

        [Header("Gap")]
        [SerializeField] private float safeGapHeight = 2.5f;

        private readonly Queue<Meteor> _pool = new();

        private Camera _camera;

        private float _left;
        private float _right;
        private float _top;
        private float _bottom;

        private void Awake()
        {
            _camera = Camera.main;

            Vector3 bottomLeft = _camera.ViewportToWorldPoint(Vector3.zero);
            Vector3 topRight = _camera.ViewportToWorldPoint(Vector3.one);

            _left = bottomLeft.x - 2f;
            _right = topRight.x + 2f;
            _bottom = bottomLeft.y;
            _top = topRight.y;

            for (int i = 0; i < poolSize; i++)
            {
                Meteor meteor = Instantiate(meteorPrefab, transform);
                meteor.gameObject.SetActive(false);
                _pool.Enqueue(meteor);
            }
        }

        private void Start()
        {
            StartCoroutine(SpawnRoutine());
        }

        private IEnumerator SpawnRoutine()
        {
            while (true)
            {
                SpawnWave();

                yield return new WaitForSeconds(
                    Random.Range(minSpawnDelay, maxSpawnDelay));
            }
        }

        private void SpawnWave()
        {
            float gapCenter = Random.Range(
                _bottom + safeGapHeight * 0.5f,
                _top - safeGapHeight * 0.5f);

            SpawnRegion(_bottom, gapCenter - safeGapHeight * 0.5f);
            SpawnRegion(gapCenter + safeGapHeight * 0.5f, _top);
        }

        private void SpawnRegion(float minY, float maxY)
        {
            if (maxY <= minY)
                return;

            int count = Random.Range(0, 3);

            for (int i = 0; i < count; i++)
            {
                if (_pool.Count == 0)
                    return;

                Meteor meteor = _pool.Dequeue();

                float scale = Random.Range(minScale, maxScale);

                // Smaller -> faster
                float t = Mathf.InverseLerp(maxScale, minScale, scale);
                float speed = Mathf.Lerp(minSpeed, maxSpeed, t);

                meteor.transform.position = new Vector2(
                    _right,
                    Random.Range(minY, maxY));

                meteor.Initialize(speed, scale, _left, this);
            }
        }

        public void ReturnToPool(Meteor meteor)
        {
            meteor.gameObject.SetActive(false);
            _pool.Enqueue(meteor);
        }
    }
}