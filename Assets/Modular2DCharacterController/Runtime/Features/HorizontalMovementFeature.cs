using Modular2DCharacterController.Runtime.Core;
using Modular2DCharacterController.Runtime.Data.FeatureProfiles;
using Modular2DCharacterController.Runtime.Input;
using UnityEngine;

namespace Modular2DCharacterController.Runtime.Features
{
    public enum FlippingMode
    {
        None,
        TransformScale,
        SpriteRendererFlip
    }

    public enum FacingDirection
    {
        Left = -1,
        Right = 1
    }

    /// <summary>
    /// A configurable feature that allows the character to move horizontally.
    ///
    /// It uses the Horizontal Movement Profile Provider to retrieve the
    /// current movement profile and calculate movement. The default movement
    /// profile is used for basic movement when grounded and the air movement
    /// profile when airborne (higher priority).
    ///
    /// Other features may register higher priority profiles in the provider
    /// to override the values, such as run or crouch features.
    /// </summary>
    [RequireComponent(typeof(CharacterController2D))]
    public class HorizontalMovementFeature : MonoBehaviour, ICharacterFeature
    {
        [Header("Default Movement Profile")]

        [Tooltip(
            "The default horizontal movement profile registered when this feature initializes.")]
        [SerializeField]
        private HorizontalMovementProfile defaultMovementProfile;
        
        [Header("Air Movement Profile")]

        [Tooltip(
            "Optional movement profile applied while airborne. " +
            "Can be used to reduce acceleration, maximum speed, or air control.")]
        [SerializeField]
        private HorizontalMovementProfile airMovementProfile;

        [Header("Momentum")]

        [Tooltip(
            "If enabled, speeds above the current profile's maximum speed are preserved " +
            "while moving in the same direction. Useful for maintaining momentum gained " +
            "from dashes, slopes, moving platforms, or external forces.")]
        [SerializeField]
        private bool preserveMomentumAboveMaxSpeed = true;

        [Tooltip(
            "The rate at which excess speed is removed when preserving momentum above max speed. " +
            "A value of 0 disables overspeed deceleration.")]
        [SerializeField]
        [Min(0f)]
        private float overspeedDeceleration = 0f;

        [Header("Flipping")]

        [Tooltip(
            "Determines how the character's facing direction is visually represented.")]
        [SerializeField]
        private FlippingMode facingMode = FlippingMode.TransformScale;

        [Tooltip(
            "The transform used when flipping via local scale. " +
            "If left empty, the GameObject's transform is used.")]
        [SerializeField]
        private Transform graphicsRoot;

        [Tooltip(
            "The SpriteRenderer used when SpriteRendererFlip mode is selected. " +
            "If left empty, the first SpriteRenderer found in the children will be used.")]
        [SerializeField]
        private SpriteRenderer spriteRenderer;
        
        [Header("Minimum Input")]
        
        [Tooltip(
            "Minimum horizontal movement input needed to generate any movement")]
        [SerializeField]
        private float minimumInput = 0.5f;
        
        // Components used by this feature.
        private CharacterMotor _motor;
        private ICharacterInput _input;
        private GroundDetector _groundDetector;
        private CharacterController2D _controller;
        private DashFeature _dashFeature;
        private RollFeature _rollFeature;
        private WallJumpFeature _wallJumpFeature;
        private GroundPoundFeature _groundPoundFeature;
        private ProfileProvider<HorizontalMovementProfile> _horizontalMovementProfileProvider;
        
        // Indicates the direction the character is currently facing.
        public FacingDirection FacingDirection { get; private set; }
            = FacingDirection.Right;

        public HorizontalMovementProfile CurrentMovementProfile =>
            _horizontalMovementProfileProvider?.GetCurrentProfile();

        private void Awake()
        {
            _motor = GetComponent<CharacterMotor>();
            _input = GetComponent<ICharacterInput>();
            _groundDetector = GetComponent<GroundDetector>();
            _controller = GetComponent<CharacterController2D>();
            _dashFeature = GetComponent<DashFeature>();
            _rollFeature = GetComponent<RollFeature>();
            _wallJumpFeature = GetComponent<WallJumpFeature>();
            _groundPoundFeature = GetComponent<GroundPoundFeature>();
            _horizontalMovementProfileProvider = _controller.HorizontalMovementProfileProvider;
            
            if (graphicsRoot == null)
            {
                graphicsRoot = transform;
            }

            if (facingMode == FlippingMode.SpriteRendererFlip &&
                spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        private void OnEnable()
        {
            if (defaultMovementProfile != null)
            {
                _horizontalMovementProfileProvider?.RegisterProfile(defaultMovementProfile);
            }
            
            _groundDetector.LeftGround += OnLeftGround;
            _groundDetector.Landed += OnLanded;
        }

        private void OnDisable()
        {
            if (defaultMovementProfile != null)
            {
                _horizontalMovementProfileProvider?.UnregisterProfile(defaultMovementProfile);
            }

            if (airMovementProfile != null)
            {
                _horizontalMovementProfileProvider?.UnregisterProfile(airMovementProfile);
            }
            
            _groundDetector.LeftGround -= OnLeftGround;
            _groundDetector.Landed -= OnLanded;
        }

        public void Tick()
        {
        }

        public void FixedTick()
        {
            if (_dashFeature != null && _dashFeature.IsDashing)
                return;

            if (_rollFeature != null && _rollFeature.IsRolling)
                return;
            
            if (_wallJumpFeature != null && _wallJumpFeature.IsControlInfluenceActive)
                return;

            if (_groundPoundFeature != null &&
                (_groundPoundFeature.IsGroundPounding ||
                 _groundPoundFeature.IsRecoveryActive))
            {
                return;
            }
            
            HorizontalMovementProfile currentProfile =
                _horizontalMovementProfileProvider.GetCurrentProfile();

            if (currentProfile == null)
                return;

            float rawMoveInput =
                _input.HorizontalMoveInput;

            float moveInput =
                Mathf.Abs(rawMoveInput) >= minimumInput
                    ? rawMoveInput
                    : 0f;

            bool isTryingToMove =
                moveInput != 0f;

            float targetSpeed =
                moveInput * currentProfile.maxSpeed;

            float currentSpeed =
                _motor.CurrentSelfVelocity.x;

            bool isTurning =
                isTryingToMove &&
                Mathf.Abs(currentSpeed) > 0.01f &&
                !Mathf.Approximately(Mathf.Sign(currentSpeed), Mathf.Sign(moveInput));

            bool isMovingSameDirection =
                isTryingToMove &&
                Mathf.Abs(currentSpeed) > 0.01f &&
                Mathf.Approximately(Mathf.Sign(currentSpeed), Mathf.Sign(moveInput));

            bool isAboveProfileMaxSpeed =
                Mathf.Abs(currentSpeed) > Mathf.Abs(targetSpeed);

            if (preserveMomentumAboveMaxSpeed &&
                isMovingSameDirection &&
                isAboveProfileMaxSpeed)
            {
                float preservedSpeed = currentSpeed;

                if (overspeedDeceleration > 0f)
                {
                    preservedSpeed = Mathf.MoveTowards(
                        currentSpeed,
                        targetSpeed,
                        overspeedDeceleration * Time.fixedDeltaTime);
                }

                _motor.SetHorizontalSelfVelocity(preservedSpeed);
                UpdateFacingDirection(moveInput);
                return;
            }

            float accelerationRate;

            if (!isTryingToMove)
            {
                accelerationRate =
                    currentProfile.deceleration;
            }
            else if (isTurning)
            {
                accelerationRate =
                    currentProfile.turnAcceleration;
            }
            else
            {
                accelerationRate =
                    currentProfile.acceleration;
            }

            float newSpeed =
                Mathf.MoveTowards(
                    currentSpeed,
                    targetSpeed,
                    accelerationRate * Time.fixedDeltaTime);

            _motor.SetHorizontalSelfVelocity(newSpeed);

            UpdateFacingDirection(moveInput);
        }

        // Updates direction the character is facing.
        private void UpdateFacingDirection(float moveInput)
        {
            if (moveInput == 0f)
                return;

            FacingDirection =
                moveInput > 0f
                    ? FacingDirection.Right
                    : FacingDirection.Left;

            switch (facingMode)
            {
                case FlippingMode.None:
                    break;

                case FlippingMode.TransformScale:
                    FlipByScale();
                    break;

                case FlippingMode.SpriteRendererFlip:
                    FlipBySpriteRenderer();
                    break;
            }
        }

        // Flips the character by updating its scale.
        private void FlipByScale()
        {
            if (graphicsRoot == null)
                return;

            Vector3 scale =
                graphicsRoot.localScale;

            scale.x =
                Mathf.Abs(scale.x) *
                (int)FacingDirection;

            graphicsRoot.localScale =
                scale;
        }

        // Flips the character by updating sprite renderer's facing direction.
        private void FlipBySpriteRenderer()
        {
            if (spriteRenderer == null)
                return;

            spriteRenderer.flipX =
                FacingDirection == FacingDirection.Left;
        }

        // Called when the character goes airborne to register the air movement
        // profile, if available.
        private void OnLeftGround(Vector2 _)
        {
            if (airMovementProfile != null)
            {
                _horizontalMovementProfileProvider?.RegisterProfile(airMovementProfile);
            }
        }
        
        // Called when the character lands to unregister the air movement profile.
        private void OnLanded(CharacterHitEvent _)
        {
            if (airMovementProfile != null)
            {
                _horizontalMovementProfileProvider?.UnregisterProfile(airMovementProfile);
            }
        }
    }
}
