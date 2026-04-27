using UnityEngine;
using UnityEngine.UI;

public class SpeedLinesShaderController : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Image targetImage;
	[SerializeField] private Rigidbody playerRb;

	[Header("Speed Thresholds")]
	[SerializeField] private bool ignoreVerticalVelocity = true;
	[SerializeField] private float minSpeedToShow = 1.5f;
	[SerializeField] private float maxSpeedForFullEffect = 8f;

	[Header("Stop Behavior")]
	[SerializeField] private float stopSpeedThreshold = 0.05f;

	[Header("Response")]
	[SerializeField] private float appearSharpness = 1.0f;
	[SerializeField] private float speedRiseSmoothing = 20f;
	[SerializeField] private float speedFallSmoothing = 6f;
	[SerializeField] private float effectRiseSmoothing = 18f;
	[SerializeField] private float effectFallSmoothing = 5f;

	[Header("Shader Values")]
	[SerializeField] private float minDensity = 0f;
	[SerializeField] private float maxDensity = 34f;

	[SerializeField] private float minBrightness = 0f;
	[SerializeField] private float maxBrightness = 3.2f;

	[SerializeField] private float minThickness = 0f;
	[SerializeField] private float maxThickness = 0.0045f;

	[SerializeField] private float minFlowSpeed = 0f;
	[SerializeField] private float maxFlowSpeed = 6.0f;

	[Header("Debug")]
	[SerializeField] private bool debugLogSpeed = false;

	private Material runtimeMaterial;
	private float smoothedSpeed;
	private float currentAmount;
	private float flowPhase;

	private static readonly int SpeedAmountID = Shader.PropertyToID("_SpeedAmount");
	private static readonly int LineDensityID = Shader.PropertyToID("_LineDensity");
	private static readonly int BrightnessID = Shader.PropertyToID("_Brightness");
	private static readonly int ThicknessID = Shader.PropertyToID("_Thickness");
	private static readonly int FlowPhaseID = Shader.PropertyToID("_FlowPhase");

	private void Awake()
	{
		if (targetImage == null)
			targetImage = GetComponent<Image>();

		if (targetImage == null)
		{
			Debug.LogError("SpeedLinesShaderController: No Image found.");
			enabled = false;
			return;
		}

		if (targetImage.material == null)
		{
			Debug.LogError("SpeedLinesShaderController: No material assigned to the Image.");
			enabled = false;
			return;
		}

		runtimeMaterial = new Material(targetImage.material);
		targetImage.material = runtimeMaterial;
	}

	private void Update()
	{
		float rawSpeed = GetCurrentSpeed();

		if (debugLogSpeed)
			Debug.Log($"SpeedLines raw: {rawSpeed:F3} | smoothed: {smoothedSpeed:F3}");

		// If the player is fully stopped, kill the effect immediately.
		if (rawSpeed <= stopSpeedThreshold)
		{
			smoothedSpeed = 0f;
			currentAmount = 0f;

			runtimeMaterial.SetFloat(SpeedAmountID, 0f);
			runtimeMaterial.SetFloat(LineDensityID, 0f);
			runtimeMaterial.SetFloat(BrightnessID, 0f);
			runtimeMaterial.SetFloat(ThicknessID, 0f);

			Color stoppedColor = targetImage.color;
			stoppedColor.a = 0f;
			targetImage.color = stoppedColor;
			return;
		}

		float speedSmooth = rawSpeed > smoothedSpeed ? speedRiseSmoothing : speedFallSmoothing;
		smoothedSpeed = Mathf.Lerp(smoothedSpeed, rawSpeed, Time.deltaTime * speedSmooth);

		float rawAmount = Mathf.InverseLerp(minSpeedToShow, maxSpeedForFullEffect, smoothedSpeed);
		float targetAmount = Mathf.Pow(rawAmount, appearSharpness);

		float effectSmooth = targetAmount > currentAmount ? effectRiseSmoothing : effectFallSmoothing;
		currentAmount = Mathf.Lerp(currentAmount, targetAmount, Time.deltaTime * effectSmooth);

		float density = Mathf.Lerp(minDensity, maxDensity, currentAmount);
		float brightness = Mathf.Lerp(minBrightness, maxBrightness, currentAmount);
		float thickness = Mathf.Lerp(minThickness, maxThickness, currentAmount);
		float flowSpeed = Mathf.Lerp(minFlowSpeed, maxFlowSpeed, currentAmount);

		// Monotonic phase: only moves forward, never backward when speed changes.
		flowPhase += flowSpeed * Time.deltaTime;

		runtimeMaterial.SetFloat(SpeedAmountID, currentAmount);
		runtimeMaterial.SetFloat(LineDensityID, density);
		runtimeMaterial.SetFloat(BrightnessID, brightness);
		runtimeMaterial.SetFloat(ThicknessID, thickness);
		runtimeMaterial.SetFloat(FlowPhaseID, flowPhase);

		Color c = targetImage.color;
		c.a = currentAmount > 0.01f ? 1f : 0f;
		targetImage.color = c;
	}

	private float GetCurrentSpeed()
	{
		if (playerRb == null)
			return 0f;

		Vector3 velocity = playerRb.linearVelocity;

		if (ignoreVerticalVelocity)
			velocity.y = 0f;

		return velocity.magnitude;
	}
}