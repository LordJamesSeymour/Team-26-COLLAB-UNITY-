using Group26.Player.Camera;
using UnityEngine;

public class Grappling : MonoBehaviour
{
	[Header("References")]
	private InputManager2 InputManager;
	private CameraModeManager cameraModeManager;
	[SerializeField] private Transform firstPersonCam; 
	[SerializeField] private Transform thirdPersonCam;
	private Transform Cam;
	[SerializeField] private Transform gunTip;
	[SerializeField] private LayerMask m_grappableLayer;
	[SerializeField] private LineRenderer lineRenderer;
	private PlayerController PlayerController;
	private Vector3 grapplePoint;

	[Header("Grappling")]
	[SerializeField] private float maxGrappleDistance;
	[SerializeField] private float grappleDelayTime;
	[SerializeField] private float overshootYAxis;

	[Header("Miss Visual")]
    [Tooltip("How long to show the line when the grapple misses.")]
    [SerializeField, Range(0f, 1f)] private float missLineTime = 0.08f;

	[Header("Cooldown")]
	[SerializeField] private float grappleCooldown;
	private float grappleCooldownTimer;
	private bool m_bGrappling;
	private int _grappleToken;

	private void Awake()
	{
		if (InputManager == null) InputManager = GetComponent<InputManager2>();
		if (PlayerController == null) PlayerController = GetComponent<PlayerController>();
		if (cameraModeManager == null) cameraModeManager = GetComponent<CameraModeManager>();

		Cam = cameraModeManager.currentCameraMode == CameraMode.FirstPerson ? firstPersonCam : thirdPersonCam;
	}

    void OnEnable()
    {
        InputManager.OnGrapplePressed += StartGrapple;
		InputManager.OnCameraSwitchPressed += () => lineRenderer.enabled = false; // Hide line when switching camera modes, to avoid weird line positions due to camera changes during grapple
    }

    void OnDisable()
    {
        InputManager.OnGrapplePressed -= StartGrapple;
		InputManager.OnCameraSwitchPressed -= () => lineRenderer.enabled = false;
		CancelInvoke();
    }

    private void LateUpdate()
	{
		if (m_bGrappling)
			lineRenderer.SetPosition(0, gunTip.position);
	}

	private void FixedUpdate()
	{
		if (grappleCooldownTimer > 0)
			grappleCooldownTimer -= Time.deltaTime;
	}

	void StartGrapple()
	{
		Cam = cameraModeManager.currentCameraMode == CameraMode.FirstPerson ? firstPersonCam : thirdPersonCam;

		if (grappleCooldownTimer > 0) return;
		if(m_bGrappling) return;

		m_bGrappling = true;
		_grappleToken++;

		CancelInvoke();

		PlayerController.m_bFreeze = true;

        bool didHit = Physics.Raycast(Cam.position, Cam.forward, out RaycastHit hit, maxGrappleDistance, m_grappableLayer);
        grapplePoint = didHit ? hit.point : (Cam.position + Cam.forward * maxGrappleDistance);

		lineRenderer.enabled = true;
		lineRenderer.SetPosition(0, gunTip.position);
		lineRenderer.SetPosition(1, grapplePoint);

		int tokenAtSchedule = _grappleToken;

		if(grappleDelayTime <= 0)
		{
			if (didHit) Invoke(nameof(ExecuteGrapple_InvokeWrapper), grappleDelayTime);
			else // Display line for a moment, then stop grapple
			{
				PlayerController.m_bFreeze = false;

            	float t = Mathf.Max(missLineTime, 0.01f); // Guarantees a frame at least for visual feedbcak

            	// Stop after a short visual delay, not grappleDelayTime
            	Invoke(nameof(StopGrapple_InvokeWrapper), t);
			}
		}
	}

	private void ExecuteGrapple_InvokeWrapper() => ExecuteGrapple(_grappleToken);
	private void StopGrapple_InvokeWrapper() => StopGrapple(_grappleToken);

	void ExecuteGrapple(int token)
	{
		if(token != _grappleToken) return;

		PlayerController.m_bFreeze = false;

		Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

		float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
		float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

		if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;

		PlayerController.JumpToPosition(grapplePoint, highestPointOnArc);

		Invoke(nameof(StopGrapple_InvokeWrapper), 1f);
	}

	public void ForceStopGrapple()
	{
		StopGrapple(_grappleToken);
	}

	private void StopGrapple(int token)
	{
		if (token != _grappleToken) return;

		PlayerController.m_bFreeze = false;
		m_bGrappling = false;
		grappleCooldownTimer = grappleCooldown;
		lineRenderer.enabled = false;
	}
}