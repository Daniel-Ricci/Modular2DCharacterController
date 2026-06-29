using UnityEngine;
using UnityEngine.Serialization;

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

        [Header("Control Influence")]

        [Tooltip(
            "Time duration in which wall jump movement has special control handling. " +
            "During this window, player input can blend with the wall jump impulse.")]
        [FormerlySerializedAs("movementLockDuration")]
        [Min(0f)]
        public float controlInfluenceDuration = 0.15f;

        [Tooltip(
            "How much horizontal player input affects the wall jump during the " +
            "control influence window. 0 keeps the wall jump impulse fully, " +
            "while 1 lets player input fully replace the wall jump impulse.")]
        [Range(0f, 1f)]
        public float horizontalInputInfluence = 0.35f;
    }
}
