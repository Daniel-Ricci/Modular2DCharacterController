using UnityEngine;

namespace Modular2DCharacterController.Runtime.Data
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
        [Min(0f)]
        public float dashSpeed = 18f;

        [Min(0.01f)]
        public float dashDuration = 0.14f;

        [Min(0f)]
        public float dashCooldown = 0.08f;

        [Header("Variable Dash")]
        public bool variableDashLength = true;

        [Min(0.01f)]
        public float minimumDashDuration = 0.06f;

        [Header("Dash Count")]
        [Min(1)]
        public int maxDashCount = 1;

        public bool resetDashCountOnGround = true;

        public bool allowGroundDash = true;

        public bool allowAirDash = true;

        [Header("Direction")]
        public bool useInputDirection = true;

        public bool fallbackToFacingDirection = true;

        [Header("Dash End")]
        public bool preserveDashMomentum = true;

        [Range(0f, 1f)]
        public float endingMomentumMultiplier = 0.65f;

        public bool clearVerticalVelocityOnEnd = true;
    }
}