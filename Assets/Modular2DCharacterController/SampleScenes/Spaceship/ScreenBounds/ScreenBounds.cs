using UnityEngine;

namespace Modular2DCharacterController.SampleScenes.Spaceship.ScreenBounds
{
    public class ScreenBounds : MonoBehaviour
    {
        [SerializeField]
        private RectTransform topBar;
        [SerializeField]
        private RectTransform bottomBar;
        [SerializeField]
        private float thickness = 1f;
    
        private Camera cam;

        void Start()
        {
            if (cam == null)
                cam = Camera.main;

            // World coordinates of the left/right edges
            float left = cam.ScreenToWorldPoint(new Vector3(0, 0, 0)).x;
            float right = cam.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;

            // World coordinates taking the UI bars into account
            float bottom = cam.ScreenToWorldPoint(
                new Vector3(0, bottomBar.rect.height, 0)).y;

            float top = cam.ScreenToWorldPoint(
                new Vector3(0, Screen.height - topBar.rect.height, 0)).y;

            CreateWall("Left",
                new Vector2(left - thickness / 2f, (top + bottom) / 2f),
                new Vector2(thickness, top - bottom));

            CreateWall("Right",
                new Vector2(right + thickness / 2f, (top + bottom) / 2f),
                new Vector2(thickness, top - bottom));

            CreateWall("Top",
                new Vector2((left + right) / 2f, top + thickness / 2f),
                new Vector2(right - left, thickness));

            CreateWall("Bottom",
                new Vector2((left + right) / 2f, bottom - thickness / 2f),
                new Vector2(right - left, thickness));
        }

        void CreateWall(string name, Vector2 position, Vector2 size)
        {
            GameObject wall = new GameObject(name + " Edge");
            wall.transform.SetParent(transform);
            wall.transform.position = position;

            BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
            col.size = size;
        }
    }
}