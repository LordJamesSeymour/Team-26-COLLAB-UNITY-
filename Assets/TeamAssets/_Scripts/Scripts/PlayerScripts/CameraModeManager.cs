using Unity.Cinemachine;
using UnityEngine;
using System.Collections;
using Group26.Player.Movement;
using Group26.Player.Inputs;
using Group26.Player.Utility;

namespace Group26.Player.Camera
{
	public enum CameraMode
	{
		//FirstPerson, 
		ThirdPerson
	}

	public class CameraModeManager : MonoBehaviour
	{
		private InputManager playerInput;
		private PlayerController playerController;
		private PlayerModeSwitcher playerModeSwitcher;
		private BallRollController ballRollController;
		private WallRunning wallRunning;
		private Rigidbody playerRigidbody;

		[SerializeField] private Transform m_playerTransform;
		[SerializeField] private Transform m_cameraPivot;

		[Header("Camera References & Settings")]
		// [SerializeField] public CinemachineCamera firstPersonVirtualCamera;
		[SerializeField] public CinemachineCamera thirdPersonVirtualCamera;
		[SerializeField] public CinemachineCamera leftWallRunningVirtualCamera;
		[SerializeField] public CinemachineCamera rightWallRunningVirtualCamera;

		private Transform cameraHolder;

		[SerializeField] private Vector2 firstPersonLookSensitivity = Vector2.one;
		[SerializeField] private Vector2 thirdPersonLookSensitivity = Vector2.one;

		public CameraMode currentCameraMode = CameraMode.ThirdPerson;
		private const int activeCameraPriority = 10;
		private const int inactiveCameraPriority = 1;

		[Header("First Person Camera References & Settings")]
		[SerializeField] private Vector2 m_firstPersonPitchLimits = new Vector2(-85f, 85f);
		[SerializeField] private Transform firstPersonYawRoot;
		[SerializeField] private Transform firstPersonPitchPivot;
		private float firstPersonYaw;
		private float firstPersonPitch;

		[Header("Third Person Camera References & Settings")]
		[SerializeField] private Vector2 m_thirdPersonPitchLimits = new Vector2(-60f, 80f);

		[Tooltip("Settings to adjust how quickly the character turns to match the camera direction in third person mode")]
		private float m_yaw;
		private float m_pitch;
		private float m_bodyYawVel;
		[SerializeField, Range(0f, 0.5f)] private float m_bodyTurnSmoothTime = 0.12f;
		[SerializeField, Range(0f, 180f)] private float m_turnWhenCameraYawOffsetExceeds = 25f;

		[Header("WallRun Yaw Reconciliation")]
		[SerializeField] private bool m_enableWallRunYawReconciliation = true;
		[SerializeField] private LayerMask m_wallRunProbeMask = ~0;
		[SerializeField, Range(0.1f, 3f)] private float m_wallRunProbeDistance = 1.25f;
		[SerializeField, Range(0.05f, 1f)] private float m_wallRunProbeRadius = 0.25f;
		[SerializeField, Range(0f, 3f)] private float m_wallRunProbeHeight = 1.0f;

		[Tooltip("How much smoothing is applied to the wall tangent itself.")]
		[SerializeField, Range(0.01f, 0.3f)] private float m_wallRunTangentYawSmoothTime = 0.12f;

		[Tooltip("How much smoothing is applied to the camera yaw once we have the target.")]
		[SerializeField, Range(0.01f, 0.3f)] private float m_wallRunCameraYawSmoothTime = 0.10f;

		[Tooltip("Maximum camera turn speed during wallrun assist.")]
		[SerializeField, Range(90f, 1440f)] private float m_wallRunMaxDegreesPerSecond = 480f;

		[Tooltip("How far ahead the camera can look into a curve.")]
		[SerializeField, Range(0f, 20f)] private float m_wallRunYawLeadAngle = 10f;

		[Tooltip("How strongly tangent change contributes to the lead angle.")]
		[SerializeField, Range(0f, 5f)] private float m_wallRunYawLeadMultiplier = 1.2f;

		[Tooltip("How strongly the camera tries to match the wallrun target yaw.")]
		[SerializeField, Range(0f, 1f)] private float m_wallRunAssistStrength = 0.95f;

		[Tooltip("Minimum yaw difference before entry assist starts.")]
		[SerializeField, Range(0f, 90f)] private float m_wallRunEntryAngleThreshold = 8f;

		private bool m_wasWallRunningLastFrame;
		private float m_smoothedWallTangentYaw;
		private float m_smoothedWallTangentYawVel;
		private float m_wallRunAssistVelocity;
		private float m_lastSmoothedWallTangentYaw;

		[Header("WallRun Body Follow")]
		[SerializeField] private bool m_enableWallRunBodyFollow = true;
		[SerializeField, Range(0f, 10f)] private float m_wallRunBodyYawOffsetThreshold = 0.25f;
		[SerializeField, Range(0.01f, 0.2f)] private float m_wallRunBodyTurnSmoothTime = 0.04f;
		[SerializeField, Range(90f, 2160f)] private float m_wallRunBodyMaxDegreesPerSecond = 1440f;
		private float m_wallRunBodyYawVel;

		[Header("Camera Shake Output")]
		[SerializeField] private bool m_enableExternalCameraShake = true;
		[SerializeField] private Transform m_cameraShakeTransform;
		private Vector3 m_cameraShakePositionOffset;
		private Vector3 m_cameraShakeRotationOffset;
		private Vector3 m_cameraShakeBaseLocalPosition;
		private Quaternion m_cameraShakeBaseLocalRotation;
		private bool m_cachedCameraShakeBase;

		[Header("FOV Settings")]
		[SerializeField] private float defaultFOV = 60f;
		[SerializeField] private float sprintFOV = 75f;
		[SerializeField, Range(0f, 0.5f)] private float FOVTransitionDuration = 0.25f;

		[Header("DashFOV Settings")]
		[SerializeField] private float dashFOV = 85f;
		[SerializeField, Range(0f, 0.25f)] private float burstTransitionDuration;

		[Header("GrappleBoost FOV Settings")]
		[SerializeField] private float grappleBoostFOV = 85f;
		[SerializeField, Range(0f, 5)] private float grappleBoostTransitionUpDuration;
		[SerializeField, Range(0f, 3f)] private float grappleBoostTransitionDownDuration;
		private bool isSprintingLastFrame = false;
		private bool isBursting;
		private bool isGrappleBoostActive;
		private float currentBaseFOV;
		private float currentDashFOV;
		private float currentGrappleBoostFOV;
		private Coroutine baseFOVCoroutine;
		private Coroutine dashFOVCoroutine;
		private Coroutine grappleBoostFOVCoroutine;
		private Coroutine ballMovementFOVCoroutine;

		private void Awake()
		{
			currentCameraMode = CameraMode.ThirdPerson;

			if (playerInput == null) playerInput = GetComponent<InputManager>();
			if (playerInput == null) Debug.LogError("No input manager found");

			if (playerController == null) playerController = GetComponent<PlayerController>();
			if (playerController == null) Debug.LogError("No player controller found");

			if (wallRunning == null) wallRunning = GetComponent<WallRunning>();
			if (wallRunning == null) Debug.LogError("No wall running script found");

			if (playerModeSwitcher == null) playerModeSwitcher = GetComponent<PlayerModeSwitcher>();
			if (playerModeSwitcher == null) Debug.LogWarning("No player mode switcher found");

			if (leftWallRunningVirtualCamera == null) Debug.LogError("Left wall running virtual camera not assigned");
			if (rightWallRunningVirtualCamera == null) Debug.LogError("Right wall running virtual camera not assigned");

			if (ballRollController == null) ballRollController = GetComponent<BallRollController>();
			if (ballRollController == null) Debug.LogWarning("No ball roll controller found");

			if (playerRigidbody == null) playerRigidbody = GetComponent<Rigidbody>();
			if (playerRigidbody == null) Debug.LogWarning("No rigidbody found for wallrun camera reconciliation");

			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;

			currentBaseFOV = defaultFOV;
			currentDashFOV = defaultFOV;
			currentGrappleBoostFOV = defaultFOV;
			SetCameraFOV(defaultFOV);

			if (m_cameraPivot != null)
			{
				Vector3 pivotEuler = m_cameraPivot.rotation.eulerAngles;
				m_yaw = pivotEuler.y;
				m_pitch = NormalizePitch(pivotEuler.x);
			}

			ResolveCameraShakeTransform();
			CacheCameraShakeBase();
		}

		private void Start()
		{
			ApplyCameraShakeOffsets();
		}

		private void OnEnable()
		{
			if (playerInput != null)
				playerInput.OnDashPressed += BurstFOVIncrease;
		}

		private void OnDisable()
		{
			if (playerInput != null)
				playerInput.OnDashPressed -= BurstFOVIncrease;
		}

		private void OnCollisionEnter(Collision collision)
		{
			EndGrappleBoostFOV();
		}

		private void Update()
		{
			if (currentCameraMode == CameraMode.ThirdPerson)
			{
				ApplyThirdPersonLook(playerInput?.LookInput ?? Vector2.zero);

				cameraHolder = thirdPersonVirtualCamera != null ? thirdPersonVirtualCamera.transform : null;

				if (leftWallRunningVirtualCamera == null || rightWallRunningVirtualCamera == null)
				{
					Debug.LogWarning("Wall running virtual cameras not assigned");
					return;
				}

				if (playerController != null && wallRunning != null && playerController.m_bIsWallRunning && wallRunning.wallLeft)
				{
					leftWallRunningVirtualCamera.Priority = activeCameraPriority;
					rightWallRunningVirtualCamera.Priority = inactiveCameraPriority;
					thirdPersonVirtualCamera.Priority = inactiveCameraPriority;
				}
				else if (playerController != null && wallRunning != null && playerController.m_bIsWallRunning && wallRunning.wallRight)
				{
					leftWallRunningVirtualCamera.Priority = inactiveCameraPriority;
					rightWallRunningVirtualCamera.Priority = activeCameraPriority;
					thirdPersonVirtualCamera.Priority = inactiveCameraPriority;
				}
				else
				{
					leftWallRunningVirtualCamera.Priority = inactiveCameraPriority;
					rightWallRunningVirtualCamera.Priority = inactiveCameraPriority;
					thirdPersonVirtualCamera.Priority = activeCameraPriority;
				}

				UpdateBodyFacingDirection();
			}

			if (playerModeSwitcher != null && playerModeSwitcher.currentMode == PlayerMode.BallMode)
			{
				if (ballMovementFOVCoroutine != null)
				{
					StopCoroutine(ballMovementFOVCoroutine);
				}

				ballMovementFOVCoroutine = StartCoroutine(DoBallMovementFOV());
			}

			HandleRunFOV();
		}

		private void LateUpdate()
		{
			ApplyCameraShakeOffsets();
		}

		private void ResolveCameraShakeTransform()
		{
			if (m_cameraShakeTransform != null)
				return;

			if (thirdPersonVirtualCamera != null && thirdPersonVirtualCamera.transform.parent != null)
			{
				m_cameraShakeTransform = thirdPersonVirtualCamera.transform.parent;
				return;
			}

			m_cameraShakeTransform = m_cameraPivot;
		}

		private void CacheCameraShakeBase()
		{
			ResolveCameraShakeTransform();

			if (m_cachedCameraShakeBase || m_cameraShakeTransform == null)
				return;

			m_cameraShakeBaseLocalPosition = m_cameraShakeTransform.localPosition;
			m_cameraShakeBaseLocalRotation = m_cameraShakeTransform.localRotation;
			m_cachedCameraShakeBase = true;
		}

		public void SetCameraShakeOffsets(Vector3 localPositionOffset, Vector3 localRotationEulerOffset)
		{
			if (!m_enableExternalCameraShake)
				return;

			m_cameraShakePositionOffset = localPositionOffset;
			m_cameraShakeRotationOffset = localRotationEulerOffset;
		}

		public void ClearCameraShakeOffsets()
		{
			m_cameraShakePositionOffset = Vector3.zero;
			m_cameraShakeRotationOffset = Vector3.zero;
		}

		private void ApplyCameraShakeOffsets()
		{
			CacheCameraShakeBase();

			if (m_cameraShakeTransform == null)
				return;

			m_cameraShakeTransform.localPosition = m_cameraShakeBaseLocalPosition + m_cameraShakePositionOffset;
			m_cameraShakeTransform.localRotation = m_cameraShakeBaseLocalRotation * Quaternion.Euler(m_cameraShakeRotationOffset);
		}

		private IEnumerator DoBallMovementFOV()
		{
			if (playerModeSwitcher == null || ballRollController == null)
			{
				yield break;
			}

			while (playerModeSwitcher != null && playerModeSwitcher.currentMode == PlayerMode.BallMode)
			{
				Vector3 flatVelocity = new Vector3(ballRollController.m_rigidBody.linearVelocity.x, 0f, ballRollController.m_rigidBody.linearVelocity.z);

				float speed = flatVelocity.magnitude;
				float targetFOV = Mathf.Lerp(defaultFOV, sprintFOV, speed / ballRollController.m_maxSpeed);

				float smoothedFOV = Mathf.Lerp(GetCurrentCameraFOV(), targetFOV, Time.deltaTime * 5f);
				SetCameraFOV(smoothedFOV);

				yield return null;
			}

			while (Mathf.Abs(GetCurrentCameraFOV() - defaultFOV) > 0.1f)
			{
				float smoothedFOV = Mathf.Lerp(GetCurrentCameraFOV(), defaultFOV, Time.deltaTime * 5f);
				SetCameraFOV(smoothedFOV);
				yield return null;
			}

			SetCameraFOV(defaultFOV);
		}

		private void ApplyThirdPersonLook(Vector2 lookInput)
		{
			m_yaw += lookInput.x * thirdPersonLookSensitivity.x * Time.deltaTime;
			m_pitch -= lookInput.y * thirdPersonLookSensitivity.y * Time.deltaTime;
			m_pitch = Mathf.Clamp(m_pitch, m_thirdPersonPitchLimits.x, m_thirdPersonPitchLimits.y);

			UpdateWallRunYawReconciliation();

			m_cameraPivot.rotation = Quaternion.Euler(m_pitch, m_yaw, 0f);
		}

		private void UpdateWallRunYawReconciliation()
		{
			bool isWallRunning =
				currentCameraMode == CameraMode.ThirdPerson &&
				m_enableWallRunYawReconciliation &&
				playerController != null &&
				playerController.m_bIsWallRunning &&
				wallRunning != null &&
				(wallRunning.wallLeft || wallRunning.wallRight) &&
				m_playerTransform != null;

			if (!isWallRunning)
			{
				ResetWallRunYawReconciliation();
				return;
			}

			if (!TryGetWallRunTangent(out Vector3 wallTangent, out float tangentYaw))
			{
				return;
			}

			if (!m_wasWallRunningLastFrame)
			{
				m_wasWallRunningLastFrame = true;
				m_smoothedWallTangentYaw = tangentYaw;
				m_lastSmoothedWallTangentYaw = tangentYaw;

				float entryDelta = Mathf.Abs(Mathf.DeltaAngle(m_yaw, tangentYaw));
				if (entryDelta >= m_wallRunEntryAngleThreshold)
				{
					float blendedEntryTarget = Mathf.LerpAngle(m_yaw, tangentYaw, m_wallRunAssistStrength);

					m_yaw = Mathf.SmoothDampAngle(
						m_yaw,
						blendedEntryTarget,
						ref m_wallRunAssistVelocity,
						m_wallRunCameraYawSmoothTime,
						m_wallRunMaxDegreesPerSecond,
						Time.deltaTime);
				}

				return;
			}

			m_smoothedWallTangentYaw = Mathf.SmoothDampAngle(
				m_smoothedWallTangentYaw,
				tangentYaw,
				ref m_smoothedWallTangentYawVel,
				m_wallRunTangentYawSmoothTime,
				m_wallRunMaxDegreesPerSecond,
				Time.deltaTime);

			float smoothedTangentDelta = Mathf.DeltaAngle(m_lastSmoothedWallTangentYaw, m_smoothedWallTangentYaw);

			float leadAngle = Mathf.Clamp(
				smoothedTangentDelta * m_wallRunYawLeadMultiplier,
				-m_wallRunYawLeadAngle,
				m_wallRunYawLeadAngle);

			float desiredYaw = m_smoothedWallTangentYaw + leadAngle;
			float blendedTargetYaw = Mathf.LerpAngle(m_yaw, desiredYaw, m_wallRunAssistStrength);

			m_yaw = Mathf.SmoothDampAngle(
				m_yaw,
				blendedTargetYaw,
				ref m_wallRunAssistVelocity,
				m_wallRunCameraYawSmoothTime,
				m_wallRunMaxDegreesPerSecond,
				Time.deltaTime);

			m_lastSmoothedWallTangentYaw = m_smoothedWallTangentYaw;
		}

		private bool TryGetWallRunTangent(out Vector3 wallTangent, out float tangentYaw)
		{
			wallTangent = Vector3.zero;
			tangentYaw = 0f;

			if (!TryProbeWallNormal(out Vector3 wallNormal))
			{
				return false;
			}

			wallTangent = Vector3.Cross(wallNormal.normalized, Vector3.up).normalized;
			if (wallTangent.sqrMagnitude < 0.0001f)
			{
				return false;
			}

			Vector3 referenceForward = GetWallRunReferenceForward();
			if (referenceForward.sqrMagnitude < 0.0001f)
			{
				referenceForward = Vector3.ProjectOnPlane(m_playerTransform.forward, Vector3.up).normalized;
			}

			if (referenceForward.sqrMagnitude < 0.0001f)
			{
				referenceForward = Vector3.forward;
			}

			if (Vector3.Dot(wallTangent, referenceForward) < 0f)
			{
				wallTangent = -wallTangent;
			}

			tangentYaw = Quaternion.LookRotation(wallTangent, Vector3.up).eulerAngles.y;
			return true;
		}

		private bool TryProbeWallNormal(out Vector3 wallNormal)
		{
			wallNormal = Vector3.zero;

			if (m_playerTransform == null || wallRunning == null)
			{
				return false;
			}

			Vector3 origin = m_playerTransform.position + Vector3.up * m_wallRunProbeHeight;
			Vector3 sideDirection;

			if (wallRunning.wallLeft)
			{
				sideDirection = -m_playerTransform.right;
			}
			else if (wallRunning.wallRight)
			{
				sideDirection = m_playerTransform.right;
			}
			else
			{
				return false;
			}

			if (Physics.SphereCast(
				origin,
				m_wallRunProbeRadius,
				sideDirection,
				out RaycastHit sphereHit,
				m_wallRunProbeDistance,
				m_wallRunProbeMask,
				QueryTriggerInteraction.Ignore))
			{
				wallNormal = sphereHit.normal;
				return true;
			}

			if (Physics.Raycast(
				origin,
				sideDirection,
				out RaycastHit rayHit,
				m_wallRunProbeDistance,
				m_wallRunProbeMask,
				QueryTriggerInteraction.Ignore))
			{
				wallNormal = rayHit.normal;
				return true;
			}

			return false;
		}

		private Vector3 GetWallRunReferenceForward()
		{
			if (playerRigidbody != null)
			{
				Vector3 flatVelocity = Vector3.ProjectOnPlane(playerRigidbody.linearVelocity, Vector3.up);
				if (flatVelocity.sqrMagnitude > 0.04f)
				{
					return flatVelocity.normalized;
				}
			}

			if (playerController != null)
			{
				Vector3 inputDirection = Vector3.ProjectOnPlane(playerController.GetDirection(), Vector3.up);
				if (inputDirection.sqrMagnitude > 0.04f)
				{
					return inputDirection.normalized;
				}
			}

			if (m_playerTransform != null)
			{
				Vector3 flatForward = Vector3.ProjectOnPlane(m_playerTransform.forward, Vector3.up);
				if (flatForward.sqrMagnitude > 0.0001f)
				{
					return flatForward.normalized;
				}
			}

			return Vector3.forward;
		}

		private void ResetWallRunYawReconciliation()
		{
			m_wasWallRunningLastFrame = false;
			m_smoothedWallTangentYaw = 0f;
			m_smoothedWallTangentYawVel = 0f;
			m_wallRunAssistVelocity = 0f;
			m_lastSmoothedWallTangentYaw = 0f;
		}

		private float NormalizePitch(float angle)
		{
			while (angle > 180f) angle -= 360f;
			while (angle < -180f) angle += 360f;
			return angle;
		}

		private void UpdateBodyFacingDirection()
		{
			if (currentCameraMode != CameraMode.ThirdPerson) return;
			if (m_playerTransform == null || m_cameraPivot == null) return;

			float targetYaw = m_cameraPivot.eulerAngles.y;
			float currentYaw = m_playerTransform.eulerAngles.y;
			float yawDifference = Mathf.DeltaAngle(currentYaw, targetYaw);

			if (playerController != null && playerController.m_bIsWallRunning)
			{
				m_bodyYawVel = 0f;

				if (!m_enableWallRunBodyFollow) return;
				if (Mathf.Abs(yawDifference) < m_wallRunBodyYawOffsetThreshold) return;

				float newYaw = Mathf.SmoothDampAngle(
					currentYaw,
					targetYaw,
					ref m_wallRunBodyYawVel,
					m_wallRunBodyTurnSmoothTime,
					m_wallRunBodyMaxDegreesPerSecond,
					Time.deltaTime);

				m_playerTransform.rotation = Quaternion.Euler(0f, newYaw, 0f);
				return;
			}

			m_wallRunBodyYawVel = 0f;

			if (Mathf.Abs(yawDifference) < m_turnWhenCameraYawOffsetExceeds) return;

			float groundedYaw = Mathf.SmoothDampAngle(
				currentYaw,
				targetYaw,
				ref m_bodyYawVel,
				m_bodyTurnSmoothTime);

			m_playerTransform.rotation = Quaternion.Euler(0f, groundedYaw, 0f);
		}

		public void GrappleBoostFOV()
		{
			if (grappleBoostFOVCoroutine != null)
			{
				StopCoroutine(grappleBoostFOVCoroutine);
			}

			isGrappleBoostActive = true;
			currentGrappleBoostFOV = GetResolvedFOV();
			grappleBoostFOVCoroutine = StartCoroutine(DoGrappleBoostUpFOV());
		}

		public void EndGrappleBoostFOV()
		{
			if (!isGrappleBoostActive && grappleBoostFOVCoroutine == null)
			{
				return;
			}

			if (grappleBoostFOVCoroutine != null)
			{
				StopCoroutine(grappleBoostFOVCoroutine);
			}

			currentGrappleBoostFOV = GetResolvedFOV();
			grappleBoostFOVCoroutine = StartCoroutine(DoGrappleBoostDownFOV());
		}

		private IEnumerator DoGrappleBoostUpFOV()
		{
			float initialFOV = currentGrappleBoostFOV;
			float elapsedTime = 0f;

			if (grappleBoostTransitionUpDuration <= 0f)
			{
				currentGrappleBoostFOV = grappleBoostFOV;
				ApplyResolvedFOV();
				grappleBoostFOVCoroutine = null;
				yield break;
			}

			while (elapsedTime < grappleBoostTransitionUpDuration)
			{
				elapsedTime += Time.deltaTime;
				float progress = Mathf.Clamp01(elapsedTime / grappleBoostTransitionUpDuration);
				float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

				currentGrappleBoostFOV = Mathf.Lerp(initialFOV, grappleBoostFOV, smoothProgress);
				ApplyResolvedFOV();
				yield return null;
			}

			currentGrappleBoostFOV = grappleBoostFOV;
			ApplyResolvedFOV();
			grappleBoostFOVCoroutine = null;
		}

		private IEnumerator DoGrappleBoostDownFOV()
		{
			float initialFOV = currentGrappleBoostFOV;
			float targetFOV = GetResolvedFOVWithoutGrapple();
			float elapsedTime = 0f;

			if (grappleBoostTransitionDownDuration <= 0f)
			{
				currentGrappleBoostFOV = targetFOV;
				isGrappleBoostActive = false;
				ApplyResolvedFOV();
				grappleBoostFOVCoroutine = null;
				yield break;
			}

			while (elapsedTime < grappleBoostTransitionDownDuration)
			{
				elapsedTime += Time.deltaTime;
				float progress = Mathf.Clamp01(elapsedTime / grappleBoostTransitionDownDuration);
				float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

				currentGrappleBoostFOV = Mathf.Lerp(initialFOV, targetFOV, smoothProgress);
				ApplyResolvedFOV();
				yield return null;
			}

			currentGrappleBoostFOV = targetFOV;
			isGrappleBoostActive = false;
			ApplyResolvedFOV();
			grappleBoostFOVCoroutine = null;
		}

		private void BurstFOVIncrease()
		{
			if (playerController == null || !playerController.m_bDashing) return;
			if (isBursting) return;

			isBursting = true;

			if (dashFOVCoroutine != null)
			{
				StopCoroutine(dashFOVCoroutine);
			}

			currentDashFOV = GetResolvedFOV();
			dashFOVCoroutine = StartCoroutine(DoBurstFOV());
		}

		private IEnumerator DoBurstFOV()
		{
			float originalFOV = currentDashFOV;
			float elapsedTime = 0f;

			if (burstTransitionDuration <= 0f)
			{
				currentDashFOV = GetResolvedFOVWithoutDash();
				isBursting = false;
				dashFOVCoroutine = null;
				ApplyResolvedFOV();
				yield break;
			}

			while (elapsedTime < burstTransitionDuration)
			{
				elapsedTime += Time.deltaTime;
				float progress = Mathf.Clamp01(elapsedTime / burstTransitionDuration);
				float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
				currentDashFOV = Mathf.Lerp(originalFOV, dashFOV, smoothProgress);

				ApplyResolvedFOV();
				yield return null;
			}

			currentDashFOV = dashFOV;
			ApplyResolvedFOV();

			elapsedTime = 0f;
			float returnTargetFOV = GetResolvedFOVWithoutDash();
			while (elapsedTime < burstTransitionDuration)
			{
				elapsedTime += Time.deltaTime;
				float progress = Mathf.Clamp01(elapsedTime / burstTransitionDuration);
				float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
				currentDashFOV = Mathf.Lerp(dashFOV, returnTargetFOV, smoothProgress);

				ApplyResolvedFOV();
				yield return null;
			}

			currentDashFOV = returnTargetFOV;
			ApplyResolvedFOV();

			dashFOVCoroutine = null;
			isBursting = false;
		}

		private void HandleRunFOV()
		{
			if (playerController == null) return;
			if (playerModeSwitcher != null && playerModeSwitcher.currentMode == PlayerMode.BallMode) return;

			bool isCurrentlyRunning =
				(playerInput != null && playerInput.MoveInput.magnitude > 0.1f) ||
				playerController.m_bIsWallRunning ||
				playerController.m_bActiveGrapple;

			if (isCurrentlyRunning != isSprintingLastFrame)
			{
				float targetFOV = isCurrentlyRunning ? sprintFOV : defaultFOV;
				StartFOVTransition(targetFOV);
				isSprintingLastFrame = isCurrentlyRunning;
			}
		}

		private void StartFOVTransition(float targetFOV)
		{
			if (baseFOVCoroutine != null)
			{
				StopCoroutine(baseFOVCoroutine);
			}

			baseFOVCoroutine = StartCoroutine(TransitionFOV(targetFOV));
		}

		private IEnumerator TransitionFOV(float targetFOV)
		{
			float startFOV = currentBaseFOV;
			float elapsedTime = 0f;

			if (FOVTransitionDuration <= 0f)
			{
				currentBaseFOV = targetFOV;
				ApplyResolvedFOV();
				baseFOVCoroutine = null;
				yield break;
			}

			while (elapsedTime < FOVTransitionDuration)
			{
				elapsedTime += Time.deltaTime;
				float progress = Mathf.Clamp01(elapsedTime / FOVTransitionDuration);
				float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
				currentBaseFOV = Mathf.Lerp(startFOV, targetFOV, smoothProgress);

				ApplyResolvedFOV();

				yield return null;
			}

			currentBaseFOV = targetFOV;
			ApplyResolvedFOV();
			baseFOVCoroutine = null;
		}

		private float GetResolvedFOV()
		{
			float resolvedFOV = currentBaseFOV;

			if (isBursting)
			{
				resolvedFOV = Mathf.Max(resolvedFOV, currentDashFOV);
			}

			if (isGrappleBoostActive)
			{
				resolvedFOV = Mathf.Max(resolvedFOV, currentGrappleBoostFOV);
			}

			return resolvedFOV;
		}

		private float GetResolvedFOVWithoutDash()
		{
			float resolvedFOV = currentBaseFOV;

			if (isGrappleBoostActive)
			{
				resolvedFOV = Mathf.Max(resolvedFOV, currentGrappleBoostFOV);
			}

			return resolvedFOV;
		}

		private float GetResolvedFOVWithoutGrapple()
		{
			float resolvedFOV = currentBaseFOV;

			if (isBursting)
			{
				resolvedFOV = Mathf.Max(resolvedFOV, currentDashFOV);
			}

			return resolvedFOV;
		}

		private void ApplyResolvedFOV()
		{
			SetCameraFOV(GetResolvedFOV());
		}

		private void SetCameraFOV(float fov)
		{
			if (thirdPersonVirtualCamera != null && thirdPersonVirtualCamera.Lens.FieldOfView != fov)
			{
				var lens = thirdPersonVirtualCamera.Lens;
				lens.FieldOfView = fov;
				thirdPersonVirtualCamera.Lens = lens;
			}

			if (leftWallRunningVirtualCamera != null && leftWallRunningVirtualCamera.Lens.FieldOfView != fov)
			{
				var lens = leftWallRunningVirtualCamera.Lens;
				lens.FieldOfView = fov;
				leftWallRunningVirtualCamera.Lens = lens;
			}

			if (rightWallRunningVirtualCamera != null && rightWallRunningVirtualCamera.Lens.FieldOfView != fov)
			{
				var lens = rightWallRunningVirtualCamera.Lens;
				lens.FieldOfView = fov;
				rightWallRunningVirtualCamera.Lens = lens;
			}
		}

		private float GetCurrentCameraFOV()
		{
			if (playerController != null && playerController.m_bIsWallRunning)
			{
				if (wallRunning != null && wallRunning.wallLeft && leftWallRunningVirtualCamera != null)
				{
					return leftWallRunningVirtualCamera.Lens.FieldOfView;
				}

				if (wallRunning != null && wallRunning.wallRight && rightWallRunningVirtualCamera != null)
				{
					return rightWallRunningVirtualCamera.Lens.FieldOfView;
				}
			}

			if (currentCameraMode == CameraMode.ThirdPerson && thirdPersonVirtualCamera != null)
			{
				return thirdPersonVirtualCamera.Lens.FieldOfView;
			}

			return defaultFOV;
		}
	}
}