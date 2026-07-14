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

        private Animator animator;
        private CharacterStatusProvider characterStatusProvider;
        private CharacterEventDispatcher characterEventDispatcher;

        private bool isGroundPounding;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            characterStatusProvider = GetComponent<CharacterStatusProvider>();
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
                Mathf.Abs(characterStatusProvider.Velocity.x));

            animator.SetBool(
                IsGroundedHash,
                characterStatusProvider.IsGrounded);
        }

        private void TriggerJumped(float _)
        {
            animator.SetTrigger(JumpedHash);
        }

        private void TriggerLanded(CharacterHitEvent _)
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
        
        private void TriggerGroundPounded(CharacterHitEvent _)
        {
            animator.SetTrigger(GroundPoundedHash);
            StopGroundPounding();
        }
    }
}
