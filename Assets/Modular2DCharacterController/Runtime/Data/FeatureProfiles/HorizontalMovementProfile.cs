using UnityEngine;

namespace Modular2DCharacterController.Runtime.Data.FeatureProfiles
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

        [Tooltip(
            "The maximum horizontal speed that can be reached while using this profile.")]
        [Min(0)]
        public float maxSpeed = 8.0f;

        [Tooltip(
            "The rate at which the character accelerates toward its target speed.")]
        [Min(0)]
        public float acceleration = 80.0f;

        [Tooltip(
            "The rate at which the character slows down when no movement input is provided.")]
        [Min(0)]
        public float deceleration = 100.0f;

        [Tooltip(
            "The rate at which the character changes direction when moving opposite to its current velocity. " +
            "Higher values produce snappier turns.")]
        [Min(0)]
        public float turnAcceleration = 150.0f;
    }
}