using System;
using Modular2DCharacterController.Runtime.Core;
using Modular2DCharacterController.Runtime.Input;
using UnityEngine;

namespace Modular2DCharacterController.Runtime.Features
{
    /// <summary>
    /// A configurable feature that detects and manages wall sliding.
    ///
    /// Character is considered wall sliding if against a wall and holding towards it.
    /// </summary>
    [RequireComponent(typeof(WallDetector))]
    [RequireComponent(typeof(CharacterController2D))]
    public class WallSlideFeature : MonoBehaviour, ICharacterFeature
    {
        [Header("Wall Slide Settings")]
        
        [Tooltip(
            "Whether to use gravity scale when wall sliding. " +
            "If true, applies the wallSlideGravityFactor to the gravity. " +
            "If false, uses the fixed velocity wallSlideVelocity.")]
        [SerializeField]
        private bool useGravityDuringWallSlide = false;

        [Tooltip(
            "The gravity multiplier applied while wall sliding if " +
            "useGravityDuringWallSlide is set to true.")]
        [SerializeField]
        [Min(0f)]
        private float wallSlideGravityFactor = 0.3f;
        
        [Tooltip(
            "The velocity while wall sliding if " +
            "useGravityDuringWallSlide is set to false.")]
        [SerializeField]
        [Min(0f)]
        private float wallSlideVelocity = 3.0f;

        // Components used by this feature.
        private CharacterMotor _motor;
        private GroundDetector _groundDetector;
        private WallDetector _wallDetector;
        private ICharacterInput _input;
        
        private const float VerticalWallThreshold = 0.9f;
        
        // True while wall sliding.
        public bool IsWallSliding { get; private set; }

        public event Action WallSlideStarted;

        public event Action WallSlideEnded;

        private void Awake()
        {
            _motor = GetComponent<CharacterMotor>();
            _groundDetector = GetComponent<GroundDetector>();
            _wallDetector = GetComponent<WallDetector>();
            _input = GetComponent<ICharacterInput>();
        }

        public void Tick()
        {
        }

        public void FixedTick()
        {
            bool wasWallSliding =
                IsWallSliding;

            if (_groundDetector.IsGrounded)
            {
                IsWallSliding = false;
                RaiseWallSlideEvents(wasWallSliding);
                return;
            }

            if (!_wallDetector.IsTouchingWall)
            {
                IsWallSliding = false;
                RaiseWallSlideEvents(wasWallSliding);
                return;
            }
            
            if (Mathf.Abs(_wallDetector.WallNormal.x) < VerticalWallThreshold)
            {
                IsWallSliding = false;
                RaiseWallSlideEvents(wasWallSliding);
                return;
            }

            float horizontalInput =
                _input.HorizontalMoveInput;

            float wallDirection =
                -_wallDetector.WallNormal.x;

            bool holdingTowardsWall =
                Mathf.Approximately(Mathf.Sign(horizontalInput), Mathf.Sign(wallDirection)) &&
                Mathf.Abs(horizontalInput) > 0.01f;

            IsWallSliding = holdingTowardsWall;

            RaiseWallSlideEvents(wasWallSliding);
            
            bool goingUp =
                _motor.CurrentSelfVelocity.y > 0.01f;

            if (IsWallSliding && !goingUp)
            {
                if (useGravityDuringWallSlide)
                {
                    _motor.AddGravityMultiplier(wallSlideGravityFactor);
                }
                else
                {
                    _motor.SetVerticalSelfVelocity(-wallSlideVelocity);
                    _motor.SuppressGravityThisFrame();
                }
            }
        }

        private void RaiseWallSlideEvents(bool wasWallSliding)
        {
            if (!wasWallSliding && IsWallSliding)
            {
                WallSlideStarted?.Invoke();
            }
            else if (wasWallSliding && !IsWallSliding)
            {
                WallSlideEnded?.Invoke();
            }
        }
    }
}
