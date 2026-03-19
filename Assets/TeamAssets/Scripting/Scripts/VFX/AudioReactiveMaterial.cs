using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class AudioReactiveMaterial : MonoBehaviour
{
	[Header("Audio")]
	[SerializeField] private AudioSource audioSource;
	[SerializeField] private FFTWindow fftWindow = FFTWindow.BlackmanHarris;
	[SerializeField, Range(64, 8192)] private int spectrumSampleCount = 512;

	[Header("Bands")]
	[SerializeField, Range(8, 256)] private int bandCount = 64;
	[SerializeField] private float sensitivity = 80f;
	[SerializeField] private float riseSpeed = 20f;
	[SerializeField] private float fallSpeed = 8f;

	[Header("Shader Property Names")]
	[SerializeField] private string spectrumTextureProperty = "_SpectrumTex";
	[SerializeField] private string amplitudeProperty = "_Amplitude";
	[SerializeField] private string bassProperty = "_Bass";
	[SerializeField] private string midProperty = "_Mid";
	[SerializeField] private string trebleProperty = "_Treble";

	private Renderer targetRenderer;
	private MaterialPropertyBlock propertyBlock;

	private float[] spectrumSamples;
	private float[] rawBands;
	private float[] smoothBands;

	private Texture2D spectrumTexture;
	private Color[] spectrumPixels;

	private void Awake()
	{
		targetRenderer = GetComponent<Renderer>();
		propertyBlock = new MaterialPropertyBlock();

		spectrumSamples = new float[spectrumSampleCount];
		rawBands = new float[bandCount];
		smoothBands = new float[bandCount];
		spectrumPixels = new Color[bandCount];

		spectrumTexture = new Texture2D(bandCount, 1, TextureFormat.RFloat, false, true);
		spectrumTexture.wrapMode = TextureWrapMode.Clamp;
		spectrumTexture.filterMode = FilterMode.Bilinear;
	}

	private void Update()
	{
		if (audioSource == null)
			return;

		if (!audioSource.isPlaying)
			return;

		audioSource.GetSpectrumData(spectrumSamples, 0, fftWindow);

		BuildBands();
		SmoothBands();
		UpdateSpectrumTexture();
		PushToMaterial();
	}

	private void BuildBands()
	{
		for (int i = 0; i < bandCount; i++)
		{
			float start01 = Mathf.Pow((float)i / bandCount, 2f);
			float end01 = Mathf.Pow((float)(i + 1) / bandCount, 2f);

			int startIndex = Mathf.FloorToInt(start01 * (spectrumSampleCount - 1));
			int endIndex = Mathf.FloorToInt(end01 * (spectrumSampleCount - 1));
			endIndex = Mathf.Max(endIndex, startIndex + 1);

			float maxValue = 0f;

			for (int s = startIndex; s < endIndex; s++)
			{
				if (spectrumSamples[s] > maxValue)
					maxValue = spectrumSamples[s];
			}

			rawBands[i] = Mathf.Clamp01(maxValue * sensitivity);
		}
	}

	private void SmoothBands()
	{
		float dt = Time.deltaTime;

		for (int i = 0; i < bandCount; i++)
		{
			float speed = rawBands[i] > smoothBands[i] ? riseSpeed : fallSpeed;
			float t = 1f - Mathf.Exp(-speed * dt);
			smoothBands[i] = Mathf.Lerp(smoothBands[i], rawBands[i], t);
		}
	}

	private void UpdateSpectrumTexture()
	{
		for (int i = 0; i < bandCount; i++)
		{
			float v = smoothBands[i];
			spectrumPixels[i] = new Color(v, 0f, 0f, 1f);
		}

		spectrumTexture.SetPixels(spectrumPixels);
		spectrumTexture.Apply(false, false);
	}

	private void PushToMaterial()
	{
		float amplitude = AverageRange(0, bandCount);
		float bass = AverageRange(0, Mathf.Max(1, bandCount / 8));
		float mid = AverageRange(bandCount / 8, Mathf.Max(bandCount / 2, bandCount / 8 + 1));
		float treble = AverageRange(bandCount / 2, bandCount);

		targetRenderer.GetPropertyBlock(propertyBlock);
		propertyBlock.SetTexture(spectrumTextureProperty, spectrumTexture);
		propertyBlock.SetFloat(amplitudeProperty, amplitude);
		propertyBlock.SetFloat(bassProperty, bass);
		propertyBlock.SetFloat(midProperty, mid);
		propertyBlock.SetFloat(trebleProperty, treble);
		targetRenderer.SetPropertyBlock(propertyBlock);
	}

	private float AverageRange(int start, int end)
	{
		start = Mathf.Clamp(start, 0, smoothBands.Length - 1);
		end = Mathf.Clamp(end, start + 1, smoothBands.Length);

		float sum = 0f;
		for (int i = start; i < end; i++)
			sum += smoothBands[i];

		return sum / (end - start);
	}

	private void OnDestroy()
	{
		if (spectrumTexture != null)
			Destroy(spectrumTexture);
	}
}