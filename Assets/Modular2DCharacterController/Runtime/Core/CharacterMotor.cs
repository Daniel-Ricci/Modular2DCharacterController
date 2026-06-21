using UnityEngine;

namespace Modular2DCharacterController.Runtime.Core
{
    /// <summary>
    /// Centralized Rigidbody2D motor for the character.
    ///
    /// Features should not write directly to Rigidbody2D.
    /// Instead, they submit velocity, acceleration, gravity modifiers,
    /// or external movement requests.
    ///
    /// The motor combines all requests and applies the final Rigidbody2D velocity
    /// once per physics step.
    /// </summary>
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterMotor : MonoBehaviour
    {
        [Header("Gravity")]

        [Tooltip(
            "If enabled, this motor applies custom gravity every physics step. " +
            "The Rigidbody2D gravity scale is set to 0 on Awake to avoid double gravity.")]
        [SerializeField]
        private bool useCustomGravity = true;

        [Tooltip(
            "Base gravity acceleration applied by the motor. " +
            "Use a negative value to pull the character downward.")]
        [SerializeField]
        private float gravityAcceleration = -45f;

        [Header("Fall Speed Limit")]

        [Tooltip(
            "Maximum fall speed possible after applying gravity. " +
            "Use 0 or below to disable the fall speed clamp.")]
        [SerializeField]
        private float maxFallSpeed = 25f;

        private Rigidbody2D _rigidbody;

        // Velocity at the start of the frame context.
        // Considers only self applied velocity, ignoring
        // all external velocity applied on the last frame.
        // Will be considered as base velocity to apply all modifiers on,
        // unless overriden by Set Velocity.
        private Vector2 _frameStartSelfVelocity;

        // Keeps track of external velocity applied over the
        // player, such as moving platforms.
        private Vector2 _externalVelocity;

        // Keeps track of the external velocity applied in
        // the last frame. Used to separate external movement
        // from the Rigidbody velocity when rebuilding the next
        // frame context.
        private Vector2 _lastAppliedExternalVelocity;

        // Velocity to be added upon the frame start self velocity, if
        // not overriden.
        private Vector2 _additiveSelfVelocity;

        // Acceleration to be added upon the frame start self velocity, if
        // not overriden.
        private Vector2 _selfAcceleration;

        // When setting a velocity directly, through the
        // set velocity methods, (horizontal, vertical or both)
        // they will override the frame start self velocity as
        // the base velocity for calculations on the current frame.
        // Additive velocity will still be applied over them.
        private bool _hasSelfVelocityOverride;
        private bool _hasHorizontalSelfVelocityOverride;
        private bool _hasVerticalSelfVelocityOverride;
        private Vector2 _selfVelocityOverride;
        private float _horizontalSelfVelocityOverride;
        private float _verticalSelfVelocityOverride;

        // Same logic is applied to external velocity.
        private bool _hasExternalVelocityOverride;
        private bool _hasHorizontalExternalVelocityOverride;
        private bool _hasVerticalExternalVelocityOverride;
        private Vector2 _externalVelocityOverride;
        private float _horizontalExternalVelocityOverride;
        private float _verticalExternalVelocityOverride;

        // Acceleration to be added to the external velocity applied each frame.
        private Vector2 _externalAcceleration;

        // If true, ignore effects of external sources this frame.
        private bool _externalVelocitySuppressed;

        // Multiplier to be applied to the gravity over the player.
        private float _gravityMultiplier = 1f;

        // If true, ignore effects of the gravity this frame.
        private bool _gravitySuppressed;

        // Indicates whether the frame context has been built.
        // The first motor access of a frame reconstructs the
        // current movement state and clears pending requests.
        private bool _frameContextInitialized;

#region Getters
        public float GravityAcceleration => gravityAcceleration;

        public Vector2 CurrentSelfVelocity
        {
            get
            {
                EnsureFrameContext();
                return ResolveSelfVelocity();
            }
        }

        public Vector2 ExternalVelocity
        {
            get
            {
                EnsureFrameContext();
                return ResolveAppliedExternalVelocity();
            }
        }

        public Vector2 FinalVelocity
        {
            get
            {
                EnsureFrameContext();
                return ResolveSelfVelocity() + ResolveAppliedExternalVelocity();
            }
        }
#endregion

#region Setters
        public void SetSelfVelocity(Vector2 velocity)
        {
            EnsureFrameContext();

            _hasSelfVelocityOverride = true;
            _selfVelocityOverride = velocity;

            _hasHorizontalSelfVelocityOverride = false;
            _hasVerticalSelfVelocityOverride = false;
        }

        public void SetHorizontalSelfVelocity(float velocity)
        {
            EnsureFrameContext();

            if (_hasSelfVelocityOverride)
                return;

            _hasHorizontalSelfVelocityOverride = true;
            _horizontalSelfVelocityOverride = velocity;
        }

        public void SetVerticalSelfVelocity(float velocity)
        {
            EnsureFrameContext();

            if (_hasSelfVelocityOverride)
                return;

            _hasVerticalSelfVelocityOverride = true;
            _verticalSelfVelocityOverride = velocity;
        }

        public void AddSelfVelocity(Vector2 velocity)
        {
            EnsureFrameContext();
            _additiveSelfVelocity += velocity;
        }

        public void AddSelfAcceleration(Vector2 acceleration)
        {
            EnsureFrameContext();
            _selfAcceleration += acceleration;
        }

        public void SetExternalVelocity(Vector2 velocity)
        {
            EnsureFrameContext();

            _hasExternalVelocityOverride = true;
            _externalVelocityOverride = velocity;

            _hasHorizontalExternalVelocityOverride = false;
            _hasVerticalExternalVelocityOverride = false;
        }

        public void SetHorizontalExternalVelocity(float velocity)
        {
            EnsureFrameContext();

            if (_hasExternalVelocityOverride)
                return;

            _hasHorizontalExternalVelocityOverride = true;
            _horizontalExternalVelocityOverride = velocity;
        }

        public void SetVerticalExternalVelocity(float velocity)
        {
            EnsureFrameContext();

            if (_hasExternalVelocityOverride)
                return;

            _hasVerticalExternalVelocityOverride = true;
            _verticalExternalVelocityOverride = velocity;
        }

        public void AddExternalVelocity(Vector2 velocity)
        {
            EnsureFrameContext();
            _externalVelocity += velocity;
        }

        public void AddExternalAcceleration(Vector2 acceleration)
        {
            EnsureFrameContext();
            _externalAcceleration += acceleration;
        }

        public void SuppressAllExternalVelocityThisFrame()
        {
            EnsureFrameContext();
            _externalVelocitySuppressed = true;
        }

        public void AddGravityMultiplier(float multiplier)
        {
            EnsureFrameContext();
            _gravityMultiplier *= multiplier;
        }

        public void SuppressGravityThisFrame()
        {
            EnsureFrameContext();
            _gravitySuppressed = true;
        }

        public void StopSelfMovement()
        {
            SetSelfVelocity(Vector2.zero);
        }

        public void StopExternalMovement()
        {
            SetExternalVelocity(Vector2.zero);
        }
#endregion

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();

            if (useCustomGravity)
            {
                _rigidbody.gravityScale = 0f;
            }
        }

        private void FixedUpdate()
        {
            Apply();
        }

        // Resolves all movement requests submitted during the current physics frame
        // and applies the resulting velocity to the Rigidbody.
        //
        // This marks the end of the motor's frame lifecycle:
        //
        //     Build Frame Context
        //         |
        //     Features submit movement requests
        //         |
        //     Resolve character velocity
        //         |
        //     Resolve external velocity
        //         |
        //     Apply final Rigidbody velocity
        //         |
        //     Invalidate frame context
        //
        // The final velocity applied to the Rigidbody is:
        //
        //     Character Velocity + External Velocity
        //
        // Once applied, the frame context is invalidated so a fresh context will be
        // reconstructed from the Rigidbody state on the next motor access.
        private void Apply()
        {
            EnsureFrameContext();

            Vector2 resolvedSelfVelocity =
                ResolveSelfVelocity();

            Vector2 resolvedExternalVelocity =
                ResolveAppliedExternalVelocity();

            Vector2 finalVelocity =
                resolvedSelfVelocity +
                resolvedExternalVelocity;

            if (maxFallSpeed > 0f &&
                finalVelocity.y < -maxFallSpeed)
            {
                finalVelocity.y = -maxFallSpeed;
            }

            _rigidbody.linearVelocity = finalVelocity;

            _lastAppliedExternalVelocity =
                resolvedExternalVelocity;

            InvalidateFrameContext();
        }

        // Reconstruct current character movement state
        // from the Rigidbody before collecting requests
        // for the new physics frame.
        private void EnsureFrameContext()
        {
            if (_frameContextInitialized)
                return;

            _frameStartSelfVelocity =
                _rigidbody.linearVelocity -
                _lastAppliedExternalVelocity;

            _externalVelocity = Vector2.zero;

            _additiveSelfVelocity = Vector2.zero;
            _selfAcceleration = Vector2.zero;

            _hasSelfVelocityOverride = false;
            _hasHorizontalSelfVelocityOverride = false;
            _hasVerticalSelfVelocityOverride = false;
            _selfVelocityOverride = Vector2.zero;
            _horizontalSelfVelocityOverride = 0f;
            _verticalSelfVelocityOverride = 0f;

            _hasExternalVelocityOverride = false;
            _hasHorizontalExternalVelocityOverride = false;
            _hasVerticalExternalVelocityOverride = false;
            _externalVelocityOverride = Vector2.zero;
            _horizontalExternalVelocityOverride = 0f;
            _verticalExternalVelocityOverride = 0f;

            _externalAcceleration = Vector2.zero;
            _externalVelocitySuppressed = false;

            _gravityMultiplier = 1f;
            _gravitySuppressed = false;

            _frameContextInitialized = true;
        }

        // Mark the current frame context as invalid.
        // The next motor access will rebuild the frame context
        // from the current Rigidbody velocity and reset all
        // pending movement requests.
        private void InvalidateFrameContext()
        {
            _frameContextInitialized = false;
        }

        // Calculate self-applied velocity (including gravity)
        private Vector2 ResolveSelfVelocity()
        {
            Vector2 resolvedSelfVelocity =
                _frameStartSelfVelocity;

            if (_hasSelfVelocityOverride)
            {
                resolvedSelfVelocity =
                    _selfVelocityOverride;
            }
            else
            {
                if (_hasHorizontalSelfVelocityOverride)
                {
                    resolvedSelfVelocity.x =
                        _horizontalSelfVelocityOverride;
                }

                if (_hasVerticalSelfVelocityOverride)
                {
                    resolvedSelfVelocity.y =
                        _verticalSelfVelocityOverride;
                }
            }

            resolvedSelfVelocity +=
                _additiveSelfVelocity;

            resolvedSelfVelocity +=
                _selfAcceleration * Time.fixedDeltaTime;

            if (useCustomGravity && !_gravitySuppressed)
            {
                resolvedSelfVelocity.y +=
                    gravityAcceleration *
                    _gravityMultiplier *
                    Time.fixedDeltaTime;
            }

            return resolvedSelfVelocity;
        }

        // Calculate velocity applied by external sources
        private Vector2 ResolveExternalVelocity()
        {
            Vector2 resolvedExternalVelocity =
                _externalVelocity;

            if (_hasExternalVelocityOverride)
            {
                resolvedExternalVelocity =
                    _externalVelocityOverride;
            }
            else
            {
                if (_hasHorizontalExternalVelocityOverride)
                {
                    resolvedExternalVelocity.x =
                        _horizontalExternalVelocityOverride;
                }

                if (_hasVerticalExternalVelocityOverride)
                {
                    resolvedExternalVelocity.y =
                        _verticalExternalVelocityOverride;
                }
            }

            resolvedExternalVelocity +=
                _externalAcceleration * Time.fixedDeltaTime;

            return resolvedExternalVelocity;
        }

        // Calculate the external velocity that will actually be applied this frame.
        private Vector2 ResolveAppliedExternalVelocity()
        {
            if (_externalVelocitySuppressed)
                return Vector2.zero;

            return ResolveExternalVelocity();
        }
    }
}