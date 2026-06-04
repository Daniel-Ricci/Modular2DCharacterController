using UnityEngine;

namespace Modular2DCharacterController.Input
{
    /// <summary>
    /// Character input provider using Unity's legacy Input Manager.
    /// </summary>
    public class LegacyInputProvider : MonoBehaviour, ICharacterInput
    {
        [Header("Axis Names")]
        [SerializeField] private string horizontalAxis = "Horizontal";

        [Header("Button Names")]
        [SerializeField] private string jumpButton = "Jump";
        [SerializeField] private string rollButton = "Roll";

        public float MoveInput => UnityEngine.Input.GetAxisRaw(horizontalAxis);

        public bool JumpPressed => UnityEngine.Input.GetButtonDown(jumpButton);

        public bool JumpHeld => UnityEngine.Input.GetButton(jumpButton);

        public bool RollPressed => UnityEngine.Input.GetButtonDown(rollButton);
    }
}