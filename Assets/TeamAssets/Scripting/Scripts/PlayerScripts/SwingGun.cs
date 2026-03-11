using UnityEngine;
using Group26.Player.Inputs;
using Group26.Player.Camera;
using System.Collections;

namespace Group26.Player.Movement
{
    public class SwingGun : MonoBehaviour
    {
        [Header("References")]
        private InputManager inputManager;
        private CameraModeManager cameraModeManager;
        
        [SerializeField] private Transform firstPersonCam; 
		[SerializeField] private Transform thirdPersonCam;
		private Transform Cam;
        public Transform gunTip;
        [SerializeField] private Transform player;
        [SerializeField] private LayerMask m_GrappableLayer;
        private PlayerController playerController;

        [Header("Swinging")]
        [SerializeField] private float maxSwingDistance = 25f;
        private Vector3 swingPoint;
        [HideInInspector] public SpringJoint joint;

        [Header("OMDGear")]
        [SerializeField] private Transform Orientation;
        private Rigidbody rigidBody;
        [SerializeField] private float horizontalThrustForce;
        [SerializeField] private float forwardThrustForce;
        [SerializeField] private float extendedCableSpeed;

        [Header("Prediction")]
        [SerializeField] private RaycastHit predictionHit;
        [SerializeField] private float predictionSphereCastRadius;
        [SerializeField] private Transform predictionPoint;

        [Header("Cancel parameters")]
        [SerializeField] private float m_cancelForce = 10.0f;
        private int m_swingTime = 0;
        [SerializeField] private int m_swingTimeForCancelForce = 10;
        /// <summary>
        /// The maximum force added when cancelling the swing. The negated version of this will be used for the minum swing force.
        /// </summary>
        [SerializeField] private Vector3 m_maxCancelSwingForce = new Vector3(25.0f, 25.0f, 25.0f);

        private Vector2 m_vMoveInput;
        private bool m_bClimbingRope;

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
            playerController = GetComponent<PlayerController>();
            inputManager = GetComponent<InputManager>();
            cameraModeManager = GetComponent<CameraModeManager>();
            if(rigidBody == null) Debug.LogError("No rigidbody found on SwingGun object.");
            if(playerController == null) Debug.LogError("No PlayerController found on SwingGun object.");

            swingPoint = gunTip.position;
        }

        private void OnEnable()
        {
            inputManager.OnSwingStarted += StartSwing;
            inputManager.OnSwingStopped += StopSwing;

            inputManager.OnJumpPressed += GetClimbingRope;
            inputManager.OnJumpRelease += StopClimbingRope;
        }

        private void OnDisable()
        {
            inputManager.OnSwingStarted -= StartSwing;
            inputManager.OnSwingStopped -= StopSwing;

            inputManager.OnJumpPressed -= GetClimbingRope;
            inputManager.OnJumpRelease -= StopClimbingRope;
        }

        private void FixedUpdate()
        {
            GetInput(inputManager.MoveInput);
            CheckForSwingPoints();

            // Only run actual swing physics if a swing joint exists
            if (joint == null)
                return;

            // Pull towards swing point while jump is held
            if (m_bClimbingRope)
            {
                Vector3 directionToPoint = swingPoint - transform.position;
                rigidBody.AddForce(directionToPoint.normalized * forwardThrustForce * Time.fixedDeltaTime);

                float distanceFromPoint = Vector3.Distance(transform.position, swingPoint);
                joint.maxDistance = distanceFromPoint * 0.8f;
                joint.minDistance = distanceFromPoint * 0.25f;
            }

            ApplySwingInput();
        }

        void CheckForSwingPoints()
        {
            if (joint != null) return;

            Cam = cameraModeManager.currentCameraMode == CameraMode.FirstPerson ? firstPersonCam : thirdPersonCam;

            RaycastHit sphereCastHit;
            Physics.SphereCast(Cam.position, predictionSphereCastRadius, Cam.forward, out sphereCastHit, maxSwingDistance, m_GrappableLayer);

            RaycastHit raycastHit;
            Physics.Raycast(Cam.position, Cam.forward, out raycastHit, maxSwingDistance, m_GrappableLayer);


            Vector3 realHitPoint;

            // Option 1 - Direct hit
            if (raycastHit.point != Vector3.zero)
                realHitPoint = raycastHit.point;

            // Option 2 - Indirect (predicted) hit
            else if (sphereCastHit.point != Vector3.zero)
                realHitPoint = sphereCastHit.point;

            // Option 3 - Miss
            else 
                realHitPoint = Vector3.zero;

            // Real hit point found
            if(realHitPoint != Vector3.zero)
            {
                predictionPoint.gameObject.SetActive(true);
                predictionPoint.position = realHitPoint;
            }
            else 
                predictionPoint.gameObject.SetActive(false);

            predictionHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;
        }

        private void ApplySwingInput()
        {
            if (m_vMoveInput == Vector2.zero || joint == null)
                return;

            // Forwards
            if (m_vMoveInput.y > 0f)
                rigidBody.AddForce(Orientation.forward * forwardThrustForce * Time.fixedDeltaTime);

            // Left
            if (m_vMoveInput.x < 0f)
                rigidBody.AddForce(-Orientation.right * horizontalThrustForce * Time.fixedDeltaTime);

            // Right
            if (m_vMoveInput.x > 0f)
                rigidBody.AddForce(Orientation.right * horizontalThrustForce * Time.fixedDeltaTime);

            // Backwards (extend cable)
            if (m_vMoveInput.y < 0f)
            {
                float extendDistanceFromPoint = Vector3.Distance(transform.position, swingPoint) + extendedCableSpeed;

                joint.maxDistance = extendDistanceFromPoint * 0.8f;
                joint.minDistance = extendDistanceFromPoint * 0.25f;
            }
        }

        private void StartSwing()
        {
            if (predictionHit.point == Vector3.zero) return;

            GetComponent<GrappleGun>().ForceStopGrapple();
            playerController.ResetRestrictions();

            // Safety: remove any previous joint reference first
            //if (joint != null)
            //{
            //	Destroy(joint);
            //	joint = null;
            //}

            playerController.m_bActiveSwing = true;
            swingPoint = predictionHit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = swingPoint;

            float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            joint.spring = 4.5f;
            joint.damper = 7f;
            joint.massScale = 4.5f;

            m_swingTime = 0;
            StartCoroutine(SwingTime());

            //RaycastHit hit;
            //if (Physics.Raycast(cam.position, cam.forward, out hit, maxSwingDistance, m_lGrappable))
            //{


            //}
            //else
            //{
            //	swingPoint = gunTip.position;
            //}
        }

        public void StopSwing()
        {
            StopCoroutine(SwingTime());

            playerController.m_bActiveSwing = false;
            m_bClimbingRope = false;
            m_vMoveInput = Vector2.zero;
            swingPoint = gunTip.position;

            if (rigidBody != null && joint != null)
            { 
                //Preventing cancel force spam
                if(m_swingTime >= m_swingTimeForCancelForce)
                {
                    Debug.Log("Applying force of: " + rigidBody.linearVelocity.normalized * m_cancelForce);
                    rigidBody.AddForce(rigidBody.linearVelocity.normalized * m_cancelForce, ForceMode.Impulse);
                }
            }
            
            if (joint != null)
            {
                Destroy(joint);
                joint = null;
            }
            
        }

        private IEnumerator SwingTime()
        {
            while (playerController.m_bActiveSwing)
            {
                yield return new WaitForSeconds(0.1f);
                m_swingTime++;
            }
        }

        private void GetInput(Vector2 inputs)
        {
            m_vMoveInput = inputs;
        }

        private void GetClimbingRope()
        {
            m_bClimbingRope = true;
        }

        private void StopClimbingRope()
        {
            m_bClimbingRope = false;
        }

        public Vector3 GetSwingPoint()
        {
            return swingPoint;
        }

        public bool IsSwinging()
        {
            return joint != null;
        }
    }
}