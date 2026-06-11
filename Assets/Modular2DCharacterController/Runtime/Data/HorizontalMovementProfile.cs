using UnityEngine;

namespace Modular2DCharacterController.Runtime.Data
{
    /// <summary>
    /// Data profile used by the horizontal movement feature.
    /// </summary>
    [CreateAssetMenu(
        fileName = "HorizontalMovementProfile",
        menuName = "Modular 2D Character Controller/Horizontal Movement Profile")]
    public class HorizontalMovementProfile : FeatureProfile
    {
        [Header("Movement Settings")]
        [Min(0)]
        public float maxSpeed = 8.0f;

        [Min(0)]
        public float acceleration = 80.0f;

        [Min(0)]
        public float deceleration = 100.0f;

        [Min(0)]
        public float turnAcceleration = 150.0f;
    }
}