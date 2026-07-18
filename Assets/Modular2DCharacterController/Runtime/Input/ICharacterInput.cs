namespace Modular2DCharacterController.Runtime.Input
{
    /// <summary>
    /// Defines the contract for character input providers.
    /// </summary>
    public interface ICharacterInput
    {
        float HorizontalMoveInput { get; }
        
        float VerticalMoveInput { get; }

        bool JumpPressed { get; }

        bool JumpHeld { get; }

        bool RunHeld { get; }
        
        bool DashPressed { get; }
        
        bool DashHeld { get; }

        bool RollPressed { get; }

        bool RollHeld { get; }
        
        bool CrouchPressed { get; }
        
        bool CrouchHeld { get; }

        bool GroundPoundPressed { get; }

        bool GroundPoundHeld { get; }
    }
}
