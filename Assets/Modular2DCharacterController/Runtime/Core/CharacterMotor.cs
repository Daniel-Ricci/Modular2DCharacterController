using UnityEngine;

namespace Modular2DCharacterController.Runtime.Core
{
    /// <summary>
    /// Centralizes all character Rigidbody2D velocity operations.
    /// Supports separate velocity layers so external motion does not overwrite player movement.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterMotor : MonoBehaviour
    {
        private Rigidbody2D _rigidbody;

        private Vector2 _selfVelocity;
        private Vector2 _externalVelocity;
        private Vector2 _lastAppliedExternalVelocity;

        private bool _velocityStepStarted;

        public Vector2 Velocity
        {
            get
            {
                EnsureVelocityStepStarted();
                return _selfVelocity;
            }
        }

        public Vector2 FinalVelocity =>
            _selfVelocity + _externalVelocity;

        public float HorizontalVelocity
        {
            get
            {
                EnsureVelocityStepStarted();
                return _selfVelocity.x;
            }
        }

        public float VerticalVelocity
        {
            get
            {
                EnsureVelocityStepStarted();
                return _selfVelocity.y;
            }
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        public void BeginVelocityStep()
        {
            _selfVelocity =
                _rigidbody.linearVelocity -
                _lastAppliedExternalVelocity;

            _externalVelocity = Vector2.zero;
            _velocityStepStarted = true;
        }

        public void ApplyVelocity()
        {
            EnsureVelocityStepStarted();

            _rigidbody.linearVelocity =
                _selfVelocity + _externalVelocity;

            _lastAppliedExternalVelocity = _externalVelocity;
            _velocityStepStarted = false;
        }

        public void SetHorizontalVelocity(float velocity)
        {
            EnsureVelocityStepStarted();
            _selfVelocity.x = velocity;
        }

        public void SetVerticalVelocity(float velocity)
        {
            EnsureVelocityStepStarted();
            _selfVelocity.y = velocity;
        }

        public void SetVelocity(Vector2 velocity)
        {
            EnsureVelocityStepStarted();
            _selfVelocity = velocity;
        }

        public void AddVelocity(Vector2 velocity)
        {
            EnsureVelocityStepStarted();
            _selfVelocity += velocity;
        }

        public void SetExternalVelocity(Vector2 velocity)
        {
            EnsureVelocityStepStarted();
            _externalVelocity = velocity;
        }

        public void AddExternalVelocity(Vector2 velocity)
        {
            EnsureVelocityStepStarted();
            _externalVelocity += velocity;
        }

        public void AddForce(
            Vector2 force,
            ForceMode2D forceMode = ForceMode2D.Force)
        {
            _rigidbody.AddForce(force, forceMode);
        }

        public void StopHorizontalMovement()
        {
            EnsureVelocityStepStarted();
            _selfVelocity.x = 0f;
        }

        public void StopVerticalMovement()
        {
            EnsureVelocityStepStarted();
            _selfVelocity.y = 0f;
        }

        public void Stop()
        {
            EnsureVelocityStepStarted();
            _selfVelocity = Vector2.zero;
            _externalVelocity = Vector2.zero;
            _lastAppliedExternalVelocity = Vector2.zero;
        }

        public void MovePosition(Vector2 position)
        {
            _rigidbody.MovePosition(position);
        }

        private void EnsureVelocityStepStarted()
        {
            if (_velocityStepStarted)
                return;

            BeginVelocityStep();
        }
    }
}