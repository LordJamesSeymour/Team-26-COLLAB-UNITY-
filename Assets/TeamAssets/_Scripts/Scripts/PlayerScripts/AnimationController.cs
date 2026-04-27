using Group26.Player.Inputs;
using Group26.Player.Movement;
using UnityEngine;

namespace Group26.Player.Animation
{
    public class AnimationController : MonoBehaviour
    {
        [Header("Script References")]
        private InputManager inputManager;
        private PlayerController playerController;
        private WallRunning wallRunning;

        [Header("Component References")]
        [SerializeField] private Animator animator;

        [Header("Hash IDs")]
        private static readonly int VelocityHash = Animator.StringToHash("Velocity");
        private static readonly int JumpHash = Animator.StringToHash("jumpTriggered");
        private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
        private static readonly int isGrappling = Animator.StringToHash("isGrappling");
        private static readonly int isSwinging = Animator.StringToHash("isSwinging");
        private static readonly int isDashing = Animator.StringToHash("isDashing");
        private static readonly int isWallRunningRight = Animator.StringToHash("wallRunningRight");
        private static readonly int isWallRunningLeft = Animator.StringToHash("wallRunningLeft");

        private void Awake()
        {
            inputManager = GetComponentInParent<InputManager>();
            playerController = GetComponentInParent<PlayerController>();
            wallRunning = GetComponentInParent<WallRunning>();

            if(animator == null)
            {
                Debug.LogError("Animator reference is not assigned.");
            }
        }

        private void OnEnable()
        {
            if(inputManager != null) inputManager.OnJumpPressed += HandleJumpAnimation;
        }

        private void OnDisable()
        {
            if(inputManager != null) inputManager.OnJumpPressed -= HandleJumpAnimation;
        }

        // Update is called once per frame
        void Update()
        {
            if(animator == null) return;

            float velocity = ComputeVelocity();
            animator.SetFloat(VelocityHash, velocity);

            if(playerController != null)
            {
                animator.SetBool(IsGroundedHash, playerController.m_bIsGrounded);
                animator.SetBool(isGrappling, playerController.m_bActiveGrapple);
                animator.SetBool(isSwinging, playerController.m_bActiveSwing);
                animator.SetBool(isDashing, playerController.m_bDashing);
            }

            if(wallRunning != null)
            {
                animator.SetBool(isWallRunningRight, wallRunning.wallRight);
                animator.SetBool(isWallRunningLeft, wallRunning.wallLeft);
            }
        }

        private float ComputeVelocity()
        {
            float vel = inputManager != null ? Mathf.Clamp01(inputManager.MoveInput.magnitude) : 0f;

            return vel;
        }

        private void HandleJumpAnimation()
        {
            if(playerController == null || animator == null) return;
            if(!playerController.m_bIsGrounded) return;

            animator.SetTrigger(JumpHash);
        }
    }
}