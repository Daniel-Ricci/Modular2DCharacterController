namespace Modular2DCharacterController.Runtime.Features
{
    /// <summary>
    /// Defines the contract for all character features.
    /// A feature represents a type of movement or ability from the player.
    /// </summary>
    public interface ICharacterFeature
    {
        void Tick();

        void FixedTick();
    }
}