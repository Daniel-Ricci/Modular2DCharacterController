using UnityEngine;

namespace Modular2DCharacterController.Scripts.Data
{
    /// <summary>
    /// Base class for each profile that provides the data used by each feature.
    /// </summary>
    public abstract class FeatureProfile : ScriptableObject
    {
        [Header("Priority")]
        [Min(0)]
        public int priority;
    }
}