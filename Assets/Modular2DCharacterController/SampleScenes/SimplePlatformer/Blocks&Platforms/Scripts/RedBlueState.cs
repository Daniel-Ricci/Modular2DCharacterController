using System;
using UnityEngine;

namespace Modular2DCharacterController.SampleScenes.SimplePlatformer.Blocks_Platforms.Scripts
{
    public class RedBlueState : MonoBehaviour
    {
        public event Action<bool> OnStateChanged;

        public bool isBlue = true;

        public void ChangeState()
        {
            isBlue = !isBlue;
            OnStateChanged?.Invoke(isBlue);
        }
    }
}
