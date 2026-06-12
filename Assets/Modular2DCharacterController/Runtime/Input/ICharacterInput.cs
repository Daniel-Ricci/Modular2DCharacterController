namespace Modular2DCharacterController.Runtime.Input
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
        
        bool DashPressed { get; }
        
        bool DashHeld { get; }
        
        bool CrouchPressed { get; }
        
        bool CrouchHeld { get; }
    }
}