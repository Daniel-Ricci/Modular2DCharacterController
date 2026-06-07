using UnityEngine;

namespace Modular2DCharacterController.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class Advanced2DPlatformerCamera : MonoBehaviour
    {
        [Header("Target")]
        // The object the camera follows, usually the player.
        [SerializeField] private Transform target;

        // Optional Rigidbody2D reference used to read the target's real physics velocity.
        // If left empty, the script tries to find one automatically on the target.
        [SerializeField] private Rigidbody2D targetRigidbody;

        [Header("Follow")]
        // Allows horizontal following to be enabled or disabled.
        [SerializeField] private bool followX = true;

        // Allows vertical following to be enabled or disabled.
        [SerializeField] private bool followY = true;

        // Position offset from the target.
        // A positive/negative X value keeps the camera slightly to the right/left of the target.
        // A positive/negative Y value keeps the camera slightly above/below the player.
        [SerializeField] private Vector2 offset = new(0f, 1.25f);

        // SmoothDamp timing for each axis.
        // Lower values make the camera tighter; higher values make it softer.
        [SerializeField] private Vector2 smoothTime = new(0.18f, 0.24f);

        // Prevents SmoothDamp from moving faster than this speed.
        [SerializeField] private float maxFollowSpeed = 100f;

        [Header("Dead Zone")]
        // When enabled, the target can move inside a small area before the camera follows.
        // This reduces constant camera movement during small player adjustments.
        [SerializeField] private bool useDeadZone = true;

        // Width and height of the dead zone in world units.
        [SerializeField] private Vector2 deadZoneSize = new(1.5f, 1f);

        [Header("Look Ahead")]
        // Moves the camera slightly in the direction the target is moving.
        // Useful in platformers so the player can see more of where they are going.
        [SerializeField] private bool useLookAhead = true;

        // Maximum horizontal distance the camera looks ahead.
        [SerializeField] private float lookAheadDistance = 2.5f;

        // How quickly the lookahead moves outward when the target starts moving.
        [SerializeField] private float lookAheadSmoothing = 6f;

        // How quickly the lookahead returns to center when the target slows down or stops.
        [SerializeField] private float lookAheadReturnSpeed = 3f;

        // The target must move at least this fast before lookahead activates.
        [SerializeField] private float minimumVelocityForLookAhead = 0.2f;

        [Header("Vertical Bias")]
        // Adds extra vertical framing based on whether the target is rising or falling.
        [SerializeField] private bool useVerticalVelocityBias = true;

        // Camera offset applied while the target is moving upward.
        [SerializeField] private float upwardBias = 0.75f;

        // Camera offset applied while the target is falling.
        // Negative values show more space below the player.
        [SerializeField] private float downwardBias = -1.25f;

        // How smoothly the vertical bias changes.
        [SerializeField] private float verticalBiasSmoothing = 5f;

        // The target must move vertically at least this fast before vertical bias activates.
        [SerializeField] private float minimumVerticalVelocityForBias = 1f;

        [Header("Camera Bounds")]
        // Restricts the camera to a rectangular world area.
        [SerializeField] private bool useBounds = false;

        // Bottom-left world position of the camera bounds.
        [SerializeField] private Vector2 minBounds;

        // Top-right world position of the camera bounds.
        [SerializeField] private Vector2 maxBounds;

        [Header("Zoom")]
        // Default orthographic camera size.
        [SerializeField] private float defaultOrthographicSize = 6f;

        // How smoothly zoom changes happen.
        [SerializeField] private float zoomSmoothTime = 0.2f;

        // Minimum allowed orthographic size.
        [SerializeField] private float minOrthographicSize = 3f;

        // Maximum allowed orthographic size.
        [SerializeField] private float maxOrthographicSize = 12f;

        [Header("Pixel Polish")]
        // Snaps the camera position to pixel-sized increments.
        // Usually useful for pixel art, but can look jittery with smooth movement.
        [SerializeField] private bool pixelSnap = false;

        // Pixel density used by pixel snapping.
        [SerializeField] private float pixelsPerUnit = 16f;

        [Header("Stability")]
        // Automatically enables interpolation on the target Rigidbody2D.
        // This helps smooth camera follow when the player moves through physics.
        [SerializeField] private bool autoEnableTargetInterpolation = true;

        [Header("Teleport Handling")]
        // If the target is farther than this distance from the camera,
        // the camera snaps instantly instead of slowly crossing the map.
        [SerializeField] private float snapDistance = 8f;

        [Header("Shake")]
        // How quickly screen shake fades out.
        [SerializeField] private float shakeTraumaDecay = 1.5f;

        // Maximum positional shake offset.
        [SerializeField] private float maxShakeOffset = 0.35f;

        // Maximum rotational shake amount in degrees.
        [SerializeField] private float maxShakeRotation = 2f;

        // How fast the shake noise changes.
        [SerializeField] private float shakeFrequency = 24f;

        // Cached Camera component.
        private UnityEngine.Camera _camera;

        // Current SmoothDamp velocity for X and Y follow.
        private Vector2 _followVelocity;

        // Current SmoothDamp velocity for zoom.
        private float _zoomVelocity;

        // Current smoothed horizontal lookahead value.
        private float _currentLookAhead;

        // Current smoothed vertical bias value.
        private float _currentVerticalBias;

        // Desired orthographic size.
        private float _targetZoom;

        // Current shake intensity.
        // Squared during shake calculation for a stronger falloff curve.
        private float _shakeTrauma;

        // Random offset used so Perlin noise does not always start from the same place.
        private float _shakeSeed;

        // Camera position without temporary shake applied.
        // This prevents shake from affecting normal follow calculations.
        private Vector3 _stableCameraPosition;

        // Previous target position, used to estimate velocity if no Rigidbody2D exists.
        private Vector3 _lastTargetPosition;

        // Tracks whether the stable camera position has been initialized.
        private bool _hasStableCameraPosition;

        // Tracks whether the previous target position has been initialized.
        private bool _hasLastTargetPosition;

        private void Awake()
        {
            // Cache the Camera component and force orthographic mode.
            _camera = GetComponent<UnityEngine.Camera>();
            _camera.orthographic = true;

            // Initialize zoom immediately so the camera starts at the intended size.
            _targetZoom = defaultOrthographicSize;
            _camera.orthographicSize = defaultOrthographicSize;

            // Create a random seed for screen shake noise.
            _shakeSeed = Random.value * 1000f;
        }

        private void LateUpdate()
        {
            // No target means there is nothing to follow.
            if (target == null)
                return;

            // Make sure the script has a Rigidbody2D reference if one exists.
            CacheRigidbodyIfNeeded();

            // Use the stable camera position for follow logic instead of transform.position.
            // transform.position may include shake offset from the previous frame.
            Vector3 currentPosition = _hasStableCameraPosition
                ? _stableCameraPosition
                : transform.position;

            // Calculate where the camera wants to move this frame.
            Vector3 targetPosition = CalculateTargetPosition(currentPosition);

            // Snap instantly if this is the first frame or the target moved too far away.
            if (ShouldSnapToTarget(targetPosition))
            {
                currentPosition = targetPosition;
                _followVelocity = Vector2.zero;
            }
            else
            {
                // Otherwise, smoothly move toward the target position.
                currentPosition = SmoothFollow(currentPosition, targetPosition);
            }

            // Clamp to bounds and optionally pixel-snap the position.
            currentPosition = ApplyBounds(currentPosition);
            currentPosition = ApplyPixelSnap(currentPosition);

            // Smoothly animate zoom toward the requested value.
            _camera.orthographicSize = Mathf.SmoothDamp(
                _camera.orthographicSize,
                _targetZoom,
                ref _zoomVelocity,
                zoomSmoothTime);

            // Calculate temporary shake offset and rotation.
            Vector3 shakeOffset = CalculateShakeOffset(out float shakeRotation);

            // Store the clean camera position before adding shake.
            _stableCameraPosition = currentPosition;
            _hasStableCameraPosition = true;

            // Apply final camera position and rotation.
            transform.position = currentPosition + shakeOffset;
            transform.rotation = Quaternion.Euler(0f, 0f, shakeRotation);

            // Store target position for velocity estimation.
            _lastTargetPosition = target.position;
            _hasLastTargetPosition = true;

            // Reduce shake intensity over time.
            DecayShake();
        }

        private void CacheRigidbodyIfNeeded()
        {
            // Find the Rigidbody2D automatically if one was not assigned manually.
            if (targetRigidbody == null)
                targetRigidbody = target.GetComponent<Rigidbody2D>();

            // Physics objects move in fixed steps, while the camera updates every frame.
            // Interpolation smooths the visual position between physics updates.
            if (autoEnableTargetInterpolation && targetRigidbody != null)
            {
                targetRigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
            }
        }

        private Vector3 CalculateTargetPosition(Vector3 currentCameraPosition)
        {
            // Start from the current camera position so disabled axes remain unchanged.
            Vector3 desired = currentCameraPosition;

            // Prefer Rigidbody2D velocity because it is more accurate for physics movement.
            // If there is no Rigidbody2D, estimate velocity from position changes.
            Vector2 targetVelocity = targetRigidbody != null
                ? targetRigidbody.linearVelocity
                : EstimateTargetVelocity();

            float desiredLookAhead = 0f;

            // Look ahead horizontally only when the target is moving fast enough.
            if (useLookAhead && Mathf.Abs(targetVelocity.x) >= minimumVelocityForLookAhead)
            {
                desiredLookAhead = Mathf.Sign(targetVelocity.x) * lookAheadDistance;
            }

            // Use one smoothing speed when moving into lookahead,
            // and another when returning back to center.
            float lookAheadSpeed = Mathf.Abs(desiredLookAhead) > 0.01f
                ? lookAheadSmoothing
                : lookAheadReturnSpeed;

            // Exponential lerp gives frame-rate-independent smoothing.
            _currentLookAhead = Mathf.Lerp(
                _currentLookAhead,
                desiredLookAhead,
                1f - Mathf.Exp(-lookAheadSpeed * Time.deltaTime));

            float desiredVerticalBias = 0f;

            // Bias the camera up or down based on vertical motion.
            if (useVerticalVelocityBias)
            {
                if (targetVelocity.y >= minimumVerticalVelocityForBias)
                    desiredVerticalBias = upwardBias;
                else if (targetVelocity.y <= -minimumVerticalVelocityForBias)
                    desiredVerticalBias = downwardBias;
            }

            // Smooth the vertical bias so the camera does not jump suddenly.
            _currentVerticalBias = Mathf.Lerp(
                _currentVerticalBias,
                desiredVerticalBias,
                1f - Mathf.Exp(-verticalBiasSmoothing * Time.deltaTime));

            // Final desired target center before dead zone handling.
            Vector2 targetCenter = new(
                target.position.x + offset.x + _currentLookAhead,
                target.position.y + offset.y + _currentVerticalBias);

            // Resolve horizontal movement.
            if (followX)
            {
                desired.x = useDeadZone
                    ? ResolveDeadZoneAxis(
                        currentCameraPosition.x,
                        targetCenter.x,
                        deadZoneSize.x)
                    : targetCenter.x;
            }

            // Resolve vertical movement.
            if (followY)
            {
                desired.y = useDeadZone
                    ? ResolveDeadZoneAxis(
                        currentCameraPosition.y,
                        targetCenter.y,
                        deadZoneSize.y)
                    : targetCenter.y;
            }

            // Preserve the camera's Z position.
            desired.z = currentCameraPosition.z;
            return desired;
        }

        private Vector2 EstimateTargetVelocity()
        {
            // Cannot estimate velocity until at least one previous position exists.
            if (!_hasLastTargetPosition || Time.deltaTime <= 0f)
                return Vector2.zero;

            // Position delta divided by frame time gives estimated velocity.
            return (target.position - _lastTargetPosition) / Time.deltaTime;
        }

        private float ResolveDeadZoneAxis(
            float cameraAxis,
            float targetAxis,
            float deadZoneAxisSize)
        {
            // Dead zone is centered on the camera.
            float halfSize = deadZoneAxisSize * 0.5f;
            float delta = targetAxis - cameraAxis;

            // If the target is still inside the dead zone, do not move the camera.
            if (Mathf.Abs(delta) <= halfSize)
                return cameraAxis;

            // If the target exits the dead zone, move the camera just enough
            // to place the target back on the edge of the dead zone.
            return targetAxis - Mathf.Sign(delta) * halfSize;
        }

        private Vector3 SmoothFollow(Vector3 currentPosition, Vector3 targetPosition)
        {
            float newX = currentPosition.x;
            float newY = currentPosition.y;

            // Smooth horizontal camera movement.
            if (followX)
            {
                newX = Mathf.SmoothDamp(
                    currentPosition.x,
                    targetPosition.x,
                    ref _followVelocity.x,
                    smoothTime.x,
                    maxFollowSpeed);
            }

            // Smooth vertical camera movement.
            if (followY)
            {
                newY = Mathf.SmoothDamp(
                    currentPosition.y,
                    targetPosition.y,
                    ref _followVelocity.y,
                    smoothTime.y,
                    maxFollowSpeed);
            }

            return new Vector3(newX, newY, currentPosition.z);
        }

        private bool ShouldSnapToTarget(Vector3 targetPosition)
        {
            // Snap immediately on the first valid frame.
            if (!_hasLastTargetPosition)
                return true;

            // Snap if the distance is large enough to imply teleporting,
            // respawning, scene loading, or another discontinuous movement.
            return Vector2.Distance(transform.position, targetPosition) >= snapDistance;
        }

        private Vector3 ApplyBounds(Vector3 position)
        {
            if (!useBounds)
                return position;

            // Convert orthographic size into camera extents.
            float verticalExtent = _camera.orthographicSize;
            float horizontalExtent = verticalExtent * _camera.aspect;

            // Adjust bounds so the camera edges stay inside the world rectangle.
            float minX = minBounds.x + horizontalExtent;
            float maxX = maxBounds.x - horizontalExtent;
            float minY = minBounds.y + verticalExtent;
            float maxY = maxBounds.y - verticalExtent;

            // Clamp only if the bounds are large enough for the current zoom level.
            if (minX <= maxX)
                position.x = Mathf.Clamp(position.x, minX, maxX);

            if (minY <= maxY)
                position.y = Mathf.Clamp(position.y, minY, maxY);

            return position;
        }

        private Vector3 ApplyPixelSnap(Vector3 position)
        {
            // Skip pixel snapping unless enabled and configured.
            if (!pixelSnap || pixelsPerUnit <= 0f)
                return position;

            // Convert pixel density into world-unit increments.
            float unitsPerPixel = 1f / pixelsPerUnit;

            // Snap X and Y to the nearest pixel-sized world position.
            position.x = Mathf.Round(position.x / unitsPerPixel) * unitsPerPixel;
            position.y = Mathf.Round(position.y / unitsPerPixel) * unitsPerPixel;

            return position;
        }

        private Vector3 CalculateShakeOffset(out float rotation)
        {
            rotation = 0f;

            // No trauma means no shake.
            if (_shakeTrauma <= 0f)
                return Vector3.zero;

            // Squaring trauma makes small shake fade gently while strong shake still feels powerful.
            float shake = _shakeTrauma * _shakeTrauma;
            float time = Time.time * shakeFrequency;

            // Perlin noise creates smoother shake than pure random values.
            float x = Mathf.PerlinNoise(_shakeSeed, time) * 2f - 1f;
            float y = Mathf.PerlinNoise(_shakeSeed + 10f, time) * 2f - 1f;
            float r = Mathf.PerlinNoise(_shakeSeed + 20f, time) * 2f - 1f;

            // Apply rotational shake.
            rotation = r * maxShakeRotation * shake;

            // Apply positional shake.
            return new Vector3(
                x * maxShakeOffset * shake,
                y * maxShakeOffset * shake,
                0f);
        }

        private void DecayShake()
        {
            // Nothing to decay if there is no active shake.
            if (_shakeTrauma <= 0f)
                return;

            // Reduce trauma over time until it reaches zero.
            _shakeTrauma = Mathf.Max(
                0f,
                _shakeTrauma - shakeTraumaDecay * Time.deltaTime);
        }

        public void SetTarget(Transform newTarget, bool snap = false)
        {
            // Replace the camera target.
            target = newTarget;

            // Cache the new target's Rigidbody2D if available.
            targetRigidbody = newTarget != null
                ? newTarget.GetComponent<Rigidbody2D>()
                : null;

            // Optionally move the camera immediately to the new target.
            if (snap && target != null)
            {
                Vector3 position = transform.position;
                position.x = target.position.x + offset.x;
                position.y = target.position.y + offset.y;
                transform.position = ApplyBounds(position);
                _followVelocity = Vector2.zero;
            }

            // Reset previous-position tracking so velocity estimation starts cleanly.
            _hasLastTargetPosition = false;
        }

        public void SetZoom(float orthographicSize)
        {
            // Set the desired zoom while respecting configured limits.
            _targetZoom = Mathf.Clamp(
                orthographicSize,
                minOrthographicSize,
                maxOrthographicSize);
        }

        public void ResetZoom()
        {
            // Return to the default configured zoom.
            SetZoom(defaultOrthographicSize);
        }

        public void AddShake(float amount)
        {
            // Add shake trauma.
            // Values are clamped so shake intensity stays predictable.
            _shakeTrauma = Mathf.Clamp01(_shakeTrauma + amount);
        }

        public void ClearShake()
        {
            // Immediately remove all active shake and reset rotation.
            _shakeTrauma = 0f;
            transform.rotation = Quaternion.identity;
        }
    }
}