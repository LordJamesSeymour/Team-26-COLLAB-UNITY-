using UnityEngine;

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

	[Tooltip("Extra acceleration down slopes.")]
	[SerializeField] float slopeGravity = 18f;

	[Tooltip("Small force into the slope so you don't bounce off it.")]
	[SerializeField] float stickToSlopeForce = 25f;

	[SerializeField] float slideYScale = 0.5f;

	[Tooltip("Ends slide when speed is very low.")]
	[SerializeField] float minSpeedToKeepSliding = 1.5f;

	[Header("Start/Stop Tweaks")]
	[Tooltip("If your speed is below this, slide won't start (prevents flicker from standstill). Set to 0 if you want slide-from-idle.")]
	[SerializeField] float minSpeedToStartSlide = 2.0f;

	[Tooltip("Ignore the grounded-check for this long after starting (prevents scale/groundcheck flicker).")]
	[SerializeField] float groundedGraceTime = 0.08f;

	[Tooltip("Tiny downward impulse on start to keep contact (not the old slam).")]
	[SerializeField] float startStickDownImpulse = 1.0f;

	float slideTimer;
	float startYScale;

	float horizontalInput;
	float verticalInput;

	float slideStartedAt;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
		playerController = GetComponent<PlayerController>();

		if (playerObj == null) playerObj = transform;
		startYScale = playerObj.localScale.y;
	}

	void FixedUpdate()
	{
		if (!playerController.sliding)
			return;

		// Grace period so scaling doesn't instantly "unground" you and cancel the slide
		bool inGrace = (Time.time - slideStartedAt) < groundedGraceTime;

		if (!inGrace && !playerController.IsGrounded)
		{
			EndSlide();
			return;
		}

		slideTimer -= Time.fixedDeltaTime;

		SlidingMovement();

		Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

		if (slideTimer <= 0f || (!inGrace && flatVel.magnitude < minSpeedToKeepSliding))
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
			if (!playerController.sliding)
				StartSlide();
		}
		else
		{
			if (playerController.sliding)
				EndSlide();
		}
	}

	private void StartSlide()
	{
		// Don’t start slide if we’re not grounded (prevents ledge flicker)
		if (!playerController.IsGrounded)
			return;

		// Optional: don’t start if basically stationary (prevents “instant end”)
		Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
		if (flatVel.magnitude < minSpeedToStartSlide)
			return;

		playerController.sliding = true;
		slideStartedAt = Time.time;

		playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
		slideTimer = maxSlideTime;

		// Gentle stick-down so ground check stays true after scaling
		rb.linearVelocity = new Vector3(rb.linearVelocity.x, Mathf.Min(rb.linearVelocity.y, 0f), rb.linearVelocity.z);
		rb.AddForce(Vector3.down * startStickDownImpulse, ForceMode.Impulse);
	}

	private void SlidingMovement()
	{
		bool onSlope = playerController.OnSlope();
		Vector3 slopeNormal = onSlope ? playerController.SlopeNormal : Vector3.up;

		Vector3 velOnPlane = Vector3.ProjectOnPlane(rb.linearVelocity, slopeNormal);

		Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
		Vector3 inputOnPlane = Vector3.ProjectOnPlane(inputDirection, slopeNormal);

		if (inputOnPlane.sqrMagnitude > 0.01f)
			rb.AddForce(inputOnPlane.normalized * steerForce, ForceMode.Force);

		rb.AddForce(-velOnPlane * slideFriction, ForceMode.Acceleration);

		if (onSlope)
		{
			Vector3 slopeDown = Vector3.ProjectOnPlane(Vector3.down, slopeNormal).normalized;
			rb.AddForce(slopeDown * slopeGravity, ForceMode.Acceleration);

			rb.AddForce(-slopeNormal * stickToSlopeForce, ForceMode.Acceleration);
		}
	}

	private void EndSlide()
	{
		playerController.sliding = false;
		playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
	}

	public void ForceEndSlide()
	{
		if (playerController.sliding)
			EndSlide();
	}
}