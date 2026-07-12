using System;
using UnityEngine;

namespace Modular2DCharacterController.Runtime.Core
{
    /// <summary>
    /// Detects and exposes information about ceilings above the character.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class CeilingDetector : MonoBehaviour
    {
        [Header("Ceiling Detection")]

        [Tooltip("Layers considered valid ceilings for ceiling checks.")]
        [SerializeField]
        private LayerMask ceilingLayers;

        [Tooltip("The distance above the character collider to check for valid ceilings.")]
        [SerializeField]
        [Min(0f)]
        private float ceilingCheckDistance = 0.1f;

        [Tooltip(
            "Minimum downward normal required for a hit to count as a ceiling. " +
            "Higher values reject walls and shallow side contacts.")]
        [SerializeField]
        [Range(0f, 1f)]
        private float minimumCeilingNormalY = 0.5f;

        [Tooltip(
            "Minimum upward velocity required to fire CeilingHit. " +
            "This prevents slow overlap/touching states from triggering hit events.")]
        [SerializeField]
        [Min(0f)]
        private float ceilingHitVelocityThreshold = 0.1f;

        [Header("Standing Clearance")]

        [Tooltip(
            "Extra clearance subtracted from standing checks. " +
            "Small values help avoid false positives from touching contacts.")]
        [SerializeField]
        [Min(0f)]
        private float standCheckSkin = 0.01f;

        [Header("Platform Effectors")]

        [Tooltip(
            "If enabled, one-way PlatformEffector2D colliders are ignored when " +
            "the checked point is on their pass-through side.")]
        [SerializeField]
        private bool ignoreOneWayPlatformsFromPassThroughSide = true;

        [Tooltip(
            "Small tolerance used when deciding which side of a one-way platform " +
            "the checked point is on.")]
        [SerializeField]
        [Min(0f)]
        private float oneWaySideTolerance = 0.001f;

        public bool IsTouchingCeiling { get; private set; }

        public Vector2 CeilingNormal { get; private set; } = Vector2.down;

        public float CeilingAngle { get; private set; }

        public Vector2 CeilingPoint { get; private set; }

        public Collider2D CurrentCeilingCollider { get; private set; }

        public Transform CurrentCeilingTransform { get; private set; }

        public float StandCheckSkin => standCheckSkin;

        public event Action<GameObject> CeilingHit;

        private Collider2D _characterCollider;
        private Rigidbody2D _rigidbody;
        private CharacterMotor _motor;

        private readonly RaycastHit2D[] _castResults = new RaycastHit2D[8];
        private readonly Collider2D[] _overlapResults = new Collider2D[8];

        private ContactFilter2D _contactFilter;

        private void Awake()
        {
            _characterCollider = GetComponent<Collider2D>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _motor = GetComponent<CharacterMotor>();

            _contactFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = ceilingLayers,
                useTriggers = false
            };
        }

        private void FixedUpdate()
        {
            UpdateCeilingState();
        }

        public bool HasBlockingCeilingInBox(
            Vector2 center,
            Vector2 size,
            float angle)
        {
            return HasBlockingCeilingInBox(
                center,
                size,
                angle,
                null,
                out _);
        }

        public bool HasBlockingCeilingInBox(
            Vector2 center,
            Vector2 size,
            float angle,
            Collider2D[] passThroughOneWayResults,
            out int passThroughOneWayCount)
        {
            passThroughOneWayCount = 0;

            Vector2 checkedSize =
                new(
                    Mathf.Max(0.01f, size.x - standCheckSkin * 2f),
                    Mathf.Max(0.01f, size.y - standCheckSkin * 2f));

            int hitCount =
                Physics2D.OverlapBox(
                    center,
                    checkedSize,
                    angle,
                    _contactFilter,
                    _overlapResults);

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit =
                    _overlapResults[i];

                _overlapResults[i] = null;

                if (!IsValidExternalCollider(hit))
                    continue;

                if (IsOneWayPlatformPassThroughFromPoint(hit, center))
                {
                    if (passThroughOneWayResults != null &&
                        passThroughOneWayCount < passThroughOneWayResults.Length)
                    {
                        passThroughOneWayResults[passThroughOneWayCount] = hit;
                        passThroughOneWayCount++;
                    }

                    continue;
                }

                return true;
            }

            return false;
        }

        private void UpdateCeilingState()
        {
            RaycastHit2D bestHit =
                FindBestCeilingHit();

            bool wasTouchingCeiling =
                IsTouchingCeiling;

            if (bestHit.collider == null)
            {
                SetTouchingCeiling(false);
                ClearCeilingData();
                return;
            }

            SetTouchingCeiling(true);

            CeilingNormal = bestHit.normal;
            CeilingAngle = Vector2.Angle(bestHit.normal, Vector2.down);
            CeilingPoint = bestHit.point;
            CurrentCeilingCollider = bestHit.collider;
            CurrentCeilingTransform = bestHit.collider.transform;

            if (!wasTouchingCeiling &&
                WasMovingUpIntoCeiling())
            {
                CeilingHit?.Invoke(bestHit.collider.gameObject);
            }
        }

        private bool WasMovingUpIntoCeiling()
        {
            if (_rigidbody.linearVelocity.y >= ceilingHitVelocityThreshold)
                return true;

            if (_motor == null)
                return false;

            return _motor.LastResolvedFinalVelocity.y >= ceilingHitVelocityThreshold;
        }

        private RaycastHit2D FindBestCeilingHit()
        {
            int hitCount =
                _characterCollider.Cast(
                    Vector2.up,
                    _contactFilter,
                    _castResults,
                    ceilingCheckDistance);

            RaycastHit2D bestHit = default;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D hit =
                    _castResults[i];

                if (hit.collider == null)
                    continue;

                if (hit.normal.y > -minimumCeilingNormalY)
                    continue;

                if (!IsBlockingCeilingCollider(hit.collider, hit.point))
                    continue;

                if (bestHit.collider == null ||
                    hit.normal.y < bestHit.normal.y)
                {
                    bestHit = hit;
                }
            }

            return bestHit;
        }

        private void SetTouchingCeiling(bool touchingCeiling)
        {
            IsTouchingCeiling =
                touchingCeiling;
        }

        private void ClearCeilingData()
        {
            CeilingNormal = Vector2.down;
            CeilingAngle = 0f;
            CeilingPoint = Vector2.zero;
            CurrentCeilingCollider = null;
            CurrentCeilingTransform = null;
        }

        private bool IsBlockingCeilingCollider(
            Collider2D hit,
            Vector2 testPoint)
        {
            if (!IsValidExternalCollider(hit))
                return false;

            return !IsOneWayPlatformPassThroughFromPoint(
                hit,
                testPoint);
        }

        private bool IsValidExternalCollider(Collider2D hit)
        {
            if (hit == null)
                return false;

            if (hit == _characterCollider)
                return false;

            if (hit.transform == transform ||
                hit.transform.IsChildOf(transform))
            {
                return false;
            }

            return true;
        }

        private bool IsOneWayPlatformPassThroughFromPoint(
            Collider2D hit,
            Vector2 testPoint)
        {
            if (!ignoreOneWayPlatformsFromPassThroughSide)
                return false;

            PlatformEffector2D platformEffector =
                hit.GetComponent<PlatformEffector2D>();

            if (platformEffector == null ||
                !platformEffector.useOneWay)
            {
                return false;
            }

            Vector2 solidSideNormal =
                GetPlatformEffectorSolidSideNormal(platformEffector);

            Vector2 directionToCheckedPoint =
                testPoint - (Vector2)platformEffector.transform.position;

            return Vector2.Dot(directionToCheckedPoint, solidSideNormal) <=
                   oneWaySideTolerance;
        }

        private static Vector2 GetPlatformEffectorSolidSideNormal(
            PlatformEffector2D platformEffector)
        {
            Quaternion rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    platformEffector.rotationalOffset);

            Vector3 localSolidSide =
                rotation * Vector2.up;

            Vector3 worldSolidSide =
                platformEffector.transform.TransformDirection(localSolidSide);

            return ((Vector2)worldSolidSide).normalized;
        }
    }
}
