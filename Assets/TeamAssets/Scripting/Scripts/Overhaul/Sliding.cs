using UnityEngine;
using Group26.Player.Movement;

public class Sliding : MonoBehaviour
{
	[Header("References")]
	[SerializeField] Transform orientation;
	[SerializeField] Transform playerObj;

	Rigidbody rb;
	PlayerController playerController;

	[Header("Sliding")]
	[SerializeField] float maxSlideTime = 1.0f;

	[Tooltip("How much you can steer while sliding.")]
	[SerializeField] float steerForce = 35f;

	[Tooltip("Friction applied to velocity along the ground/slope.")]
	[SerializeField] float slideFriction = 6f;

	[Tooltip("Extra acceleration down slopes (lets ramps feel natural).")]
	[SerializeField] float slopeGravity = 18f;

	[Tooltip("Small force into the slope so you don't bounce off it.")]
	[SerializeField] float stickToSlopeForce = 25f;

	[SerializeField] float slideYScale = 0.5f;

	[Tooltip("Ends slide when speed is very low.")]
	[SerializeField] float minSpeedToKeepSliding = 1.5f;

	float slideTimer;
	float startYScale;

	float horizontalInput;
	float verticalInput;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
		playerController = GetComponent<PlayerController>();

		if (playerObj == null) playerObj = transform;
		startYScale = playerObj.localScale.y;
	}

	void FixedUpdate()
	{
		if (!playerController.m_bSliding)
			return;

		// If we leave the ground (jump/ledge), end slide
		if (!playerController.IsGrounded)
		{
			EndSlide();
			return;
		}

		slideTimer -= Time.fixedDeltaTime;

		SlidingMovement();

		// End conditions
		Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
		if (slideTimer <= 0f || flatVel.magnitude < minSpeedToKeepSliding)
			EndSlide();
	}

	public void GetInput(Vector2 input)
	{
		horizontalInput = input.x;
		verticalInput = input.y;
	}

	public void Slide(bool pressed)
	{
		if (pressed)
		{
			if (!playerController.m_bSliding)
				StartSlide();
		}
		else
		{
			if (playerController.m_bSliding)
				EndSlide();
		}
	}

	private void StartSlide()
	{
		playerController.m_bSliding = true;

		// Scale once (no repeated slamming)
		playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);

		slideTimer = maxSlideTime;
	}

	private void SlidingMovement()
	{
		bool onSlope = playerController.OnSlope();
		Vector3 slopeNormal = onSlope ? playerController.SlopeNormal : Vector3.up;

		// Velocity along the surface
		Vector3 velOnPlane = Vector3.ProjectOnPlane(rb.linearVelocity, slopeNormal);

		// Steering direction along the surface
		Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
		Vector3 inputOnPlane = Vector3.ProjectOnPlane(inputDirection, slopeNormal);

		if (inputOnPlane.sqrMagnitude > 0.01f)
		{
			rb.AddForce(inputOnPlane.normalized * steerForce, ForceMode.Force);
		}

		// Friction: slows momentum over time (so ramps kill speed naturally)
		rb.AddForce(-velOnPlane * slideFriction, ForceMode.Acceleration);

		if (onSlope)
		{
			// Gravity component along slope: accelerates downhill,
			// but if you have momentum up a ramp, you'll go up until you lose it.
			Vector3 slopeDown = Vector3.ProjectOnPlane(Vector3.down, slopeNormal).normalized;
			rb.AddForce(slopeDown * slopeGravity, ForceMode.Acceleration);

			// Stick to slope (replaces your old "slam down" impulse)
			rb.AddForce(-slopeNormal * stickToSlopeForce, ForceMode.Acceleration);
		}
	}

	private void EndSlide()
	{
		playerController.m_bSliding = false;
		playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
	}

	// Called by PlayerController when jumping
	public void ForceEndSlide()
	{
		if (playerController.m_bSliding)
			EndSlide();
	}
}