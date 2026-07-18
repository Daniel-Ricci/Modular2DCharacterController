using UnityEngine;

namespace Modular2DCharacterController.Runtime.Core
{
    /// <summary>
    /// Detects floor edges and ledge shapes around the character.
    ///
    /// Floor-edge detection answers questions like "will this movement carry
    /// the character off the ground?". Ledge detection answers questions like
    /// "is there a climbable edge in front of the character?".
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class EdgeDetector : MonoBehaviour
    {
        [Header("Floor Edge Detection")]

        [Tooltip("Layers considered valid ground when checking for floor edges.")]
        [SerializeField]
        private LayerMask groundLayers = ~0;

        [Tooltip(
            "How far outside the collider's horizontal bounds the standing edge probes check for ground.")]
        [SerializeField]
        [Min(0f)]
        private float floorEdgeProbeInset = 0.03f;

        [Tooltip(
            "How high above the collider bottom the floor edge probes start.")]
        [SerializeField]
        [Min(0f)]
        private float floorEdgeProbeStartHeight = 0.05f;

        [Tooltip(
            "How far below the collider bottom the floor edge probes search for ground.")]
        [SerializeField]
        [Min(0f)]
        private float floorEdgeProbeDownDistance = 0.12f;

        [Header("Projected Floor Edge Detection")]

        [Tooltip(
            "Extra distance added to projected edge checks. Useful for stopping fast movement before a ledge.")]
        [SerializeField]
        [Min(0f)]
        private float projectedEdgeForwardSkin = 0.03f;

        [Header("Ledge Detection")]

        [Tooltip("Layers considered valid ledges for ledge grab or pull-up checks.")]
        [SerializeField]
        private LayerMask ledgeLayers = ~0;

        [Tooltip(
            "Horizontal distance used to look for a wall face that can form a ledge.")]
        [SerializeField]
        [Min(0f)]
        private float ledgeWallCheckDistance = 0.12f;

        [Tooltip(
            "Height from the collider bottom used to check for the lower wall face.")]
        [SerializeField]
        [Min(0f)]
        private float ledgeLowerProbeHeight = 0.45f;

        [Tooltip(
            "Height from the collider bottom used to check that the upper ledge space is clear.")]
        [SerializeField]
        [Min(0f)]
        private float ledgeUpperProbeHeight = 1.2f;

        [Tooltip(
            "How far above the upper ledge probe to start the downward top-surface check.")]
        [SerializeField]
        [Min(0f)]
        private float ledgeTopProbeHeight = 0.25f;

        [Tooltip(
            "How far downward from the top probe to look for a ledge surface.")]
        [SerializeField]
        [Min(0f)]
        private float ledgeTopProbeDownDistance = 0.6f;

        [Tooltip(
            "Minimum upward normal required for the ledge top surface.")]
        [SerializeField]
        [Range(0f, 1f)]
        private float minimumLedgeTopNormalY = 0.5f;

        public bool HasGroundAtLeftEdge { get; private set; }

        public bool HasGroundAtRightEdge { get; private set; }

        public bool IsAtLeftFloorEdge =>
            !HasGroundAtLeftEdge;

        public bool IsAtRightFloorEdge =>
            !HasGroundAtRightEdge;

        public bool IsAtAnyFloorEdge =>
            IsAtLeftFloorEdge ||
            IsAtRightFloorEdge;

        public Vector2 LeftFloorPoint { get; private set; }

        public Vector2 RightFloorPoint { get; private set; }

        public Vector2 LeftFloorNormal { get; private set; } = Vector2.up;

        public Vector2 RightFloorNormal { get; private set; } = Vector2.up;

        public Collider2D LeftFloorCollider { get; private set; }

        public Collider2D RightFloorCollider { get; private set; }

        public bool HasLeftLedge { get; private set; }

        public bool HasRightLedge { get; private set; }

        public bool HasAnyLedge =>
            HasLeftLedge ||
            HasRightLedge;

        public Vector2 LeftLedgePoint { get; private set; }

        public Vector2 RightLedgePoint { get; private set; }

        public Vector2 LeftLedgeNormal { get; private set; } = Vector2.up;

        public Vector2 RightLedgeNormal { get; private set; } = Vector2.up;

        public Collider2D LeftLedgeCollider { get; private set; }

        public Collider2D RightLedgeCollider { get; private set; }

        private Collider2D _characterCollider;
        private ContactFilter2D _groundFilter;
        private ContactFilter2D _ledgeFilter;

        private readonly RaycastHit2D[] _groundResults =
            new RaycastHit2D[4];

        private readonly RaycastHit2D[] _ledgeResults =
            new RaycastHit2D[4];

        private void Awake()
        {
            _characterCollider = GetComponent<Collider2D>();

            _groundFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = groundLayers,
                useTriggers = false
            };

            _ledgeFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = ledgeLayers,
                useTriggers = false
            };
        }

        private void FixedUpdate()
        {
            UpdateFloorEdgeState();
            UpdateLedgeState();
        }

        public bool WouldLeaveGround(float horizontalVelocity)
        {
            if (Mathf.Abs(horizontalVelocity) < 0.01f)
                return false;

            float direction =
                Mathf.Sign(horizontalVelocity);

            float projectedDistance =
                Mathf.Abs(horizontalVelocity) *
                Time.fixedDeltaTime +
                projectedEdgeForwardSkin;

            RaycastHit2D hit =
                FindFloorHit(direction, projectedDistance);

            return hit.collider == null;
        }

        public bool WouldLeaveGround(Vector2 velocity)
        {
            return WouldLeaveGround(velocity.x);
        }

        private void UpdateFloorEdgeState()
        {
            RaycastHit2D leftHit =
                FindFloorHit(-1f, floorEdgeProbeInset);

            RaycastHit2D rightHit =
                FindFloorHit(1f, floorEdgeProbeInset);

            HasGroundAtLeftEdge = leftHit.collider != null;
            HasGroundAtRightEdge = rightHit.collider != null;

            LeftFloorPoint =
                HasGroundAtLeftEdge ? leftHit.point : Vector2.zero;

            RightFloorPoint =
                HasGroundAtRightEdge ? rightHit.point : Vector2.zero;

            LeftFloorNormal =
                HasGroundAtLeftEdge ? leftHit.normal : Vector2.up;

            RightFloorNormal =
                HasGroundAtRightEdge ? rightHit.normal : Vector2.up;

            LeftFloorCollider =
                leftHit.collider;

            RightFloorCollider =
                rightHit.collider;
        }

        private void UpdateLedgeState()
        {
            UpdateLedgeStateForDirection(-1f);
            UpdateLedgeStateForDirection(1f);
        }

        private void UpdateLedgeStateForDirection(float direction)
        {
            bool hasLowerWall =
                HasWallAtHeight(direction, ledgeLowerProbeHeight);

            bool hasUpperWall =
                HasWallAtHeight(direction, ledgeUpperProbeHeight);

            RaycastHit2D topHit =
                hasLowerWall && !hasUpperWall
                    ? FindLedgeTopHit(direction)
                    : default;

            bool hasLedge =
                topHit.collider != null;

            if (direction < 0f)
            {
                HasLeftLedge = hasLedge;
                LeftLedgePoint = hasLedge ? topHit.point : Vector2.zero;
                LeftLedgeNormal = hasLedge ? topHit.normal : Vector2.up;
                LeftLedgeCollider = topHit.collider;
                return;
            }

            HasRightLedge = hasLedge;
            RightLedgePoint = hasLedge ? topHit.point : Vector2.zero;
            RightLedgeNormal = hasLedge ? topHit.normal : Vector2.up;
            RightLedgeCollider = topHit.collider;
        }

        private RaycastHit2D FindFloorHit(
            float direction,
            float horizontalOffset)
        {
            Bounds bounds =
                _characterCollider.bounds;

            Vector2 origin =
                new(
                    direction > 0f
                        ? bounds.max.x + horizontalOffset
                        : bounds.min.x - horizontalOffset,
                    bounds.min.y + floorEdgeProbeStartHeight);

            float distance =
                floorEdgeProbeStartHeight +
                floorEdgeProbeDownDistance;

            return FindBestDownHit(
                origin,
                distance,
                _groundFilter,
                _groundResults,
                0f);
        }

        private bool HasWallAtHeight(
            float direction,
            float heightFromBottom)
        {
            Bounds bounds =
                _characterCollider.bounds;

            Vector2 origin =
                new(
                    direction > 0f ? bounds.max.x : bounds.min.x,
                    bounds.min.y + heightFromBottom);

            int hitCount =
                Physics2D.Raycast(
                    origin,
                    new Vector2(direction, 0f),
                    _ledgeFilter,
                    _ledgeResults,
                    ledgeWallCheckDistance);

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D hit =
                    _ledgeResults[i];

                if (!IsValidExternalCollider(hit.collider))
                    continue;

                return true;
            }

            return false;
        }

        private RaycastHit2D FindLedgeTopHit(float direction)
        {
            Bounds bounds =
                _characterCollider.bounds;

            Vector2 origin =
                new(
                    direction > 0f
                        ? bounds.max.x + ledgeWallCheckDistance
                        : bounds.min.x - ledgeWallCheckDistance,
                    bounds.min.y +
                    ledgeUpperProbeHeight +
                    ledgeTopProbeHeight);

            return FindBestDownHit(
                origin,
                ledgeTopProbeDownDistance,
                _ledgeFilter,
                _ledgeResults,
                minimumLedgeTopNormalY);
        }

        private RaycastHit2D FindBestDownHit(
            Vector2 origin,
            float distance,
            ContactFilter2D filter,
            RaycastHit2D[] results,
            float minimumNormalY)
        {
            int hitCount =
                Physics2D.Raycast(
                    origin,
                    Vector2.down,
                    filter,
                    results,
                    distance);

            RaycastHit2D bestHit = default;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D hit =
                    results[i];

                if (!IsValidExternalCollider(hit.collider))
                    continue;

                if (hit.normal.y < minimumNormalY)
                    continue;

                if (bestHit.collider == null ||
                    hit.distance < bestHit.distance)
                {
                    bestHit = hit;
                }
            }

            return bestHit;
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
    }
}
