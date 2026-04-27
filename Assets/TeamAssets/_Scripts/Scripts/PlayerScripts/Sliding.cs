using UnityEngine;
using Group26.Player.Inputs;

namespace Group26.Player.Movement
{
	[DisallowMultipleComponent]
	public class Sliding : MonoBehaviour
	{
		[Header("Slide Settings")]
		[SerializeField] private float minSlideStartSpeed = 8f;
		[SerializeField] private float minSlideEndSpeed = 4f;
		[SerializeField] private float slideStartBoost = 2f;
		[SerializeField] private float slideDrag = 10f;
		[SerializeField] private float maxSlideTime = 1.1f;
		[SerializeField] private float groundStickForce = 20f;

		[Range(0f, 1f)]
		[SerializeField] private float steerAmount = 0.12f;

		[Range(0.25f, 1f)]
		[SerializeField] private float slideHeightMultiplier = 0.5f;

		[SerializeField] private float ungroundedGraceTime = 0.15f;

		private PlayerController playerController;
		private InputManager inputManager;
		private Rigidbody rb;

		private bool isSliding;
		private bool lastCrouchHeld;

		private float slideTimer;
		private float ungroundedTimer;
		private float currentSlideSpeed;

		private Vector3 slideDirection;

		private Transform slideBodyRoot;
		private Vector3 originalBodyScale;

		public bool IsSliding => isSliding;

		private void Awake()
		{
			playerController = GetComponent<PlayerController>();
			inputManager = GetComponent<InputManager>();
			rb = GetComponent<Rigidbody>();

			if (playerController != null)
				slideBodyRoot = playerController.OrientationTransform;

			if (slideBodyRoot == null)
				slideBodyRoot = transform;

			originalBodyScale = slideBodyRoot.localScale;
		}

		private void OnDisable()
		{
			ForceEndSlide();
		}

		private void Update()
		{
			if (playerController == null || inputManager == null || rb == null)
				return;

			bool crouchHeld = inputManager.isCrouching;

			if (!lastCrouchHeld && crouchHeld)
				TryStartSlide();

			if (isSliding && !crouchHeld)
				ForceEndSlide();

			lastCrouchHeld = crouchHeld;
		}

		private void FixedUpdate()
		{
			if (!isSliding || playerController == null || rb == null)
				return;

			UpdateSlide(Time.fixedDeltaTime);
		}

		private void TryStartSlide()
		{
			if (isSliding)
				return;

			if (!playerController.IsGrounded)
				return;

			if (playerController.IsMovementLockedForSlide)
				return;

			float startSpeed = playerController.CurrentHorizontalSpeed;
			if (startSpeed < minSlideStartSpeed)
				return;

			Vector3 flatVelocity = playerController.FlatVelocity;

			if (flatVelocity.sqrMagnitude > 0.01f)
				slideDirection = flatVelocity.normalized;
			else if (playerController.OrientationTransform != null)
				slideDirection = Flatten(playerController.OrientationTransform.forward).normalized;
			else
				slideDirection = Flatten(transform.forward).normalized;

			if (slideDirection.sqrMagnitude < 0.01f)
				slideDirection = Vector3.forward;

			currentSlideSpeed = Mathf.Max(startSpeed + slideStartBoost, minSlideStartSpeed);
			slideTimer = maxSlideTime;
			ungroundedTimer = 0f;
			isSliding = true;

			playerController.SetSliding(true);
			ApplySlideHeight();

			Vector3 startVelocity = slideDirection * currentSlideSpeed;
			rb.linearVelocity = new Vector3(startVelocity.x, rb.linearVelocity.y, startVelocity.z);
		}

		private void UpdateSlide(float deltaTime)
		{
			slideTimer -= deltaTime;

			if (playerController.IsMovementLockedForSlide)
			{
				ForceEndSlide();
				return;
			}

			if (playerController.IsGrounded)
				ungroundedTimer = 0f;
			else
				ungroundedTimer += deltaTime;

			if (ungroundedTimer > ungroundedGraceTime)
			{
				ForceEndSlide();
				return;
			}

			Vector2 moveInput = inputManager.MoveInput;
			Vector3 steerInput = Vector3.zero;

			if (playerController.OrientationTransform != null)
			{
				steerInput = Flatten(
					playerController.OrientationTransform.forward * moveInput.y +
					playerController.OrientationTransform.right * moveInput.x);
			}
			else
			{
				steerInput = Flatten(transform.forward * moveInput.y + transform.right * moveInput.x);
			}

			if (steerInput.sqrMagnitude > 0.001f)
				slideDirection = Vector3.Slerp(slideDirection, steerInput.normalized, steerAmount);

			Vector3 slideMoveDirection = slideDirection;

			if (playerController.OnSlope())
				slideMoveDirection = playerController.GetSlopeMoveDirection(slideMoveDirection);

			currentSlideSpeed = Mathf.MoveTowards(currentSlideSpeed, 0f, slideDrag * deltaTime);

			Vector3 targetFlatVelocity = slideMoveDirection.normalized * currentSlideSpeed;
			rb.linearVelocity = new Vector3(targetFlatVelocity.x, rb.linearVelocity.y, targetFlatVelocity.z);

			if (playerController.OnSlope())
				rb.AddForce(-playerController.SlopeNormal * groundStickForce, ForceMode.Force);
			else
				rb.AddForce(Vector3.down * groundStickForce, ForceMode.Force);

			if (slideTimer <= 0f || currentSlideSpeed <= minSlideEndSpeed)
				ForceEndSlide();
		}

		public void ForceEndSlide()
		{
			if (!isSliding)
				return;

			isSliding = false;
			slideTimer = 0f;
			ungroundedTimer = 0f;
			currentSlideSpeed = 0f;

			if (playerController != null)
				playerController.SetSliding(false);

			RestoreHeight();
		}

		private void ApplySlideHeight()
		{
			if (slideBodyRoot == null)
				return;

			Vector3 newScale = originalBodyScale;
			newScale.y = originalBodyScale.y * slideHeightMultiplier;
			slideBodyRoot.localScale = newScale;
		}

		private void RestoreHeight()
		{
			if (slideBodyRoot == null)
				return;

			slideBodyRoot.localScale = originalBodyScale;
		}

		private static Vector3 Flatten(Vector3 value)
		{
			value.y = 0f;
			return value;
		}
	}
}