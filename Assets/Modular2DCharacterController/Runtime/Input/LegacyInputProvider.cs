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
        [SerializeField] private string verticalAxis = "Vertical";

        [Header("Button Names")]
        [SerializeField] private string jumpButton = "Jump";
        [SerializeField] private string runButton = "Run";
        [SerializeField] private string dashButton = "Dash";
        [SerializeField] private string crouchButton = "Crouch";
        [SerializeField] private string groundPoundButton = "GroundPound";

        public float HorizontalMoveInput => UnityEngine.Input.GetAxisRaw(horizontalAxis);
        
        public float VerticalMoveInput => UnityEngine.Input.GetAxisRaw(verticalAxis);

        public bool JumpPressed => UnityEngine.Input.GetButtonDown(jumpButton);

        public bool JumpHeld => UnityEngine.Input.GetButton(jumpButton);

        public bool RunHeld => UnityEngine.Input.GetButton(runButton);
        
        public bool DashPressed => UnityEngine.Input.GetButtonDown(dashButton);

        public bool DashHeld => UnityEngine.Input.GetButton(dashButton);
        
        public bool CrouchPressed => UnityEngine.Input.GetButtonDown(crouchButton);
        
        public bool CrouchHeld => UnityEngine.Input.GetButton(crouchButton);

        public bool GroundPoundPressed => UnityEngine.Input.GetButtonDown(groundPoundButton);

        public bool GroundPoundHeld => UnityEngine.Input.GetButton(groundPoundButton);
    }
}
