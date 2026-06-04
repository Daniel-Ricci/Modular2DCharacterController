using UnityEngine;
using UnityEngine.InputSystem;

namespace Modular2DCharacterController.Input
{
    /// <summary>
    /// Character input provider using Unity's Input System package.
    /// </summary>
    public class NewInputSystemProvider : MonoBehaviour, ICharacterInput
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference rollAction;

        public float MoveInput =>
            moveAction != null
                ? moveAction.action.ReadValue<Vector2>().x
                : 0f;

        public bool JumpPressed =>
            jumpAction != null &&
            jumpAction.action.WasPressedThisFrame();

        public bool JumpHeld =>
            jumpAction != null &&
            jumpAction.action.IsPressed();

        public bool RollPressed =>
            rollAction != null &&
            rollAction.action.WasPressedThisFrame();

        private void OnEnable()
        {
            moveAction?.action.Enable();
            jumpAction?.action.Enable();
            rollAction?.action.Enable();
        }

        private void OnDisable()
        {
            moveAction?.action.Disable();
            jumpAction?.action.Disable();
            rollAction?.action.Disable();
        }
    }
}