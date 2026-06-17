using UnityEngine;

namespace Modular2DCharacterController.Runtime.Core
{
    /// <summary>
    /// Detects and exposes information about the wall in front of the character.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class WallDetector : MonoBehaviour
    {
        [Header("Wall Detection")]

        [Tooltip("Layers considered walls for wall checks.")]
        [SerializeField]
        private LayerMask wallLayers;
        
        [Tooltip("Distance in which to check for walls.")]
        [SerializeField]
        [Min(0.01f)]
        private float wallCheckDistance = 0.1f;

        private Collider2D _collider;

        private ContactFilter2D _filter;

        private readonly RaycastHit2D[] _results =
            new RaycastHit2D[8];

        public bool IsTouchingWall { get; private set; }

        public Vector2 WallNormal { get; private set; }

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();

            _filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = wallLayers,
                useTriggers = false
            };
        }

        private void FixedUpdate()
        {
            UpdateWallState();
        }

        private void UpdateWallState()
        {
            int hitCount = _collider.Cast(
                Vector2.right,
                _filter,
                _results,
                wallCheckDistance);

            RaycastHit2D bestHit = default;

            for (int i = 0; i < hitCount; i++)
            {
                if (Mathf.Abs(_results[i].normal.x) > 0.5f)
                {
                    bestHit = _results[i];
                    break;
                }
            }

            if (bestHit.collider != null)
            {
                IsTouchingWall = true;
                WallNormal = bestHit.normal;
                return;
            }

            hitCount = _collider.Cast(
                Vector2.left,
                _filter,
                _results,
                wallCheckDistance);

            for (int i = 0; i < hitCount; i++)
            {
                if (Mathf.Abs(_results[i].normal.x) > 0.5f)
                {
                    bestHit = _results[i];
                    break;
                }
            }

            IsTouchingWall = bestHit.collider != null;
            WallNormal = IsTouchingWall ? bestHit.normal : Vector2.zero;
        }
    }
}