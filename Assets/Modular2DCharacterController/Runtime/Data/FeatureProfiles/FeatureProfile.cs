using UnityEngine;

namespace Modular2DCharacterController.Runtime.Data.FeatureProfiles
{
    /// <summary>
    /// Base class for all feature profiles.
    /// 
    /// Profiles are used to store configurable data that can be shared,
    /// swapped, and prioritized at runtime by the various controller features.
    /// </summary>
    public abstract class FeatureProfile : ScriptableObject
    {
        [Header("Priority")]

        [Tooltip(
            "Determines which profile takes precedence when multiple profiles " +
            "of the same type are active. Higher values have higher priority.")]
        [Min(0)]
        public int priority;
    }
}