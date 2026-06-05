using UnityEngine;

namespace Modular2DCharacterController.Data
{
    [CreateAssetMenu(
        fileName = "JumpSettings",
        menuName = "Modular 2D Character Controller/Jump Settings")]
    public class JumpSettings : ScriptableObject
    {
        [Min(0.1f)]
        public float JumpHeight = 4f;

        [Min(0.05f)]
        public float TimeToApex = 0.4f;

        [Min(1f)]
        public float FallGravityMultiplier = 2f;
    }
}