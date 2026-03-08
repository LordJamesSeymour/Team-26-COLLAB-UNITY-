using UnityEngine;

public class SwingGun : MonoBehaviour
{
	[Header("References")]
	[SerializeField] InputManager2 InputManager;
	[SerializeField] LineRenderer lr;
	[SerializeField] Transform gunTip, cam, player;
	[SerializeField] LayerMask m_lGrappable;
	[SerializeField] PlayerController PlayerController;

	[Header("Swinging")]
	[SerializeField] float maxSwingDistance = 25f;
	private Vector3 swingPoint;
	private SpringJoint joint;

	[Header("OMDGear")]
	[SerializeField] Transform Orientation;
	private Rigidbody rb;
	[SerializeField] float horizontalThrustForce;
	[SerializeField] float forwardThrustForce;
	[SerializeField] float extendedCableSpeed;

	private Vector2 m_vMoveInput;
	private bool m_bJumping;
	private Vector3 currentGrapplePosition;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}

	private void OnEnable()
	{
		InputManager.OnInteractPressed += StartSwing;
		InputManager.OnInteractCanceled += StopSwing;

		InputManager.OnJumpPressed += GetJump;
		InputManager.OnJumpRelease += StopJump;
	}

	private void OnDisable()
	{
		InputManager.OnInteractPressed -= StartSwing;
		InputManager.OnInteractCanceled -= StopSwing;

		InputManager.OnJumpPressed -= GetJump;
		InputManager.OnJumpRelease -= StopJump;
	}

	private void LateUpdate()
	{
		DrawRope();
	}

	private void FixedUpdate()
	{
		// Do nothing unless currently swinging
		if (joint == null)
			return;

		// Pull towards swing point while jump is held
		if (m_bJumping)
		{
			Vector3 directionToPoint = swingPoint - transform.position;
			rb.AddForce(directionToPoint.normalized * forwardThrustForce * Time.fixedDeltaTime);

			float distanceFromPoint = Vector3.Distance(transform.position, swingPoint);
			joint.maxDistance = distanceFromPoint * 0.8f;
			joint.minDistance = distanceFromPoint * 0.25f;
		}

		// OMD movement while swinging
		ApplySwingInput();
	}

	private void ApplySwingInput()
	{
		if (m_vMoveInput == Vector2.zero)
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

	private void DrawRope()
	{
		if (!joint) return;

		currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);

		lr.SetPosition(0, gunTip.position);
		lr.SetPosition(1, swingPoint);
	}

	private void StartSwing()
	{
		Debug.Log("Fire");

		PlayerController.m_bActiveSwing = true;

		RaycastHit hit;
		if (Physics.Raycast(cam.position, cam.forward, out hit, maxSwingDistance, m_lGrappable))
		{
			swingPoint = hit.point;
			joint = player.gameObject.AddComponent<SpringJoint>();
			joint.autoConfigureConnectedAnchor = false;
			joint.connectedAnchor = swingPoint;

			float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

			joint.maxDistance = distanceFromPoint * 0.8f;
			joint.minDistance = distanceFromPoint * 0.25f;

			joint.spring = 4.5f;
			joint.damper = 7f;
			joint.massScale = 4.5f;

			lr.positionCount = 2;
			currentGrapplePosition = gunTip.position;
		}
	}

	private void StopSwing()
	{
		Debug.Log("Unfire");

		PlayerController.m_bActiveSwing = false;
		m_bJumping = false;
		m_vMoveInput = Vector2.zero;

		lr.positionCount = 0;

		if (joint != null)
			Destroy(joint);
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
}