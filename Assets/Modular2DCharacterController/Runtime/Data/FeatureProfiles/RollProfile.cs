using UnityEngine;

namespace Modular2DCharacterController.Runtime.Data.FeatureProfiles
{
    /// <summary>
    /// Data profile used by the roll feature.
    /// </summary>
    [CreateAssetMenu(
        fileName = "RollProfile",
        menuName = "Modular 2D Character Controller/Roll Profile")]
    public class RollProfile : FeatureProfile
    {
        [Header("Roll Settings")]

        [Tooltip(
            "The speed applied while rolling.")]
        [Min(0f)]
        public float rollSpeed = 14f;

        [Tooltip(
            "The maximum duration of the roll in seconds.")]
        [Min(0.01f)]
        public float rollDuration = 0.22f;

        [Tooltip(
            "The amount of time that must pass before another roll can be started.")]
        [Min(0f)]
        public float rollCooldown = 0.12f;

        [Header("Variable Roll")]

        [Tooltip(
            "If enabled, releasing the roll input early can end the roll before its full duration.")]
        public bool variableRollLength = true;

        [Tooltip(
            "The minimum amount of time a roll will last before it can be interrupted " +
            "when Variable Roll Length is enabled.")]
        [Min(0.01f)]
        public float minimumRollDuration = 0.08f;

        [Header("Roll End")]

        [Tooltip(
            "If enabled, some of the roll velocity is preserved when the roll ends.")]
        public bool preserveRollMomentum = true;

        [Tooltip(
            "The percentage of horizontal roll velocity retained when Preserve Roll Momentum is enabled.")]
        [Range(0f, 1f)]
        public float endingMomentumMultiplier = 0.45f;

        [Tooltip(
            "If enabled, the character's vertical velocity is cleared when the roll ends.")]
        public bool clearVerticalVelocityOnEnd = false;

        [Header("Gravity")]

        [Tooltip(
            "If enabled, gravity is applied while rolling.")]
        public bool applyGravity = true;
    }
}
