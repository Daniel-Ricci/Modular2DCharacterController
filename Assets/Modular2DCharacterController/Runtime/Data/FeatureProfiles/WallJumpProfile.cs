using UnityEngine;

namespace Modular2DCharacterController.Runtime.Data.FeatureProfiles
{
    /// <summary>
    /// Data profile used by the wall jump feature.
    /// </summary>
    [CreateAssetMenu(
        fileName = "WallJumpProfile",
        menuName = "Modular 2D Character Controller/Wall Jump Profile")]
    public class WallJumpProfile : FeatureProfile
    {
        [Header("Jump Force")]

        [Tooltip("Horizontal force applied during wall jump.")]
        [Min(0.1f)]
        public float horizontalForce = 8f;

        [Tooltip("Vertical force applied during wall jump.")]
        [Min(0.1f)]
        public float verticalForce = 14f;

        [Header("Control Lock")]

        [Tooltip(
            "Time duration in which the player controls are locked after a wall jump.")]
        [Min(0f)]
        public float movementLockDuration = 0.15f;
    }
}