using UnityEngine;

public class SwingGun : MonoBehaviour
{
	[Header ("References")]
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

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}

	void OnEnable()
	{
		InputManager.OnInteractPressed += StartSwing;
		InputManager.OnInteractCanceled += StopSwing;

		InputManager.OnJumpPressed += GetJump;
		InputManager.OnJumpRelease += StopJump;
	}

	void OnDisable()
	{
		InputManager.OnInteractPressed -= StartSwing;
		InputManager.OnInteractCanceled -= StopSwing;

		InputManager.OnJumpPressed -= GetJump;

		CancelInvoke();
	}
	private void LateUpdate()
	{
		DrawRope();
	}

	private void FixedUpdate()
	{
		if (m_bJumping)
		{
			Vector3 directionToPoint = swingPoint - transform.position;
			rb.AddForce(directionToPoint.normalized * forwardThrustForce * Time.deltaTime);

			float distanceFromPoint = Vector3.Distance(transform.position, swingPoint);

			joint.maxDistance = distanceFromPoint * 0.8f;
			joint.minDistance = distanceFromPoint * 0.25f;
		}
	}

	private Vector3 currentGrapplePosition;
	void DrawRope()
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

			// The distance grapple will attempt to keep from the grapple point
			joint.maxDistance = distanceFromPoint * 0.8f;
			joint.minDistance = distanceFromPoint * 0.25f;

			// Customizable values
			joint.spring = 4.5f;
			joint.damper = 7f;
			joint.massScale = 4.5f;

			lr.positionCount = 2;
			currentGrapplePosition = gunTip.position;
		}
	}

	void StopSwing()
	{
		Debug.Log("Unfire");

		PlayerController.m_bActiveSwing = false;
		lr.positionCount = 0;
		Destroy(joint);
	}

	public void GetInput(Vector2 Inputs)
	{
		// OMD Movement style for swinging:
		
		while (Inputs != new Vector2(0, 0))
		{
			//  Forwards
			if (Inputs.y != 0 && Inputs.y > 0)
				rb.AddForce(Orientation.right * forwardThrustForce * Time.deltaTime);
			// Debug.Log("Forwards");

			//  Left
			if (Inputs.x != 0 && Inputs.x < 0)
				rb.AddForce(-Orientation.right * horizontalThrustForce * Time.deltaTime);
			// Debug.Log("Left");

			//  Right
			if (Inputs.x != 0 && Inputs.x > 0)
				rb.AddForce(Orientation.right * horizontalThrustForce * Time.deltaTime);
			// Debug.Log("Right");

			//  Backwards (EXTEND CABLE)
			if (Inputs.y != 0 && Inputs.y < 0)
			{
				float extendDistanceFromPoint = Vector3.Distance(transform.position, swingPoint) + extendedCableSpeed;

				joint.maxDistance = extendDistanceFromPoint * 0.8f;
				joint.minDistance = extendDistanceFromPoint * 0.25f;
			}
		}
	}

	private bool m_bJumping;
	void GetJump()
	{
		m_bJumping = true;
	}

	void StopJump()
	{
		m_bJumping = false;
	}
}
