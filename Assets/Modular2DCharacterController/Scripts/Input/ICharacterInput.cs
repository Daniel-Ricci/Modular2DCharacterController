namespace Modular2DCharacterController.Input
{
    /// <summary>
    /// Defines the contract for character input providers.
    /// </summary>
    public interface ICharacterInput
    {
        float MoveInput { get; }

        bool JumpPressed { get; }

        bool JumpHeld { get; }

        bool RunHeld { get; }
    }
}