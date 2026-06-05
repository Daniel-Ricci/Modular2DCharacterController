using UnityEngine;

namespace Modular2DCharacterController.Core
{
    /// <summary>
    /// Detects and exposes information about the ground beneath the character.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class GroundDetector : MonoBehaviour
    {
        [Header("Ground Detection")]
        [SerializeField] private LayerMask groundLayers;
        [SerializeField] private float groundCheckDistance;

        [Header("Slope Filtering")]
        [SerializeField]
        [Range(0f, 1f)]
        private float minGroundNormalY = 0.65f;

        public bool IsGrounded { get; private set; }

        public Vector2 GroundNormal { get; private set; } = Vector2.up;

        public float GroundAngle { get; private set; }

        private Collider2D _characterCollider;
        private Rigidbody2D _rigidbody;

        private readonly RaycastHit2D[] _results = new RaycastHit2D[8];
        private ContactFilter2D _contactFilter;
        
        private float ascendingVelocityThreshold = 1f;

        private void Awake()
        {
            _characterCollider = GetComponent<Collider2D>();
            _rigidbody = GetComponent<Rigidbody2D>();

            _contactFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = groundLayers,
                useTriggers = false
            };
        }

        private void FixedUpdate()
        {
            UpdateGroundState();
        }

        private void UpdateGroundState()
        {
            // Prevent becoming grounded while actively moving upward.
            if (_rigidbody.linearVelocity.y > ascendingVelocityThreshold)
            {
                IsGrounded = false;
                GroundNormal = Vector2.up;
                GroundAngle = 0f;
                return;
            }

            int hitCount = _characterCollider.Cast(
                Vector2.down,
                _contactFilter,
                _results,
                groundCheckDistance);

            if (hitCount == 0)
            {
                IsGrounded = false;
                GroundNormal = Vector2.up;
                GroundAngle = 0f;
                return;
            }

            RaycastHit2D bestHit = _results[0];

            for (int i = 1; i < hitCount; i++)
            {
                if (_results[i].normal.y > bestHit.normal.y)
                {
                    bestHit = _results[i];
                }
            }

            if (bestHit.normal.y < minGroundNormalY)
            {
                IsGrounded = false;
                GroundNormal = Vector2.up;
                GroundAngle = 0f;
                return;
            }

            IsGrounded = true;
            GroundNormal = bestHit.normal;
            GroundAngle = Vector2.Angle(bestHit.normal, Vector2.up);
        }
    }
}