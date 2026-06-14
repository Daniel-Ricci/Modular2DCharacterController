using UnityEngine;

public enum PlatformMovementAxis
{
    Horizontal,
    Vertical
}
    
/// <summary>
/// Moves a platform back and forth between two points using transform movement.
/// </summary>
public class MovingPlatform : MonoBehaviour
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

    private Vector3 _startPosition;
    private Vector3 _targetPosition;

    private void Awake()
    {
        _startPosition = transform.position;

        Vector3 direction =
            movementAxis == PlatformMovementAxis.Horizontal
                ? Vector3.right
                : Vector3.up;

        _targetPosition =
            _startPosition +
            direction * distance;
    }

    private void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            _targetPosition,
            speed * Time.deltaTime);

        if (Vector3.Distance(
                transform.position,
                _targetPosition) < 0.001f)
        {
            (_startPosition, _targetPosition) =
                (_targetPosition, _startPosition);
        }
    }
}