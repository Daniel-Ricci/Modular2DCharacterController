#if ENABLE_INPUT_SYSTEM

using UnityEngine;
using UnityEngine.InputSystem;

namespace Modular2DCharacterController.Runtime.Input.NewInputSystem
{
    /// <summary>
    /// Character input provider using Unity's Input System package.
    /// </summary>
    public class NewInputSystemProvider : MonoBehaviour, ICharacterInput
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference runAction;
        [SerializeField] private InputActionReference glideAction;
        [SerializeField] private InputActionReference dashAction;
        [SerializeField] private InputActionReference rollAction;
        [SerializeField] private InputActionReference crouchAction;
        [SerializeField] private InputActionReference groundPoundAction;

        public float HorizontalMoveInput =>
            moveAction != null
                ? moveAction.action.ReadValue<Vector2>().x
                : 0f;
        
        public float VerticalMoveInput =>
            moveAction != null
                ? moveAction.action.ReadValue<Vector2>().y
                : 0f;

        public bool JumpPressed =>
            jumpAction != null &&
            jumpAction.action.WasPressedThisFrame();

        public bool JumpHeld =>
            jumpAction != null &&
            jumpAction.action.IsPressed();

        public bool RunHeld =>
            runAction != null &&
            runAction.action.IsPressed();
        
        public bool GlideHeld =>
            glideAction != null &&
            glideAction.action.IsPressed();
        
        public bool DashPressed =>
            dashAction != null &&
            dashAction.action.WasPressedThisFrame();

        public bool DashHeld =>
            dashAction != null &&
            dashAction.action.IsPressed();

        public bool RollPressed =>
            rollAction != null &&
            rollAction.action.WasPressedThisFrame();

        public bool RollHeld =>
            rollAction != null &&
            rollAction.action.IsPressed();

        public bool CrouchPressed =>
            crouchAction != null &&
            crouchAction.action.WasPressedThisFrame();

        public bool CrouchHeld =>
            crouchAction != null &&
            crouchAction.action.IsPressed();

        public bool GroundPoundPressed =>
            groundPoundAction != null &&
            groundPoundAction.action.WasPressedThisFrame();

        public bool GroundPoundHeld =>
            groundPoundAction != null &&
            groundPoundAction.action.IsPressed();

        private void OnEnable()
        {
            moveAction?.action.Enable();
            jumpAction?.action.Enable();
            runAction?.action.Enable();
            dashAction?.action.Enable();
            rollAction?.action.Enable();
            crouchAction?.action.Enable();
            groundPoundAction?.action.Enable();
        }

        private void OnDisable()
        {
            moveAction?.action.Disable();
            jumpAction?.action.Disable();
            runAction?.action.Disable();
            dashAction?.action.Disable();
            rollAction?.action.Disable();
            crouchAction?.action.Disable();
            groundPoundAction?.action.Disable();
        }
    }
}

#endif
