using Modular2DCharacterController.Runtime.Features;
using System;
using UnityEngine;

namespace Modular2DCharacterController.Runtime.Core
{
    /// <summary>
    /// Listens to the low-level feature events and expose a single integration point for external systems.
    /// </summary>
    public class CharacterEventDispatcher : MonoBehaviour
    {
        public event Action<Vector2> Landed;
        public event Action<Vector2> LeftGround;
        public event Action<GameObject> CeilingHit;
        public event Action<float> Jumped;
        public event Action StartedRun;
        public event Action StoppedRun;
        public event Action<float> Dashed;
        public event Action DashEnded;
        public event Action CrouchStarted;
        public event Action CrouchEnded;
        public event Action CrouchStandBlocked;
        public event Action<bool> CrouchColliderChanged;
        public event Action GlideStarted;
        public event Action GlideEnded;
        public event Action GroundPoundStarted;
        public event Action GroundPoundInterrupted;
        public event Action<GameObject> GroundPoundFinished;
        public event Action WallSlideStarted;
        public event Action WallSlideEnded;
        public event Action WallJumped;

        private GroundDetector groundDetector;
        private CeilingDetector ceilingDetector;
        private JumpFeature jumpFeature;
        private RunFeature runFeature;
        private DashFeature dashFeature;
        private CrouchFeature crouchFeature;
        private GlideFeature glideFeature;
        private GroundPoundFeature groundPoundFeature;
        private WallSlideFeature wallSlideFeature;
        private WallJumpFeature wallJumpFeature;
        
        private void Awake()
        {
            groundDetector = GetComponent<GroundDetector>();
            ceilingDetector = GetComponent<CeilingDetector>();
            jumpFeature = GetComponent<JumpFeature>();
            runFeature = GetComponent<RunFeature>();
            dashFeature = GetComponent<DashFeature>();
            crouchFeature = GetComponent<CrouchFeature>();
            glideFeature = GetComponent<GlideFeature>();
            groundPoundFeature = GetComponent<GroundPoundFeature>();
            wallSlideFeature = GetComponent<WallSlideFeature>();
            wallJumpFeature = GetComponent<WallJumpFeature>();
        }

        private void OnEnable()
        {
            if (groundDetector != null)
            {
                groundDetector.Landed += OnLanded;
                groundDetector.LeftGround += OnLeftGround;
            }

            if (ceilingDetector != null)
            {
                ceilingDetector.CeilingHit += OnCeilingHit;
            }
            
            if (jumpFeature != null)
                jumpFeature.Jumped += OnJumped;

            if (runFeature != null)
            {
                runFeature.StartedRun += OnStartedRun;
                runFeature.StoppedRun += OnStoppedRun;
            }

            if (dashFeature != null)
            {
                dashFeature.Dashed += OnDashed;
                dashFeature.DashEnded += OnDashEnded;
            }
                

            if (crouchFeature != null)
            {
                crouchFeature.CrouchStarted += OnCrouchStarted;
                crouchFeature.CrouchEnded += OnCrouchEnded;
                crouchFeature.CrouchStandBlocked += OnCrouchStandBlocked;
                crouchFeature.CrouchColliderChanged += OnCrouchColliderChanged;
            }

            if (glideFeature != null)
            {
                glideFeature.GlideStarted += OnGlideStarted;
                glideFeature.GlideEnded += OnGlideEnded;
            }

            if (groundPoundFeature != null)
            {
                groundPoundFeature.GroundPoundStarted += OnGroundPoundStarted;
                groundPoundFeature.GroundPoundInterrupted += OnGroundPoundInterrupted;
                groundPoundFeature.GroundPoundFinished += OnGroundPoundFinished;
            }

            if (wallSlideFeature != null)
            {
                wallSlideFeature.WallSlideStarted += OnWallSlideStarted;
                wallSlideFeature.WallSlideEnded += OnWallSlideEnded;
            }

            if (wallJumpFeature != null)
            {
                wallJumpFeature.WallJumped += OnWallJumped;
            }
        }

        private void OnDisable()
        {
            if (groundDetector != null)
            {
                groundDetector.Landed -= OnLanded;
                groundDetector.LeftGround -= OnLeftGround;
            }

            if (ceilingDetector != null)
            {
                ceilingDetector.CeilingHit -= OnCeilingHit;
            }
            
            if (jumpFeature != null)
                jumpFeature.Jumped -= OnJumped;
            
            if (runFeature != null)
            {
                runFeature.StartedRun -= OnStartedRun;
                runFeature.StoppedRun -= OnStoppedRun;
            }

            if (dashFeature != null)
            {
                dashFeature.Dashed -= OnDashed;
                dashFeature.DashEnded -= OnDashEnded;
            }
            
            if (crouchFeature != null)
            {
                crouchFeature.CrouchStarted -= OnCrouchStarted;
                crouchFeature.CrouchEnded -= OnCrouchEnded;
                crouchFeature.CrouchStandBlocked -= OnCrouchStandBlocked;
                crouchFeature.CrouchColliderChanged -= OnCrouchColliderChanged;
            }

            if (glideFeature != null)
            {
                glideFeature.GlideStarted -= OnGlideStarted;
                glideFeature.GlideEnded -= OnGlideEnded;
            }

            if (groundPoundFeature != null)
            {
                groundPoundFeature.GroundPoundStarted -= OnGroundPoundStarted;
                groundPoundFeature.GroundPoundInterrupted -= OnGroundPoundInterrupted;
                groundPoundFeature.GroundPoundFinished -= OnGroundPoundFinished;
            }

            if (wallSlideFeature != null)
            {
                wallSlideFeature.WallSlideStarted -= OnWallSlideStarted;
                wallSlideFeature.WallSlideEnded -= OnWallSlideEnded;
            }
            
            if (wallJumpFeature != null)
            {
                wallJumpFeature.WallJumped -= OnWallJumped;
            }
        }
        
        private void OnLanded(Vector2 landingVelocity)
        {
            Landed?.Invoke(landingVelocity);
        }

        private void OnLeftGround(Vector2 launchVelocity)
        {
            LeftGround?.Invoke(launchVelocity);
        }

        private void OnCeilingHit(GameObject hitObject)
        {
            CeilingHit?.Invoke(hitObject);
        }

        private void OnJumped(float jumpVelocity)
        {
            Jumped?.Invoke(jumpVelocity);
        }

        private void OnStartedRun()
        {
            StartedRun?.Invoke();
        }

        private void OnStoppedRun()
        {
            StoppedRun?.Invoke();
        }

        private void OnDashed(float dashVelocity)
        {
            Dashed?.Invoke(dashVelocity);
        }
        
        private void OnDashEnded()
        {
            DashEnded?.Invoke();
        }

        private void OnCrouchStarted()
        {
            CrouchStarted?.Invoke();
        }

        private void OnCrouchEnded()
        {
            CrouchEnded?.Invoke();
        }

        private void OnCrouchStandBlocked()
        {
            CrouchStandBlocked?.Invoke();
        }

        private void OnCrouchColliderChanged(bool isCrouching)
        {
            CrouchColliderChanged?.Invoke(isCrouching);
        }

        private void OnGlideStarted()
        {
            GlideStarted?.Invoke();
        }

        private void OnGlideEnded()
        {
            GlideEnded?.Invoke();
        }

        private void OnGroundPoundStarted()
        {
            GroundPoundStarted?.Invoke();
        }
        
        private void OnGroundPoundInterrupted()
        {
            GroundPoundInterrupted?.Invoke();
        }

        private void OnGroundPoundFinished(GameObject hitObject)
        {
            GroundPoundFinished?.Invoke(hitObject);
        }

        private void OnWallSlideStarted()
        {
            WallSlideStarted?.Invoke();
        }

        private void OnWallSlideEnded()
        {
            WallSlideEnded?.Invoke();
        }
        
        private void OnWallJumped()
        {
            WallJumped?.Invoke();
        }
    }
}
