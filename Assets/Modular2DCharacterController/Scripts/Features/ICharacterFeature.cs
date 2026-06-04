namespace Modular2DCharacterController.Features
{
    /// <summary>
    /// Defines the contract for all character features.
    /// </summary>
    public interface ICharacterFeature
    {
        void Tick();

        void FixedTick();
    }
}