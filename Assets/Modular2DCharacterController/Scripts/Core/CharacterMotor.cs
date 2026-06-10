using UnityEngine;

namespace Modular2DCharacterController.Scripts.Core
{
    /// <summary>
    /// Provides a centralized interface for character physics operations.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterMotor : MonoBehaviour
    {
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
            _rigidbody.linearVelocityX = velocity;
        }

        public void SetVerticalVelocity(float velocity)
        {
            _rigidbody.linearVelocityY = velocity;
        }

        public void SetVelocity(Vector2 velocity)
        {
            _rigidbody.linearVelocity = velocity;
        }

        public void AddVelocity(Vector2 velocity)
        {
            _rigidbody.linearVelocity += velocity;
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

        public void Stop()
        {
            _rigidbody.linearVelocity = Vector2.zero;
        }
    }
}