using UnityEngine;
using Group26.Player.Inputs;
using Group26.Player.Camera;

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
        [SerializeField] private LayerMask m_grappableLayer;
        private PlayerController playerController;

        [Header("Swinging")]
        [SerializeField] private float maxSwingDistance = 25f;
        private Vector3 swingPoint;
        [HideInInspector] public SpringJoint joint;
        /// <summary>
        /// Testing variable for toggling the swing through walls prevention
        /// </summary>
        [SerializeField] private bool m_bpreventSwingingThroughWalls = true;

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

        [Header("Debug")]
        [SerializeField] private bool m_bDrawPredictionRays = false;
        [SerializeField] private bool m_bLogIncorrectLayerHits = false;

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
            Physics.SphereCast(Cam.position, predictionSphereCastRadius, Cam.forward, out sphereCastHit, maxSwingDistance, m_grappableLayer);

            RaycastHit raycastHit;
            Physics.Raycast(Cam.position, Cam.forward, out raycastHit, maxSwingDistance, m_grappableLayer);


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

            
            if (m_bpreventSwingingThroughWalls)
            {
                //casts a ray to the predicted grapple point to check for obstacles in the way and prevent grappling through walls
                RaycastHit obstaclePrevention;
                Vector3 distance = predictionHit.point - Cam.position;
                Physics.Raycast(Cam.position, distance.normalized, out obstaclePrevention, maxSwingDistance);
                if (obstaclePrevention.collider != null)
                {
                    if (m_bDrawPredictionRays)
                    {
                        Vector3 direction = obstaclePrevention.point - Cam.position;
                        Debug.DrawRay(Cam.position, direction.normalized * maxSwingDistance, Color.red, 100.0f);
                }

                    if (obstaclePrevention.collider.gameObject != null)
                    {
                        //layers need to be bit shifted to the left by 1 to be compared with a layer mask
                        if ((1 << obstaclePrevention.collider.gameObject.layer) != m_grappableLayer.value)
                        {
                            if (m_bLogIncorrectLayerHits)
                            {
                                Debug.Log("Swing cancelled because it hit an object with the layer: " + (1 << obstaclePrevention.collider.gameObject.layer) + " first, instead of the expected " + m_grappableLayer.value.ToString() + " layer");
                                Debug.Log("Hit object is named: " + obstaclePrevention.collider.gameObject.name);
                            }
                            return;
                        }
                    }
                }
            }


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
            playerController.m_bActiveSwing = false;
            m_bClimbingRope = false;
            m_vMoveInput = Vector2.zero;
            swingPoint = gunTip.position;

            if (joint != null)
            {
                Destroy(joint);
                joint = null;
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