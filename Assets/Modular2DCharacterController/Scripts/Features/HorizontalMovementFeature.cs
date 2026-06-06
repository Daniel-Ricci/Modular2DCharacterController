using UnityEngine;
using Modular2DCharacterController.Core;
using Modular2DCharacterController.Data;
using Modular2DCharacterController.Input;

namespace Modular2DCharacterController.Features
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
    /// Handles horizontal character movement.
    /// </summary>
    [RequireComponent(typeof(CharacterController2D))]
    public class HorizontalMovementFeature : MonoBehaviour, ICharacterFeature
    {
        [Header("Default Movement Profile")]
        [SerializeField]
        private HorizontalMovementProfile defaultMovementProfile;

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
        private ProfileProvider<HorizontalMovementProfile> _horizontalMovementProfileProvider;

        private void Awake()
        {
            _motor = GetComponent<CharacterMotor>();
            _input = GetComponent<ICharacterInput>();
            _controller = GetComponent<CharacterController2D>();
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
            HorizontalMovementProfile currentProfile = _horizontalMovementProfileProvider.GetCurrentProfile();
            
            if (currentProfile == null)
                return;
            
            float targetSpeed =
                _input.MoveInput * currentProfile.maxSpeed;

            float currentSpeed =
                _motor.HorizontalVelocity;

            bool isTryingToMove =
                Mathf.Abs(_input.MoveInput) > 0.01f;

            bool isTurning =
                isTryingToMove &&
                Mathf.Abs(currentSpeed) > 0.01f &&
                !Mathf.Approximately(Mathf.Sign(currentSpeed), Mathf.Sign(_input.MoveInput));

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