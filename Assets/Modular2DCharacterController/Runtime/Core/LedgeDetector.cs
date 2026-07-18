using Modular2DCharacterController.Runtime.Features;
using UnityEngine;

namespace Modular2DCharacterController.Runtime.Core
{
    /// <summary>
    /// Detects ledge-related information in the direction the character is facing.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(GroundDetector))]
    [RequireComponent(typeof(WallDetector))]
    public class LedgeDetector : MonoBehaviour
    {
        [Header("Ground Edge")]

        [Tooltip("How far ahead of the character to check for ground.")]
        [SerializeField]
        [Min(0f)]
        private float groundCheckDistanceAhead = 0.08f;

        [Header("High Ledge")]

        [Tooltip("How far above the character to check for clear wall space.")]
        [SerializeField]
        [Min(0f)]
        private float clearCheckHeight = 1.2f;

        public bool IsOnGroundEdge { get; private set; }

        public bool HasGroundAhead { get; private set; }

        public Vector2 GroundAheadPoint { get; private set; }

        public Vector2 GroundAheadNormal { get; private set; } = Vector2.up;

        public Collider2D GroundAheadCollider { get; private set; }

        public bool HasWallAhead { get; private set; }

        public bool IsClearAboveWall { get; private set; }

        public bool HasHighLedge =>
            HasWallAhead &&
            IsClearAboveWall;

        public Vector2 WallAheadPoint { get; private set; }

        public Vector2 WallAheadNormal { get; private set; }

        public Collider2D WallAheadCollider { get; private set; }

        private GroundDetector _groundDetector;
        private WallDetector _wallDetector;
        private HorizontalMovementFeature _horizontalMovementFeature;

        private void Awake()
        {
            _groundDetector = GetComponent<GroundDetector>();
            _wallDetector = GetComponent<WallDetector>();
            _horizontalMovementFeature = GetComponent<HorizontalMovementFeature>();
        }

        private void FixedUpdate()
        {
            Vector2 facingDirection =
                GetFacingDirection();

            UpdateGroundEdge(facingDirection);
            UpdateHighLedge(facingDirection);
        }

        private void UpdateGroundEdge(Vector2 facingDirection)
        {
            if (!_groundDetector.IsGrounded)
            {
                ClearGroundAheadData();
                IsOnGroundEdge = false;
                return;
            }

            Vector2 offset =
                facingDirection *
                groundCheckDistanceAhead;

            HasGroundAhead =
                _groundDetector.TryFindGroundAtOffset(
                    offset,
                    out RaycastHit2D groundHit);

            IsOnGroundEdge =
                !HasGroundAhead;

            if (!HasGroundAhead)
            {
                GroundAheadPoint = Vector2.zero;
                GroundAheadNormal = Vector2.up;
                GroundAheadCollider = null;
                return;
            }

            GroundAheadPoint = groundHit.point;
            GroundAheadNormal = groundHit.normal;
            GroundAheadCollider = groundHit.collider;
        }

        private void UpdateHighLedge(Vector2 facingDirection)
        {
            HasWallAhead =
                _wallDetector.TryFindWall(
                    facingDirection,
                    Vector2.zero,
                    out RaycastHit2D wallHit);

            if (HasWallAhead)
            {
                WallAheadPoint = wallHit.point;
                WallAheadNormal = wallHit.normal;
                WallAheadCollider = wallHit.collider;
            }
            else
            {
                WallAheadPoint = Vector2.zero;
                WallAheadNormal = Vector2.zero;
                WallAheadCollider = null;
            }

            IsClearAboveWall =
                !_wallDetector.TryFindWall(
                    facingDirection,
                    Vector2.up * clearCheckHeight,
                    out _);
        }

        private void ClearGroundAheadData()
        {
            HasGroundAhead = false;
            GroundAheadPoint = Vector2.zero;
            GroundAheadNormal = Vector2.up;
            GroundAheadCollider = null;
        }

        private Vector2 GetFacingDirection()
        {
            if (_horizontalMovementFeature != null)
            {
                return _horizontalMovementFeature.FacingDirection == FacingDirection.Right
                    ? Vector2.right
                    : Vector2.left;
            }

            return transform.lossyScale.x >= 0f
                ? Vector2.right
                : Vector2.left;
        }
    }
}
