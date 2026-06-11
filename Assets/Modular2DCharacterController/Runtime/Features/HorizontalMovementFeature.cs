using Modular2DCharacterController.Runtime.Core;
using Modular2DCharacterController.Runtime.Data;
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
    /// A configurable feature that allows the player to move horizontally.
    /// </summary>
    [RequireComponent(typeof(CharacterController2D))]
    public class HorizontalMovementFeature : MonoBehaviour, ICharacterFeature
    {
        [Header("Default Movement Profile")]
        // Default horizontal movement profile registered when this feature wakes up.
        [SerializeField]
        private HorizontalMovementProfile defaultMovementProfile;

        [Header("Momentum")]
        [SerializeField]
        // True if the momentum should be preserved if the player is able to achieve a speed above the maximum.
        private bool preserveMomentumAboveMaxSpeed = true;

        [SerializeField]
        [Min(0f)]
        // Deceleration rate when above maximum speed.
        private float overspeedDeceleration = 0f;

        [Header("Flipping")]
        [SerializeField]
        private FlippingMode facingMode = FlippingMode.TransformScale;

        [SerializeField]
        private Transform graphicsRoot;

        [SerializeField]
        private SpriteRenderer spriteRenderer;

        public FacingDirection FacingDirection { get; private set; }
            = FacingDirection.Right;

        private CharacterMotor _motor;
        private ICharacterInput _input;
        private CharacterController2D _controller;
        private DashFeature _dashFeature;
        private ProfileProvider<HorizontalMovementProfile> _horizontalMovementProfileProvider;

        private void Awake()
        {
            _motor = GetComponent<CharacterMotor>();
            _input = GetComponent<ICharacterInput>();
            _controller = GetComponent<CharacterController2D>();
            _dashFeature = GetComponent<DashFeature>();
            _horizontalMovementProfileProvider = _controller.HorizontalMovementProfileProvider;

            if (defaultMovementProfile != null)
            {
                _horizontalMovementProfileProvider?.RegisterProfile(defaultMovementProfile);
            }

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

        public void Tick()
        {
        }

        public void FixedTick()
        {
            if (_dashFeature != null && _dashFeature.IsDashing)
                return;
            
            HorizontalMovementProfile currentProfile =
                _horizontalMovementProfileProvider.GetCurrentProfile();

            if (currentProfile == null)
                return;

            float moveInput = _input.MoveInput;

            float targetSpeed =
                moveInput * currentProfile.maxSpeed;

            float currentSpeed =
                _motor.HorizontalVelocity;

            bool isTryingToMove =
                Mathf.Abs(moveInput) > 0.01f;

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

            if (
                preserveMomentumAboveMaxSpeed &&
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

                _motor.SetHorizontalVelocity(preservedSpeed);
                UpdateFacingDirection();
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

            _motor.SetHorizontalVelocity(newSpeed);

            UpdateFacingDirection();
        }

        private void UpdateFacingDirection()
        {
            if (Mathf.Abs(_input.MoveInput) < 0.01f)
                return;

            FacingDirection =
                _input.MoveInput > 0f
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

        private void FlipBySpriteRenderer()
        {
            if (spriteRenderer == null)
                return;

            spriteRenderer.flipX =
                FacingDirection == FacingDirection.Left;
        }
    }
}