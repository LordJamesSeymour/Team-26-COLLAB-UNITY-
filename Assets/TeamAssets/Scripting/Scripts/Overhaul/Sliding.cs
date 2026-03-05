using System.Collections.Specialized;
using Unity.VisualScripting;
using UnityEngine;

public class Sliding : MonoBehaviour
{
	[Header("References")]
	[SerializeField] Transform orientation;
	[SerializeField] Transform playerObj;
	private Rigidbody rb;
	private PlayerController PlayerController;

	[Header("Sliding")]
	[SerializeField] float maxSlideTime;
	[SerializeField] float slideForce;
	[SerializeField] float slideYScale;

	private float slideTimer;
	private float startYScale;

	private float horizontalInput;
	private float verticalInput;

	private bool m_bSliding;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		PlayerController = GetComponent<PlayerController>();

		startYScale = playerObj.localScale.y;
	}

	private void Update()
	{
		if (PlayerController.sliding && (horizontalInput != 0 || verticalInput != 0))
			StartSlide();
	}

	private void FixedUpdate()
	{
		if (PlayerController.sliding)
		{
			SlidingMovement();
		}
	}

	public void GetInput(Vector2 Input)
	{
		horizontalInput = Input.x;
		verticalInput = Input.y;
	}

	public void Slide(bool state)
	{
		if (PlayerController.sliding)
			EndSlide();

		PlayerController.sliding = state;
	}

	private void StartSlide() 
	{
		playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
		rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

		slideTimer = maxSlideTime;
	}

	private void SlidingMovement()
	{
		Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;


		if (!PlayerController.OnSlope() || rb.linearVelocity.y > 0.1f)
		{
			rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

			slideTimer -= Time.deltaTime;
		}

		else rb.AddForce(PlayerController.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);


		if (slideTimer <= 0)
			EndSlide();
	}

	private void EndSlide()
	{
		PlayerController.sliding = false;
		playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
	}
}
