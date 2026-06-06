using UnityEngine;

namespace Modular2DCharacterController.Data
{
    public abstract class FeatureProfile : ScriptableObject
    {
        [Header("Priority")]
        [Min(0)]
        public int priority;
    }
}