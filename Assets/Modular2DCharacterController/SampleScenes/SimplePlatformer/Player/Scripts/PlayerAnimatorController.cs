using Modular2DCharacterController.Runtime.Core;
using UnityEngine;

namespace Modular2DCharacterController.SampleScenes.SimplePlatformer.Player.Scripts
{
    public class PlayerAnimatorController : MonoBehaviour
    {
        private static readonly int HorizontalVelocityHash =
            Animator.StringToHash("HorizontalVelocity");

        private static readonly int IsGroundedHash =
            Animator.StringToHash("IsGrounded");
        
        private static readonly int JumpedHash =
            Animator.StringToHash("Jumped");
        
        private static readonly int LandedHash =
            Animator.StringToHash("Landed");
        
        private static readonly int GroundPoundedHash =
            Animator.StringToHash("GroundPounded");

        private Rigidbody2D rigidbody2D;
        private Animator animator;
        private GroundDetector groundDetector;
        private CharacterEventDispatcher characterEventDispatcher;

        private bool isGroundPounding;

        private void Awake()
        {
            rigidbody2D = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            groundDetector = GetComponent<GroundDetector>();
            characterEventDispatcher = GetComponent<CharacterEventDispatcher>();

            if (characterEventDispatcher != null)
            {
                characterEventDispatcher.Jumped += TriggerJumped;
                characterEventDispatcher.Landed += TriggerLanded;
                characterEventDispatcher.GroundPoundStarted += StartGroundPounding;
                characterEventDispatcher.GroundPoundInterrupted += StopGroundPounding;
                characterEventDispatcher.GroundPoundFinished += TriggerGroundPounded;
            }
        }

        private void Update()
        {
            animator.SetFloat(
                HorizontalVelocityHash,
                Mathf.Abs(rigidbody2D.linearVelocity.x));

            animator.SetBool(
                IsGroundedHash,
                groundDetector.IsGrounded);
        }

        private void TriggerJumped(float _)
        {
            animator.SetTrigger(JumpedHash);
        }

        private void TriggerLanded(Vector2 _)
        {
            if(!isGroundPounding)
                animator.SetTrigger(LandedHash);
        }

        private void StartGroundPounding()
        {
            isGroundPounding = true;
        }
        
        private void StopGroundPounding()
        {
            isGroundPounding = false;
        }
        
        private void TriggerGroundPounded(GameObject _)
        {
            animator.SetTrigger(GroundPoundedHash);
            StopGroundPounding();
        }
    }
}