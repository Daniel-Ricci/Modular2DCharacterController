using UnityEngine;

namespace Modular2DCharacterController.Core
{
    /// <summary>
    /// Character motor based on Rigidbody2D forces.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class ForceMotor : MonoBehaviour, ICharacterMotor
    {
        [SerializeField] private float acceleration = 50f;

        private Rigidbody2D _rigidbody;

        public Vector2 Velocity => _rigidbody.linearVelocity;

        public float HorizontalVelocity => _rigidbody.linearVelocityX;

        public float VerticalVelocity => _rigidbody.linearVelocityY;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        public void SetHorizontalVelocity(float velocity)
        {
            float velocityDelta = velocity - _rigidbody.linearVelocityX;

            _rigidbody.AddForce(
                Vector2.right * velocityDelta * acceleration);
        }

        public void SetVerticalVelocity(float velocity)
        {
            float velocityDelta = velocity - _rigidbody.linearVelocityY;

            _rigidbody.AddForce(
                Vector2.up * velocityDelta * acceleration);
        }

        public void AddForce(
            Vector2 force,
            ForceMode2D forceMode = ForceMode2D.Force)
        {
            _rigidbody.AddForce(force, forceMode);
        }

        public void StopHorizontalMovement()
        {
            _rigidbody.linearVelocityX = 0f;
        }

        public void StopVerticalMovement()
        {
            _rigidbody.linearVelocityY = 0f;
        }
    }
}