using UnityEngine;

namespace Modular2DCharacterController.Runtime.Data.FeatureProfiles
{
    /// <summary>
    /// Data profile used by the ground pound feature.
    /// </summary>
    [CreateAssetMenu(
        fileName = "GroundPoundProfile",
        menuName = "Modular 2D Character Controller/Ground Pound Profile")]
    public class GroundPoundProfile : FeatureProfile
    {
        [Header("Startup")]

        [Tooltip(
            "How long the character stays still in the air before descending.")]
        [Min(0f)]
        public float stillTimeBeforeDescending = 0.08f;

        [Header("Descent")]

        [Tooltip(
            "Downward speed applied while ground pounding.")]
        [Min(0f)]
        public float descendSpeed = 28f;

        [Tooltip(
            "Maximum time the descent can last. Use 0 or below to continue " +
            "until the character hits the ground or is interrupted.")]
        public float descendTime = 0f;

        [Header("Landing Recovery")]

        [Tooltip(
            "How long horizontal movement remains disabled after hitting the ground.")]
        [Min(0f)]
        public float timeBeforeCanMoveAgainIfHitGround = 0.12f;
    }
}
