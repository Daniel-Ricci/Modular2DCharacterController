using UnityEngine;

namespace Modular2DCharacterController.Runtime.Data
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
        [Min(0.1f)]
        public float jumpHeight = 3.0f;

        [Min(0.05f)]
        public float timeToApex = 0.25f;

        [Min(1f)]
        public float fallGravityMultiplier = 2.5f;
    }
}