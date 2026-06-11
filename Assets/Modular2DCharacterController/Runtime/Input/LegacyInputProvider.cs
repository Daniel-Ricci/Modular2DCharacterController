using UnityEngine;

namespace Modular2DCharacterController.Runtime.Input
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
        [SerializeField] private string runButton = "Run";
        [SerializeField] private string dashButton = "Dash";

        public float MoveInput => UnityEngine.Input.GetAxisRaw(horizontalAxis);

        public bool JumpPressed => UnityEngine.Input.GetButtonDown(jumpButton);

        public bool JumpHeld => UnityEngine.Input.GetButton(jumpButton);

        public bool RunHeld => UnityEngine.Input.GetButtonDown(runButton);
        
        public bool DashPressed => UnityEngine.Input.GetButtonDown(dashButton);

        public bool DashHeld => UnityEngine.Input.GetButton(dashButton);
    }
}