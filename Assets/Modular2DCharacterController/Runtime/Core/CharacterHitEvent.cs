using UnityEngine;

namespace Modular2DCharacterController.Runtime.Core
{
    /// <summary>
    /// Shared payload for character hit events.
    /// </summary>
    public readonly struct CharacterHitEvent
    {
        public CharacterHitEvent(
            GameObject hitObject,
            Vector2 point,
            Vector2 normal,
            Collider2D hitCollider = null,
            Rigidbody2D hitRigidbody = null,
            GameObject source = null,
            Vector2 velocity = default)
        {
            HitObject = hitObject;
            Point = point;
            Normal = normal;
            HitCollider = hitCollider;
            HitRigidbody = hitRigidbody;
            Source = source;
            Velocity = velocity;
        }

        public GameObject HitObject { get; }

        public Vector2 Point { get; }

        public Vector2 Normal { get; }

        public Collider2D HitCollider { get; }

        public Rigidbody2D HitRigidbody { get; }

        public GameObject Source { get; }

        public Vector2 Velocity { get; }
    }
}
