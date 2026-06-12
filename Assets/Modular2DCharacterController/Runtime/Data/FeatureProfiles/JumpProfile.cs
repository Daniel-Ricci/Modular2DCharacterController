using UnityEngine;

namespace Modular2DCharacterController.Runtime.Data.FeatureProfiles
{
    /// <summary>
    /// Data profile used by the jump feature.
    /// </summary>
    [CreateAssetMenu(
        fileName = "JumpProfile",
        menuName = "Modular 2D Character Controller/Jump Profile")]
    public class JumpProfile : FeatureProfile
    {
        [Header("Jump Settings")]

        [Tooltip(
            "The maximum height reached by the jump when using the full jump duration.")]
        [Min(0.1f)]
        public float jumpHeight = 3.0f;

        [Tooltip(
            "The time it takes to reach the highest point of the jump. " +
            "Lower values result in a faster, snappier jump.")]
        [Min(0.05f)]
        public float timeToApex = 0.25f;

        [Tooltip(
            "Gravity multiplier applied while falling. " +
            "Higher values make the character fall faster and create a snappier jump arc.")]
        [Min(1f)]
        public float fallGravityMultiplier = 2.5f;
    }
}