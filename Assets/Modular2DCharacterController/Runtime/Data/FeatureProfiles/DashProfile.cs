using UnityEngine;

namespace Modular2DCharacterController.Runtime.Data.FeatureProfiles
{
    /// <summary>
    /// Data profile used by the dash feature.
    /// </summary>
    [CreateAssetMenu(
        fileName = "DashProfile",
        menuName = "Modular 2D Character Controller/Dash Profile")]
    public class DashProfile : FeatureProfile
    {
        [Header("Dash")]

        [Tooltip(
            "The speed applied while dashing.")]
        [Min(0f)]
        public float dashSpeed = 18f;

        [Tooltip(
            "The maximum duration of the dash in seconds.")]
        [Min(0.01f)]
        public float dashDuration = 0.14f;

        [Tooltip(
            "The amount of time that must pass before another dash can be started.")]
        [Min(0f)]
        public float dashCooldown = 0.08f;

        [Header("Variable Dash")]

        [Tooltip(
            "If enabled, releasing the dash input early can end the dash before its full duration.")]
        public bool variableDashLength = true;

        [Tooltip(
            "The minimum amount of time a dash will last before it can be interrupted " +
            "when Variable Dash Length is enabled.")]
        [Min(0.01f)]
        public float minimumDashDuration = 0.06f;

        [Header("Dash Count")]

        [Tooltip(
            "The maximum number of consecutive dashes that can be performed " +
            "before the dash count must be reset.")]
        [Min(1)]
        public int maxDashCount = 1;

        [Tooltip(
            "If enabled, the available dash count is restored when the character becomes grounded.")]
        public bool resetDashCountOnGround = true;

        [Tooltip(
            "If enabled, dashing is allowed while grounded.")]
        public bool allowGroundDash = true;

        [Tooltip(
            "If enabled, dashing is allowed while airborne.")]
        public bool allowAirDash = true;

        [Header("Direction")]

        [Tooltip(
            "If enabled, the dash direction is determined from the current movement input.")]
        public bool useInputDirection = true;

        [Tooltip(
            "If enabled and no valid input direction is available, the dash will use " +
            "the character's facing direction instead.")]
        public bool fallbackToFacingDirection = true;

        [Header("Dash End")]

        [Tooltip(
            "If enabled, some of the dash velocity is preserved when the dash ends.")]
        public bool preserveDashMomentum = true;

        [Tooltip(
            "The percentage of horizontal dash velocity retained when Preserve Dash Momentum is enabled.")]
        [Range(0f, 1f)]
        public float endingMomentumMultiplier = 0.65f;

        [Tooltip(
            "If enabled, the character's vertical velocity is cleared when the dash ends.")]
        public bool clearVerticalVelocityOnEnd = true;
    }
}