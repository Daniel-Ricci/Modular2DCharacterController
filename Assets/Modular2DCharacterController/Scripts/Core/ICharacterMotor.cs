using UnityEngine;

namespace Modular2DCharacterController.Core
{
    /// <summary>
    /// Defines the contract for character movement implementations.
    /// </summary>
    public interface ICharacterMotor
    {
        float HorizontalVelocity { get; }

        float VerticalVelocity { get; }

        void SetHorizontalVelocity(float velocity);

        void SetVerticalVelocity(float velocity);

        void AddForce(Vector2 force, ForceMode2D forceMode = ForceMode2D.Force);

        void StopHorizontalMovement();

        void StopVerticalMovement();
    }
}