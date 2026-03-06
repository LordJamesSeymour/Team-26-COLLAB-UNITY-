using UnityEngine;

public class Dashing : MonoBehaviour
{
	[Header("References")]
	[SerializeField] Transform orientation;
	[SerializeField] Transform playerCam;
	private InputManager2 InputManager;
	private Rigidbody rb;
	private PlayerController PlayerController;

	[Header("Dashing")]
	[SerializeField] float dashForce;
	[SerializeField] float dashUpwardsForce;
	[SerializeField] float maxDashYSpeed;
	[SerializeField] float dashDuration;

	[Header("Cooldown")]
	[SerializeField] float dashCd;
	private float dashCdTimer;

	[Header("Settings")]
	[SerializeField] bool useCameraForward = true;
	[SerializeField] bool allowAllDirections = true;
	[SerializeField] bool disableGravity = true;
	[SerializeField] bool resetVel = true;

	private float horizontalinput;
	private float verticalinput;

	private void OnEnable()
	{
		if (InputManager == null)
			InputManager = GetComponent<InputManager2>();
		if (rb == null)
			rb = GetComponent<Rigidbody>();
		if (PlayerController)
			PlayerController = GetComponent<PlayerController>();

		InputManager.OnDashPressed += Dash;
	}

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		PlayerController = GetComponent<PlayerController>();
	}

	private void FixedUpdate()
	{
		if (dashCdTimer > 0)
			dashCdTimer -= Time.deltaTime;
	}

	private void Dash()
	{
		if (dashCdTimer > 0) return;
		else dashCdTimer = dashCd;

		PlayerController.m_bDashing = true;
		PlayerController.maxYSpeed = maxDashYSpeed;

		Transform forwardT;
		if (useCameraForward)
			forwardT = playerCam;
		else
			forwardT = orientation;

		Vector3 direction = GetDirection(forwardT);
		Vector3 forceToApply = direction * dashForce + orientation.up * dashUpwardsForce;

		if (disableGravity)
			rb.useGravity = false;

		delayedForceToApply = forceToApply;
		Invoke(nameof(DelayedDashForce), 0.025f);
		Invoke(nameof(ResetDash), dashDuration);
	}

	private Vector3 delayedForceToApply;
	private void DelayedDashForce()
	{
		if (resetVel)
			rb.linearVelocity = Vector3.zero;

		rb.AddForce(delayedForceToApply, ForceMode.Impulse); 
	}

	private void ResetDash()
	{
		PlayerController.m_bDashing = false;
		PlayerController.maxYSpeed = 0;


		if (disableGravity)
			rb.useGravity = true;

	}

	public void GetInput(Vector2 Inputs)
	{
		horizontalinput = Inputs.x;
		verticalinput = Inputs.y;
	}

	private Vector3 GetDirection(Transform forwardT)
	{
		Vector3 direction = new Vector3();

		if (allowAllDirections)
		{
			direction = forwardT.forward * verticalinput + forwardT.right * horizontalinput;
		}
		else
			direction = forwardT.forward;

		if (verticalinput == 0 && horizontalinput == 0)
			direction = forwardT.forward;

		return direction.normalized;
	}
}
