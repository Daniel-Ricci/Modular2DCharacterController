using Modular2DCharacterController.Runtime.Features;
using UnityEngine;

namespace Modular2DCharacterController.Runtime.Core
{
    /// <summary>
    /// Read-only aggregation point for commonly used character status values.
    /// External systems can depend on this component instead of knowing which
    /// detector or feature owns each individual value.
    /// </summary>
    [DisallowMultipleComponent]
    public class CharacterStatusProvider : MonoBehaviour
    {
        private Rigidbody2D _rigidbody;
        private CharacterMotor _motor;
        private GroundDetector _groundDetector;
        private WallDetector _wallDetector;
        private CeilingDetector _ceilingDetector;
        private EdgeDetector _edgeDetector;
        private HorizontalMovementFeature _horizontalMovementFeature;
        private JumpFeature _jumpFeature;
        private RunFeature _runFeature;
        private DashFeature _dashFeature;
        private RollFeature _rollFeature;
        private CrouchFeature _crouchFeature;
        private WallSlideFeature _wallSlideFeature;
        private WallJumpFeature _wallJumpFeature;
        private GlideFeature _glideFeature;
        private GroundPoundFeature _groundPoundFeature;

        public Vector2 Velocity =>
            _rigidbody != null
                ? _rigidbody.linearVelocity
                : Vector2.zero;

        public Vector2 SelfVelocity =>
            _motor != null
                ? _motor.LastResolvedSelfVelocity
                : Vector2.zero;

        public Vector2 ExternalVelocity =>
            _motor != null
                ? _motor.LastResolvedExternalVelocity
                : Vector2.zero;

        public FacingDirection FacingDirection =>
            _horizontalMovementFeature != null
                ? _horizontalMovementFeature.FacingDirection
                : FacingDirection.Right;

        public bool IsGrounded =>
            _groundDetector != null &&
            _groundDetector.IsGrounded;

        public Vector2 GroundNormal =>
            _groundDetector != null
                ? _groundDetector.GroundNormal
                : Vector2.up;

        public float GroundAngle =>
            _groundDetector != null
                ? _groundDetector.GroundAngle
                : 0f;

        public Transform CurrentGroundTransform =>
            _groundDetector != null
                ? _groundDetector.CurrentGroundTransform
                : null;

        public bool IsTouchingWall =>
            _wallDetector != null &&
            _wallDetector.IsTouchingWall;

        public Vector2 WallNormal =>
            _wallDetector != null
                ? _wallDetector.WallNormal
                : Vector2.zero;

        public bool IsTouchingCeiling =>
            _ceilingDetector != null &&
            _ceilingDetector.IsTouchingCeiling;

        public Transform CurrentCeilingTransform =>
            _ceilingDetector != null
                ? _ceilingDetector.CurrentCeilingTransform
                : null;

        public bool IsAtLeftFloorEdge =>
            _edgeDetector != null &&
            _edgeDetector.IsAtLeftFloorEdge;

        public bool IsAtRightFloorEdge =>
            _edgeDetector != null &&
            _edgeDetector.IsAtRightFloorEdge;

        public bool IsAtAnyFloorEdge =>
            _edgeDetector != null &&
            _edgeDetector.IsAtAnyFloorEdge;

        public bool HasLeftLedge =>
            _edgeDetector != null &&
            _edgeDetector.HasLeftLedge;

        public bool HasRightLedge =>
            _edgeDetector != null &&
            _edgeDetector.HasRightLedge;

        public bool HasAnyLedge =>
            _edgeDetector != null &&
            _edgeDetector.HasAnyLedge;

        public bool IsJumpActive =>
            _jumpFeature != null &&
            _jumpFeature.IsJumpActive;

        public bool IsJumpAscending =>
            _jumpFeature != null &&
            _jumpFeature.IsJumpAscending;

        public int RemainingAirJumps =>
            _jumpFeature != null
                ? _jumpFeature.RemainingAirJumps
                : 0;

        public bool IsRunning =>
            _runFeature != null &&
            _runFeature.IsRunning;

        public bool IsDashing =>
            _dashFeature != null &&
            _dashFeature.IsDashing;

        public Vector2 DashDirection =>
            _dashFeature != null
                ? _dashFeature.DashDirection
                : Vector2.zero;

        public int RemainingDashes =>
            _dashFeature != null
                ? _dashFeature.RemainingDashes
                : 0;

        public bool IsRolling =>
            _rollFeature != null &&
            _rollFeature.IsRolling;

        public Vector2 RollDirection =>
            _rollFeature != null
                ? _rollFeature.RollDirection
                : Vector2.zero;

        public bool IsCrouching =>
            _crouchFeature != null &&
            _crouchFeature.IsCrouching;

        public bool IsStandBlocked =>
            _crouchFeature != null &&
            _crouchFeature.IsStandBlocked;

        public bool IsWallSliding =>
            _wallSlideFeature != null &&
            _wallSlideFeature.IsWallSliding;

        public bool IsWallJumpControlInfluenceActive =>
            _wallJumpFeature != null &&
            _wallJumpFeature.IsControlInfluenceActive;

        public bool IsGliding =>
            _glideFeature != null &&
            _glideFeature.IsGliding;

        public bool IsGroundPounding =>
            _groundPoundFeature != null &&
            _groundPoundFeature.IsGroundPounding;

        public bool IsGroundPoundRecoveryActive =>
            _groundPoundFeature != null &&
            _groundPoundFeature.IsRecoveryActive;

        private void Awake()
        {
            CacheComponents();
        }

        private void Reset()
        {
            CacheComponents();
        }

        private void CacheComponents()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _motor = GetComponent<CharacterMotor>();
            _groundDetector = GetComponent<GroundDetector>();
            _wallDetector = GetComponent<WallDetector>();
            _ceilingDetector = GetComponent<CeilingDetector>();
            _edgeDetector = GetComponent<EdgeDetector>();
            _horizontalMovementFeature = GetComponent<HorizontalMovementFeature>();
            _jumpFeature = GetComponent<JumpFeature>();
            _runFeature = GetComponent<RunFeature>();
            _dashFeature = GetComponent<DashFeature>();
            _rollFeature = GetComponent<RollFeature>();
            _crouchFeature = GetComponent<CrouchFeature>();
            _wallSlideFeature = GetComponent<WallSlideFeature>();
            _wallJumpFeature = GetComponent<WallJumpFeature>();
            _glideFeature = GetComponent<GlideFeature>();
            _groundPoundFeature = GetComponent<GroundPoundFeature>();
        }
    }
}
