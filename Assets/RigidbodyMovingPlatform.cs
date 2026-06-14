using UnityEngine;
    
/// <summary>
/// Moves a platform back and forth between two points using Rigidbody2D.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class RigidbodyMovingPlatform : MonoBehaviour
{
    [Header("Movement")]

    [Tooltip(
        "The axis along which the platform moves.")]
    [SerializeField]
    private PlatformMovementAxis movementAxis =
        PlatformMovementAxis.Horizontal;

    [Tooltip(
        "Distance from the starting position to the destination.")]
    [SerializeField]
    [Min(0f)]
    private float distance = 5f;

    [Tooltip(
        "Movement speed in units per second.")]
    [SerializeField]
    [Min(0f)]
    private float speed = 2f;

    private Rigidbody2D _rigidbody;

    private Vector2 _startPosition;
    private Vector2 _targetPosition;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();

        _startPosition = _rigidbody.position;

        Vector2 direction =
            movementAxis == PlatformMovementAxis.Horizontal
                ? Vector2.right
                : Vector2.up;

        _targetPosition =
            _startPosition +
            direction * distance;
    }

    private void FixedUpdate()
    {
        Vector2 newPosition =
            Vector2.MoveTowards(
                _rigidbody.position,
                _targetPosition,
                speed * Time.fixedDeltaTime);

        _rigidbody.MovePosition(newPosition);

        if (Vector2.Distance(
                _rigidbody.position,
                _targetPosition) < 0.001f)
        {
            (_startPosition, _targetPosition) =
                (_targetPosition, _startPosition);
        }
    }
}