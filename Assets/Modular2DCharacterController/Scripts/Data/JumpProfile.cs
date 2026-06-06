using UnityEngine;

namespace Modular2DCharacterController.Data
{
    [CreateAssetMenu(
        fileName = "JumpProfile",
        menuName = "Modular 2D Character Controller/Jump Profile")]
    public class JumpProfile : FeatureProfile
    {
        [Header("Jump Settings")]
        [Min(0.1f)]
        public float jumpHeight;

        [Min(0.05f)]
        public float timeToApex;

        [Min(1f)]
        public float fallGravityMultiplier;
    }
}