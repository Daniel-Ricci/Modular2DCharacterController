namespace Modular2DCharacterController.Runtime.Core
{
    public interface ILandedHitReceiver
    {
        void OnLandedHit(CharacterHitEvent hitEvent);
    }

    public interface ICeilingHitReceiver
    {
        void OnCeilingHit(CharacterHitEvent hitEvent);
    }

    public interface IGroundPoundHitReceiver
    {
        void OnGroundPoundHit(CharacterHitEvent hitEvent);
    }

    public interface IDashHitReceiver
    {
        void OnDashHit(CharacterHitEvent hitEvent);
    }
}
