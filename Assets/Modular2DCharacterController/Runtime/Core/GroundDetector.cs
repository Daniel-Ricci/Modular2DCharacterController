using System;
using UnityEngine;

namespace Modular2DCharacterController.Runtime.Core
{
    /// <summary>
    /// Detects and exposes information about the ground beneath the character.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class GroundDetector : MonoBehaviour
    {
        [Header("Ground Detection")]

        [Tooltip("Layers considered valid ground for grounding checks.")]
        [SerializeField]
        private LayerMask groundLayers;

        [Tooltip("The distance below the character collider to check for valid ground.")]
        [SerializeField]
        [Min(0f)]
        private float groundCheckDistance = 0.05f;

        [Header("Slope Filtering")]

        [Tooltip(
            "Minimum upward-facing normal required for a surface to be considered ground. " +
            "Lower values allow steeper slopes.")]
        [SerializeField]
        [Range(0f, 1f)]
        private float minGroundNormalY = 0.65f;

        [Header("Ascending Velocity Threshold")]

        [Tooltip(
            "The maximum upward velocity at which grounding is still allowed. " +
            "Prevents the character from becoming grounded while actively moving upward.")]
        [SerializeField]
        private float ascendingVelocityThreshold = 1f;

        [Header("Moving Platforms")]

        [Tooltip(
            "If enabled, Rigidbody2D.GetPointVelocity() will be used when standing on " +
            "a Rigidbody2D. This supports Rigidbody-driven moving platforms more reliably.")]
        [SerializeField]
        private bool useGroundRigidbodyVelocity = true;

        [Tooltip(
            "Very small platform deltas below this value are treated as zero. " +
            "Helps remove tiny floating-point noise.")]
        [SerializeField]
        [Min(0f)]
        private float groundDeltaDeadZone = 0.00001f;

        public bool IsGrounded { get; private set; }

        public Vector2 GroundNormal { get; private set; } = Vector2.up;

        public float GroundAngle { get; private set; }

        public Transform CurrentGroundTransform { get; private set; }

        public Vector2 GroundVelocity { get; private set; }

        public Vector2 GroundDelta { get; private set; }

        public event Action<Vector2> Landed;
        public event Action<Vector2> LeftGround;

        private Collider2D _characterCollider;
        private Rigidbody2D _rigidbody;
        private CharacterMotor _motor;

        private Vector2 _lastGroundPosition;

        private readonly RaycastHit2D[] _results = new RaycastHit2D[8];
        private ContactFilter2D _contactFilter;

        private void Awake()
        {
            _characterCollider = GetComponent<Collider2D>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _motor = GetComponent<CharacterMotor>();

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
            if (_rigidbody.linearVelocity.y > ascendingVelocityThreshold)
            {
                SetGrounded(false);
                ClearGroundData();

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
                SetGrounded(false);
                ClearGroundData();

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
                SetGrounded(false);
                ClearGroundData();

                GroundNormal = Vector2.up;
                GroundAngle = 0f;

                return;
            }

            SetGrounded(true);

            GroundNormal = bestHit.normal;
            GroundAngle = Vector2.Angle(bestHit.normal, Vector2.up);

            UpdateGroundData(bestHit);
        }

        private void UpdateGroundData(RaycastHit2D hit)
        {
            Transform groundTransform = hit.collider.transform;

            if (groundTransform == null)
            {
                ClearGroundData();
                return;
            }

            Vector2 currentGroundPosition = groundTransform.position;

            if (CurrentGroundTransform != groundTransform)
            {
                CurrentGroundTransform = groundTransform;
                _lastGroundPosition = currentGroundPosition;

                GroundVelocity = Vector2.zero;
                GroundDelta = Vector2.zero;

                return;
            }

            if (useGroundRigidbodyVelocity && hit.rigidbody != null)
            {
                GroundVelocity =
                    hit.rigidbody.GetPointVelocity(hit.point);

                GroundDelta =
                    GroundVelocity * Time.fixedDeltaTime;
            }
            else
            {
                GroundDelta =
                    currentGroundPosition - _lastGroundPosition;

                GroundVelocity =
                    Time.fixedDeltaTime > 0f
                        ? GroundDelta / Time.fixedDeltaTime
                        : Vector2.zero;
            }

            if (GroundDelta.sqrMagnitude <= groundDeltaDeadZone * groundDeltaDeadZone)
            {
                GroundDelta = Vector2.zero;
                GroundVelocity = Vector2.zero;
            }

            _lastGroundPosition = currentGroundPosition;
        }

        private void ClearGroundData()
        {
            CurrentGroundTransform = null;
            GroundVelocity = Vector2.zero;
            GroundDelta = Vector2.zero;
            _lastGroundPosition = Vector2.zero;
        }

        private void SetGrounded(bool grounded)
        {
            if (IsGrounded == grounded)
                return;

            IsGrounded = grounded;

            if (grounded)
                Landed?.Invoke(_motor.Velocity);
            else
                LeftGround?.Invoke(_motor.Velocity);
        }
    }
}