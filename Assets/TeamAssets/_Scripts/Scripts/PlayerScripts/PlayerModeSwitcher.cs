using UnityEngine;
using Group26.Player.Movement;
using Group26.Player.Inputs;

namespace Group26.Player.Utility
{
    public enum PlayerMode
    {
        CapsuleMode,
        BallMode
    }
    public class PlayerModeSwitcher : MonoBehaviour
    {
        [Header("References")]
        private PlayerController playerController;
        private BallRollController ballRollController;
        private InputManager inputManager;

        [SerializeField] private GameObject capsuleModeObject;
        [SerializeField] private GameObject ballModeObject;

        public PlayerMode currentMode = PlayerMode.CapsuleMode;

        private GrappleGun m_grappleGunScript;

        private void Awake()
        {
            if(playerController == null)
            {
                playerController = GetComponent<PlayerController>();
            }
            if(ballRollController == null)
            {
                ballRollController = GetComponent<BallRollController>();
            }
            if(inputManager == null)
            {
                inputManager = GetComponent<InputManager>();
            }

            if(capsuleModeObject == null || ballModeObject == null)
            {
                Debug.LogError("PlayerModeSwitcher: One or more mode objects are not assigned.");
            }

            m_grappleGunScript = GetComponent<GrappleGun>();
            if (m_grappleGunScript == null) {

                Debug.LogError("Grapple gun script is not attached");
            
            }

        }

        private void OnEnable()
        {
            inputManager.OnModeSwitchPressed += SwitchMode;
        }

        private void OnDisable()
        {
            inputManager.OnModeSwitchPressed -= SwitchMode;
        }

        private void Start()
        {
            ApplyMode(currentMode);
        }

        private void SwitchMode()
        {
            currentMode = currentMode == PlayerMode.CapsuleMode ? PlayerMode.BallMode : PlayerMode.CapsuleMode;
            ApplyMode(currentMode);
        }

        private void ApplyMode(PlayerMode mode)
        {
            switch(mode)
            {
                case PlayerMode.CapsuleMode:
                    ballModeObject.SetActive(false);
                    capsuleModeObject.SetActive(true);
                    playerController.enabled = true;
                    ballRollController.enabled = false;
                    break;
                case PlayerMode.BallMode:
                    capsuleModeObject.SetActive(false);
                    ballModeObject.SetActive(true);
                    playerController.m_bIsGrounded = false;
                    playerController.enabled = false;
                    ballRollController.enabled = true;

                    if (m_grappleGunScript != null)
                        m_grappleGunScript.ForceStopGrapple();
                    if (playerController != null)
                        playerController.ReleaseGrappleMovement();
                    break;
            }
        }
    }
}