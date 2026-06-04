using UnityEngine;
using Modular2DCharacterController.Features;

namespace Modular2DCharacterController.Core
{
    /// <summary>
    /// Coordinates character features.
    /// </summary>
    public class CharacterController2D : MonoBehaviour
    {
        private ICharacterFeature[] _features;

        private void Awake()
        {
            _features = GetComponents<ICharacterFeature>();
        }

        private void Update()
        {
            foreach (ICharacterFeature feature in _features)
            {
                feature.Tick();
            }
        }

        private void FixedUpdate()
        {
            foreach (ICharacterFeature feature in _features)
            {
                feature.FixedTick();
            }
        }
    }
}