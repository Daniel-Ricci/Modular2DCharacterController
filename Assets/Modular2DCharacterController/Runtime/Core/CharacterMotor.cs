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
        private float gravityAcceleration = -35f;

        [Tooltip(
            "Maximum downward speed caused by gravity. " +
            "Use 0 or below to disable the fall speed clamp.")]
        [SerializeField]
        private float maxFallSpeed = 25f;

        private Rigidbody2D _rigidbody;

        private Vector2 _baseVelocity;
        private Vector2 _selfVelocity;
        private Vector2 _externalVelocity;
        private Vector2 _lastAppliedExternalVelocity;

        private Vector2 _additiveVelocity;
        private Vector2 _acceleration;

        private bool _hasSelfVelocityOverride;
        private Vector2 _selfVelocityOverride;

        private bool _hasHorizontalVelocityOverride;
        private bool _hasVerticalVelocityOverride;
        private float _horizontalVelocityOverride;
        private float _verticalVelocityOverride;

        private bool _hasExternalVelocityOverride;
        private Vector2 _externalVelocityOverride;

        private float _gravityMultiplier = 1f;
        private bool _gravitySuppressed;

        private bool _requestBufferInitialized;

        public float GravityAcceleration => gravityAcceleration;

        public bool UseCustomGravity => useCustomGravity;

        public Vector2 CurrentVelocity
        {
            get
            {
                EnsureRequestBuffer();
                return _selfVelocity;
            }
        }

        public Vector2 ExternalVelocity
        {
            get
            {
                EnsureRequestBuffer();
                return ResolveExternalVelocity();
            }
        }

        public Vector2 FinalVelocity
        {
            get
            {
                EnsureRequestBuffer();
                return ResolveFinalVelocity();
            }
        }

        public float HorizontalVelocity
        {
            get
            {
                EnsureRequestBuffer();
                return _selfVelocity.x;
            }
        }

        public float VerticalVelocity
        {
            get
            {
                EnsureRequestBuffer();
                return _selfVelocity.y;
            }
        }

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

        public void SetVelocity(Vector2 velocity)
        {
            EnsureRequestBuffer();

            _hasSelfVelocityOverride = true;
            _selfVelocityOverride = velocity;
        }

        public void SetHorizontalVelocity(float velocity)
        {
            EnsureRequestBuffer();

            _hasHorizontalVelocityOverride = true;
            _horizontalVelocityOverride = velocity;
        }

        public void SetVerticalVelocity(float velocity)
        {
            EnsureRequestBuffer();

            _hasVerticalVelocityOverride = true;
            _verticalVelocityOverride = velocity;
        }

        public void AddVelocity(Vector2 velocity)
        {
            EnsureRequestBuffer();

            _additiveVelocity += velocity;
        }

        public void AddAcceleration(Vector2 acceleration)
        {
            EnsureRequestBuffer();

            _acceleration += acceleration;
        }

        public void SetExternalVelocity(Vector2 velocity)
        {
            EnsureRequestBuffer();

            _hasExternalVelocityOverride = true;
            _externalVelocityOverride = velocity;
        }

        public void AddExternalVelocity(Vector2 velocity)
        {
            EnsureRequestBuffer();

            _externalVelocity += velocity;
        }

        public void AddGravityMultiplier(float multiplier)
        {
            EnsureRequestBuffer();

            _gravityMultiplier *= multiplier;
        }

        public void SuppressGravityThisFrame()
        {
            EnsureRequestBuffer();

            _gravitySuppressed = true;
        }

        public void StopHorizontalMovement()
        {
            SetHorizontalVelocity(0f);
        }

        public void StopVerticalMovement()
        {
            SetVerticalVelocity(0f);
        }

        public void Stop()
        {
            SetVelocity(Vector2.zero);
            SetExternalVelocity(Vector2.zero);
            _lastAppliedExternalVelocity = Vector2.zero;
        }

        public void MovePosition(Vector2 position)
        {
            _rigidbody.MovePosition(position);
        }

        private void Apply()
        {
            EnsureRequestBuffer();

            Vector2 resolvedSelfVelocity =
                ResolveSelfVelocity();

            Vector2 resolvedExternalVelocity =
                ResolveExternalVelocity();

            _rigidbody.linearVelocity =
                resolvedSelfVelocity + resolvedExternalVelocity;

            _lastAppliedExternalVelocity =
                resolvedExternalVelocity;

            ClearRequestBuffer();
        }

        private void EnsureRequestBuffer()
        {
            if (_requestBufferInitialized)
                return;

            _baseVelocity = _rigidbody.linearVelocity;

            _selfVelocity =
                _baseVelocity -
                _lastAppliedExternalVelocity;

            _externalVelocity = Vector2.zero;

            _additiveVelocity = Vector2.zero;
            _acceleration = Vector2.zero;

            _hasSelfVelocityOverride = false;
            _selfVelocityOverride = Vector2.zero;

            _hasHorizontalVelocityOverride = false;
            _hasVerticalVelocityOverride = false;
            _horizontalVelocityOverride = 0f;
            _verticalVelocityOverride = 0f;

            _hasExternalVelocityOverride = false;
            _externalVelocityOverride = Vector2.zero;

            _gravityMultiplier = 1f;
            _gravitySuppressed = false;

            _requestBufferInitialized = true;
        }

        private void ClearRequestBuffer()
        {
            _requestBufferInitialized = false;
        }

        private Vector2 ResolveSelfVelocity()
        {
            Vector2 resolvedSelfVelocity = _selfVelocity;

            if (_hasSelfVelocityOverride)
            {
                resolvedSelfVelocity = _selfVelocityOverride;
            }

            if (_hasHorizontalVelocityOverride)
            {
                resolvedSelfVelocity.x = _horizontalVelocityOverride;
            }

            if (_hasVerticalVelocityOverride)
            {
                resolvedSelfVelocity.y = _verticalVelocityOverride;
            }

            resolvedSelfVelocity += _additiveVelocity;
            resolvedSelfVelocity += _acceleration * Time.fixedDeltaTime;

            if (useCustomGravity && !_gravitySuppressed)
            {
                resolvedSelfVelocity.y +=
                    gravityAcceleration *
                    _gravityMultiplier *
                    Time.fixedDeltaTime;

                if (maxFallSpeed > 0f)
                {
                    resolvedSelfVelocity.y =
                        Mathf.Max(resolvedSelfVelocity.y, -maxFallSpeed);
                }
            }

            return resolvedSelfVelocity;
        }

        private Vector2 ResolveExternalVelocity()
        {
            Vector2 resolvedExternalVelocity = _externalVelocity;

            if (_hasExternalVelocityOverride)
            {
                resolvedExternalVelocity = _externalVelocityOverride;
            }

            return resolvedExternalVelocity;
        }

        private Vector2 ResolveFinalVelocity()
        {
            return ResolveSelfVelocity() + ResolveExternalVelocity();
        }
    }
}