using UnityEngine;

namespace Modular2DCharacterController.Runtime.Data.FeatureProfiles
{
    /// <summary>
    /// Data profile used by the glide feature.
    /// </summary>
    [CreateAssetMenu(
        fileName = "GlideProfile",
        menuName = "Modular 2D Character Controller/Glide Profile")]
    public class GlideProfile : FeatureProfile
    {
        [Header("Glide Settings")]
        
        [Tooltip(
            "Whether to use gravity scale when gliding. " +
            "If true, applies the gravityFactor to the gravity. " +
            "If false, uses the fixed velocity fallSpeed.")]
        public bool useGravityDuringGlide = false;

        [Tooltip(
            "The gravity multiplier applied while gliding if " +
            "useGravityDuringGlide is set to true.")]
        public float gravityFactor = 0.3f;

        [Tooltip(
            "The fall speed while gliding if " +
            "useGravityDuringGlide is set to false.")]
        public float fallSpeed = 3f;
    }
}
