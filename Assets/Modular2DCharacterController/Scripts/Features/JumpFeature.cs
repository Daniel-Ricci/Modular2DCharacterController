using UnityEngine;
using Modular2DCharacterController.Core;
using Modular2DCharacterController.Data;
using Modular2DCharacterController.Input;

namespace Modular2DCharacterController.Features
{
    /// <summary>
    /// Handles jumping, air jumps, coyote time,
    /// jump buffering and custom gravity.
    /// </summary>
    [RequireComponent(typeof(GroundDetector))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class JumpFeature : MonoBehaviour, ICharacterFeature
    {
        [Header("Jump Feel")]
        [SerializeField] private JumpSettings settings;

        [Header("Gameplay")]
        [SerializeField]
        [Min(1)]
        private int maxJumpCount = 2;

        [SerializeField]
        [Min(0f)]
        private float coyoteTime = 0.1f;

        [SerializeField]
        [Min(0f)]
        private float jumpBufferTime = 0.1f;

        [Header("Jump Type")]
        [SerializeField]
        private bool fixedJumpHeight = false;

        [SerializeField]
        [Min(1f)]
        private float jumpReleaseGravityMultiplier = 3f;

        [Header("Jump Hang Time")]
        [SerializeField]
        private bool enableJumpHangTime = true;

        [SerializeField]
        [Min(0f)]
        private float jumpHangVelocityThreshold = 1f;

        [SerializeField]
        [Range(0.1f, 1f)]
        private float jumpHangGravityMultiplier = 0.35f;

        private CharacterMotor _motor;
        private ICharacterInput _input;
        private GroundDetector _groundDetector;
        private Rigidbody2D _rigidbody;

        private float _gravity;
        private float _jumpVelocity;

        private float _coyoteTimer;
        private float _jumpBufferTimer;

        private int _remainingJumps;

        private bool _wasGrounded;
        private bool _jumpRequested;

        private void Awake()
        {
            _motor = GetComponent<CharacterMotor>();
            _input = GetComponent<ICharacterInput>();
            _groundDetector = GetComponent<GroundDetector>();
            _rigidbody = GetComponent<Rigidbody2D>();

            _rigidbody.gravityScale = 0f;

            CalculateJumpValues();

            _remainingJumps = maxJumpCount;
        }

        public void Tick()
        {
            if (_input.JumpPressed)
            {
                _jumpRequested = true;
            }
        }

        public void FixedTick()
        {
            UpdateGroundState();
            UpdateTimers();
            TryJump();
            ApplyCustomGravity();
        }

        private void CalculateJumpValues()
        {
            _gravity =
                -(2f * settings.JumpHeight) /
                (settings.TimeToApex * settings.TimeToApex);

            _jumpVelocity =
                Mathf.Abs(_gravity) *
                settings.TimeToApex;
        }

        private void UpdateGroundState()
        {
            bool isGrounded = _groundDetector.IsGrounded;

            if (isGrounded && !_wasGrounded)
            {
                _remainingJumps = maxJumpCount;
            }

            _wasGrounded = isGrounded;
        }

        private void UpdateTimers()
        {
            if (_groundDetector.IsGrounded)
            {
                _coyoteTimer = coyoteTime;
            }
            else
            {
                _coyoteTimer -= Time.fixedDeltaTime;
            }

            if (_jumpRequested)
            {
                _jumpBufferTimer = jumpBufferTime;
                _jumpRequested = false;
            }
            else
            {
                _jumpBufferTimer -= Time.fixedDeltaTime;
            }
        }

        private void TryJump()
        {
            if (_jumpBufferTimer <= 0f)
            {
                return;
            }

            bool canGroundJump =
                _groundDetector.IsGrounded ||
                _coyoteTimer > 0f;

            bool canAirJump =
                !canGroundJump &&
                _remainingJumps > 0;

            if (!canGroundJump && !canAirJump)
            {
                return;
            }

            if (canGroundJump)
            {
                _remainingJumps = maxJumpCount - 1;
            }
            else
            {
                _remainingJumps--;
            }

            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;

            _motor.SetVerticalVelocity(_jumpVelocity);
        }

        private void ApplyCustomGravity()
        {
            float gravityMultiplier = 1f;

            // Falling
            if (_motor.VerticalVelocity < 0f)
            {
                gravityMultiplier =
                    settings.FallGravityMultiplier;
            }
            // Variable jump height
            else if (
                !fixedJumpHeight &&
                _motor.VerticalVelocity > 0f &&
                !_input.JumpHeld)
            {
                gravityMultiplier =
                    jumpReleaseGravityMultiplier;
            }

            // Jump hang time (only while ascending near apex)
            if (
                enableJumpHangTime &&
                _motor.VerticalVelocity > 0f &&
                _motor.VerticalVelocity <= jumpHangVelocityThreshold)
            {
                gravityMultiplier *=
                    jumpHangGravityMultiplier;
            }

            _rigidbody.AddForce(
                Vector2.up *
                _gravity *
                gravityMultiplier *
                _rigidbody.mass,
                ForceMode2D.Force);
        }
    }
}