using UnityEngine;

public class SwingGun : MonoBehaviour
{
	[Header("References")]
	[SerializeField] InputManager2 InputManager;
	public Transform gunTip, cam, player;
	[SerializeField] LayerMask m_lGrappable;
	[SerializeField] PlayerController PlayerController;

	[Header("Swinging")]
	[SerializeField] float maxSwingDistance = 25f;
	private Vector3 swingPoint;
	[HideInInspector] public SpringJoint joint;

	[Header("OMDGear")]
	[SerializeField] Transform Orientation;
	private Rigidbody rb;
	[SerializeField] float horizontalThrustForce;
	[SerializeField] float forwardThrustForce;
	[SerializeField] float extendedCableSpeed;

	[Header("Prediction")]
	[SerializeField] RaycastHit predictionHit;
	[SerializeField] float predictionSphereCastRadius;
	[SerializeField] Transform predictionPoint;

	private Vector2 m_vMoveInput;
	private bool m_bJumping;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		swingPoint = gunTip.position;
	}

	private void OnEnable()
	{
		InputManager.OnSwingPressed += StartSwing;
		InputManager.OnSwingRelease += StopSwing;

		InputManager.OnJumpPressed += GetJump;
		InputManager.OnJumpRelease += StopJump;
	}

	private void OnDisable()
	{
		InputManager.OnSwingPressed -= StartSwing;
		InputManager.OnSwingRelease -= StopSwing;

		InputManager.OnJumpPressed -= GetJump;
		InputManager.OnJumpRelease -= StopJump;
	}

	private void FixedUpdate()
	{
		// OMD movement while swinging
		ApplySwingInput();
		CheckForSwingPoints();

		// Pull towards swing point while jump is held
		if (m_bJumping)
		{
			Vector3 directionToPoint = swingPoint - transform.position;
			rb.AddForce(directionToPoint.normalized * forwardThrustForce * Time.fixedDeltaTime);

			float distanceFromPoint = Vector3.Distance(transform.position, swingPoint);
			joint.maxDistance = distanceFromPoint * 0.8f;
			joint.minDistance = distanceFromPoint * 0.25f;
		}	
	}

	void CheckForSwingPoints()
	{
		if (joint != null) return;

		RaycastHit sphereCastHit;
		Physics.SphereCast(cam.position, predictionSphereCastRadius, cam.forward, out sphereCastHit, maxSwingDistance, m_lGrappable);

		RaycastHit raycastHit;
		Physics.Raycast(cam.position, cam.forward, out raycastHit, maxSwingDistance, m_lGrappable);


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
			rb.AddForce(Orientation.forward * forwardThrustForce * Time.fixedDeltaTime);

		// Left
		if (m_vMoveInput.x < 0f)
			rb.AddForce(-Orientation.right * horizontalThrustForce * Time.fixedDeltaTime);

		// Right
		if (m_vMoveInput.x > 0f)
			rb.AddForce(Orientation.right * horizontalThrustForce * Time.fixedDeltaTime);

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

		GetComponent<Grappling>().ForceStopGrapple();
		PlayerController.ResetRestrictions();

		// Safety: remove any previous joint reference first
		//if (joint != null)
		//{
		//	Destroy(joint);
		//	joint = null;
		//}

		PlayerController.m_bActiveSwing = true;
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
		PlayerController.m_bActiveSwing = false;
		m_bJumping = false;
		m_vMoveInput = Vector2.zero;
		swingPoint = gunTip.position;

		if (joint != null)
		{
			Destroy(joint);
			joint = null;
		}
	}

	public void GetInput(Vector2 inputs)
	{
		m_vMoveInput = inputs;
	}

	private void GetJump()
	{
		m_bJumping = true;
	}

	private void StopJump()
	{
		m_bJumping = false;
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