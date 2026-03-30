using UnityEngine;
using Group26.Player.Camera;
using Group26.Player.Inputs;
using Group26.Player.Utility;

namespace Group26.Player.Movement
{
    public class GrappleGun : MonoBehaviour
    {
        [Header("References")]
        private InputManager InputManager;
        private PlayerModeSwitcher playerModeSwitcher;
        private CameraModeManager cameraModeManager;
        [SerializeField] private Transform firstPersonCam;
        [SerializeField] private Transform thirdPersonCam;
        private Transform Cam;
        [SerializeField] private Transform gunTip;
        [SerializeField] private LayerMask m_grappableLayer;
        [SerializeField] private LineRenderer lineRenderer;
        private PlayerController PlayerController;
        private Vector3 grapplePoint;
        [SerializeField] private Transform m_maincam;

        [Header("Grappling")]
        [SerializeField] private float maxGrappleDistance;
        [SerializeField] private float grappleDelayTime;
        [SerializeField] private float straightGrappleSpeed = 35f;
        [SerializeField] private bool m_preventGrappleThroughWalls = true;
        [SerializeField] private LayerMask m_ignoredGrapplePredictionLayer;

        [Header("Prediction")]
        [SerializeField] RaycastHit predictionHit;
        [SerializeField] float predictionSphereCastRadius;
        [SerializeField] Transform predictionPoint;

        [Header("Cooldown")]
        [SerializeField] private float grappleCooldown;
        private float grappleCooldownTimer;
        private bool m_bGrappling;
        private int _grappleToken = 0;

        [Header("Debug")]
        [SerializeField] private bool m_logGrappleCooldown = false;
        [SerializeField] private bool m_logIncorrectLayerHits = false;
        [SerializeField] private bool m_bDrawPredictionRays = false;

        private void Awake()
        {
            if (InputManager == null) InputManager = GetComponent<InputManager>();
            if (PlayerController == null) PlayerController = GetComponent<PlayerController>();
            if (cameraModeManager == null) cameraModeManager = GetComponent<CameraModeManager>();
            if (playerModeSwitcher == null) playerModeSwitcher = GetComponent<PlayerModeSwitcher>();

            if (predictionPoint != null)
                predictionPoint.gameObject.SetActive(false);

            //Cam = cameraModeManager.currentCameraMode == CameraMode.FirstPerson ? firstPersonCam : thirdPersonCam;

            Cam = thirdPersonCam;

            m_ignoredGrapplePredictionLayer = ~m_ignoredGrapplePredictionLayer;
        }

        private void OnEnable()
        {
            InputManager.OnGrapplePressed += StartGrapple;
            InputManager.OnGrappleReleased += StopGrapple;

            InputManager.OnCameraSwitchPressed += HandleCameraSwitch;
        }

        private void OnDisable()
        {
            InputManager.OnGrapplePressed -= StartGrapple;
            InputManager.OnGrappleReleased -= StopGrapple;

            InputManager.OnCameraSwitchPressed -= HandleCameraSwitch;
            CancelInvoke();
        }

        private void HandleCameraSwitch()
        {
            if (lineRenderer != null)
                lineRenderer.enabled = false;
        }

        private void LateUpdate()
        {
            if (m_bGrappling && lineRenderer != null)
                lineRenderer.SetPosition(0, gunTip.position);
        }

        private void FixedUpdate()
        {
            if (grappleCooldownTimer > 0f)
                grappleCooldownTimer -= Time.fixedDeltaTime;

            CheckForGrapplePoints();

            if (m_logGrappleCooldown)
                Debug.Log(grappleCooldownTimer);
        }

        private void CheckForGrapplePoints()
        {
            if (m_bGrappling)
            {
                if (predictionPoint != null)
                    predictionPoint.gameObject.SetActive(false);
                return;
            }

            //Cam = cameraModeManager.currentCameraMode == CameraMode.FirstPerson ? firstPersonCam : thirdPersonCam;
            Cam = thirdPersonCam;

            Physics.SphereCast(Cam.position, predictionSphereCastRadius, Cam.forward, out RaycastHit sphereCastHit, maxGrappleDistance, m_grappableLayer);
            Physics.Raycast(Cam.position, Cam.forward, out RaycastHit raycastHit, maxGrappleDistance, m_grappableLayer);

            Vector3 realHitPoint;

            if (raycastHit.point != Vector3.zero)
                realHitPoint = raycastHit.point;
            else if (sphereCastHit.point != Vector3.zero)
                realHitPoint = sphereCastHit.point;
            else
                realHitPoint = Vector3.zero;

            if (realHitPoint != Vector3.zero)
            {
                if (predictionPoint != null)
                {
                    predictionPoint.gameObject.SetActive(true);
                    predictionPoint.position = realHitPoint;
                }
            }
            else
            {
                if (predictionPoint != null)
                    predictionPoint.gameObject.SetActive(false);
            }

            predictionHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;
        }

        void StartGrapple()
        {
            //Cam = cameraModeManager.currentCameraMode == CameraMode.FirstPerson ? firstPersonCam : thirdPersonCam;
            Cam = thirdPersonCam;

            if (grappleCooldownTimer > 0f) return;
            if (m_bGrappling) return;
            if (playerModeSwitcher.currentMode != PlayerMode.CapsuleMode) return;

            if (predictionHit.point == Vector3.zero)
            {
                PlayerController.m_bFreeze = false;
                return;
            }

            if (m_preventGrappleThroughWalls)
            {
                Transform camera = null;
                if (m_maincam != null)
                {
                    camera = m_maincam;
                }
                else
                {
                    Debug.LogWarning("No main cam reference set in the GrappleGun script, obstacle prevent ray may be inaccurate");
                    camera = Cam;
                }

                RaycastHit obstaclePrevention;
                Vector3 distance = predictionHit.point - camera.position;

                Physics.Raycast(camera.position, distance.normalized, out obstaclePrevention, maxGrappleDistance, m_ignoredGrapplePredictionLayer);

                if (obstaclePrevention.collider != null)
                {
                    if (m_bDrawPredictionRays)
                    {
                        Vector3 direction = obstaclePrevention.point - camera.position;
                        Debug.DrawRay(camera.position, direction.normalized * maxGrappleDistance, Color.red, 100.0f);
                    }

                    if (obstaclePrevention.collider.gameObject != null)
                    {
                        if ((1 << obstaclePrevention.collider.gameObject.layer) != m_grappableLayer.value)
                        {
                            if (m_logIncorrectLayerHits)
                            {
                                Debug.Log("Grapple cancelled because it hit an object with the layer: " + (1 << obstaclePrevention.collider.gameObject.layer) + " first, instead of the expected " + m_grappableLayer.value.ToString() + " layer");
                                Debug.Log("Hit object is named: " + obstaclePrevention.collider.gameObject.name);
                            }
                            return;
                        }
                    }
                }
            }

            GetComponent<SwingGun>().StopSwing();

            m_bGrappling = true;
            _grappleToken++;

            CancelInvoke();

            PlayerController.m_bFreeze = true;

            grapplePoint = predictionHit.point;

            if (predictionPoint != null)
                predictionPoint.gameObject.SetActive(false);

            Invoke(nameof(ExecuteGrapple_InvokeWrapper), Mathf.Max(grappleDelayTime, 0f));
        }

        private void StopGrapple()
        {
            ForceStopGrapple();
        }

        private void ExecuteGrapple_InvokeWrapper() => ExecuteGrapple(_grappleToken);
        private void StopGrapple_InvokeWrapper() => StopGrapple(_grappleToken);

        void ExecuteGrapple(int token)
        {
            if (token != _grappleToken) return;

            PlayerController.m_bFreeze = false;

            PlayerController.GrappleToPositionStraight(grapplePoint, straightGrappleSpeed);

            float distanceToTarget = Vector3.Distance(transform.position, grapplePoint);
            float autoStopDelay = (straightGrappleSpeed > 0.01f)
                ? (distanceToTarget / straightGrappleSpeed) + 0.15f
                : 1f;

            Invoke(nameof(StopGrapple_InvokeWrapper), Mathf.Max(autoStopDelay, 0.1f));
        }

        public void ForceStopGrapple()
        {
            StopGrapple(_grappleToken);
        }

        private void StopGrapple(int token)
        {
            if (token != _grappleToken) return;

            PlayerController.m_bFreeze = false;
            m_bGrappling = false;
            grappleCooldownTimer = grappleCooldown;

            if (lineRenderer != null)
                lineRenderer.enabled = false;
        }

        public Vector3 GetGrapplePoint()
        {
            return grapplePoint;
        }

        public Transform GetGunTip()
        {
            return gunTip;
        }

        public bool IsRopeActive()
        {
            return m_bGrappling;
        }
    }
}