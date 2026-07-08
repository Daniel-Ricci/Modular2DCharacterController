using UnityEngine;

namespace Modular2DCharacterController.SampleScenes.Spaceship.Background
{
    public class StarField : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject starPrefab;

        [Header("Field")]
        [SerializeField] private int starCount = 200;
        [SerializeField] private float width = 20f;
        [SerializeField] private float height = 10f;
        [SerializeField] private float speed = 2f;

        [Header("Stars")]
        [SerializeField] private Vector2 sizeRange = new(0.02f, 0.08f);
        [SerializeField] private Vector2 brightnessRange = new(0.6f, 1f);

        [Header("Twinkle")]
        [SerializeField, Range(0f, 1f)] private float twinkleChance = 0.15f;
        [SerializeField] private Vector2 twinkleSpeedRange = new(1f, 3f);

        private Star[] stars;

        private class Star
        {
            public Transform Transform;
            public SpriteRenderer Renderer;
            public bool Twinkles;
            public float TwinkleSpeed;
            public float PhaseOffset;
        }

        private void Start()
        {
            stars = new Star[starCount];

            for (int i = 0; i < starCount; i++)
            {
                GameObject starObject = Instantiate(starPrefab, transform);

                starObject.transform.position = new Vector3(
                    Random.Range(-width * 0.5f, width * 0.5f),
                    Random.Range(-height * 0.5f, height * 0.5f),
                    0f);

                float size = Random.Range(sizeRange.x, sizeRange.y);
                starObject.transform.localScale = Vector3.one * size;

                SpriteRenderer renderer = starObject.GetComponent<SpriteRenderer>();

                float brightness = Random.Range(brightnessRange.x, brightnessRange.y);
                renderer.color = new Color(brightness, brightness, brightness, 1f);

                stars[i] = new Star
                {
                    Transform = starObject.transform,
                    Renderer = renderer,
                    Twinkles = Random.value < twinkleChance,
                    TwinkleSpeed = Random.Range(twinkleSpeedRange.x, twinkleSpeedRange.y),
                    PhaseOffset = Random.Range(0f, Mathf.PI * 2f)
                };
            }
        }

        private void Update()
        {
            foreach (Star star in stars)
            {
                star.Transform.position += Vector3.left * speed * Time.deltaTime;

                if (star.Transform.position.x < -width * 0.5f)
                {
                    star.Transform.position = new Vector3(
                        width * 0.5f,
                        Random.Range(-height * 0.5f, height * 0.5f),
                        0f);
                }

                if (star.Twinkles)
                {
                    Color color = star.Renderer.color;

                    color.a = Mathf.Lerp(
                        0.4f,
                        1f,
                        (Mathf.Sin(Time.time * star.TwinkleSpeed + star.PhaseOffset) + 1f) * 0.5f);

                    star.Renderer.color = color;
                }
            }
        }
    }
}