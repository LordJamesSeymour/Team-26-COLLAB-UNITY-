using UnityEngine;

public class AudioAnalyzer : MonoBehaviour
{
    [Header("Target Renderer")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Audio Auto-Find")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool searchInChildren = true;

    [Header("Audio")]
    [SerializeField] private FFTWindow fftWindow = FFTWindow.BlackmanHarris;
    [SerializeField, Range(64, 8192)] private int spectrumSampleCount = 1024;

    [Header("Bands")]
    [SerializeField, Range(32, 512)] private int bandCount = 256;
    [SerializeField] private float sensitivity = 80f;
    [SerializeField] private float riseSpeed = 20f;
    [SerializeField] private float fallSpeed = 8f;

    [Header("Layout")]
    [SerializeField] private bool centerOut = false;

    [Header("Curve Smoothing")]
    [SerializeField, Range(0, 8)] private int curveSmoothRadius = 3;
    [SerializeField, Range(0, 6)] private int curveSmoothPasses = 2;

    [Header("Shader Property Names")]
    [SerializeField] private string spectrumTextureProperty = "_SpectrumTex";
    [SerializeField] private string amplitudeProperty = "_Amplitude";
    [SerializeField] private string bassProperty = "_Bass";
    [SerializeField] private string midProperty = "_Mid";
    [SerializeField] private string trebleProperty = "_Treble";

    private static AudioSource sharedAudioSource;

    private AudioSource audioSource;
    private MaterialPropertyBlock propertyBlock;

    private float[] spectrumSamples;
    private float[] rawBands;
    private float[] smoothBands;
    private float[] displayBands;
    private float[] smoothingBuffer;

    private Texture2D spectrumTexture;
    private Color[] spectrumPixels;

    public void SetTargetRenderer(Renderer renderer)
    {
        targetRenderer = renderer;
    }

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        if (targetRenderer == null)
        {
            Debug.LogWarning($"AudioAnalyzer on '{name}' could not find a Renderer. Disable or assign Target Renderer manually.", this);
            enabled = false;
            return;
        }

        propertyBlock = new MaterialPropertyBlock();

        spectrumSamples = new float[spectrumSampleCount];
        rawBands = new float[bandCount];
        smoothBands = new float[bandCount];
        displayBands = new float[bandCount];
        smoothingBuffer = new float[bandCount];
        spectrumPixels = new Color[bandCount];

        spectrumTexture = new Texture2D(bandCount, 1, TextureFormat.RFloat, false, true);
        spectrumTexture.wrapMode = TextureWrapMode.Repeat;
        spectrumTexture.filterMode = FilterMode.Bilinear;
    }

    private void Start()
    {
        ResolveAudioSource();
    }

    private void Update()
    {
        if (targetRenderer == null)
            return;

        if (audioSource == null)
        {
            ResolveAudioSource();
            return;
        }

        if (!audioSource.isPlaying)
            return;

        audioSource.GetSpectrumData(spectrumSamples, 0, fftWindow);

        BuildBands();
        SmoothBandsOverTime();
        SmoothBandsAcrossX();
        UpdateSpectrumTexture();
        PushToMaterial();
    }

    private void ResolveAudioSource()
    {
        if (sharedAudioSource != null)
        {
            audioSource = sharedAudioSource;
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
            return;

        audioSource = searchInChildren
            ? player.GetComponentInChildren<AudioSource>()
            : player.GetComponent<AudioSource>();

        if (audioSource != null)
            sharedAudioSource = audioSource;
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

    private void SmoothBandsOverTime()
    {
        float dt = Time.deltaTime;

        for (int i = 0; i < bandCount; i++)
        {
            float speed = rawBands[i] > smoothBands[i] ? riseSpeed : fallSpeed;
            float t = 1f - Mathf.Exp(-speed * dt);
            smoothBands[i] = Mathf.Lerp(smoothBands[i], rawBands[i], t);
        }
    }

    private void SmoothBandsAcrossX()
    {
        for (int i = 0; i < bandCount; i++)
            displayBands[i] = smoothBands[i];

        for (int pass = 0; pass < curveSmoothPasses; pass++)
        {
            for (int i = 0; i < bandCount; i++)
            {
                float sum = 0f;
                float weightSum = 0f;

                for (int offset = -curveSmoothRadius; offset <= curveSmoothRadius; offset++)
                {
                    int index = Mathf.Clamp(i + offset, 0, bandCount - 1);
                    float weight = curveSmoothRadius + 1 - Mathf.Abs(offset);
                    sum += displayBands[index] * weight;
                    weightSum += weight;
                }

                smoothingBuffer[i] = sum / Mathf.Max(0.0001f, weightSum);
            }

            for (int i = 0; i < bandCount; i++)
                displayBands[i] = smoothingBuffer[i];
        }
    }

    private void UpdateSpectrumTexture()
    {
        for (int x = 0; x < bandCount; x++)
        {
            int bandIndex = centerOut ? GetCenterOutBandIndex(x) : x;
            float v = displayBands[bandIndex];
            spectrumPixels[x] = new Color(v, 0f, 0f, 1f);
        }

        spectrumTexture.SetPixels(spectrumPixels);
        spectrumTexture.Apply(false, false);
    }

    private int GetCenterOutBandIndex(int textureX)
    {
        float u = (textureX + 0.5f) / bandCount;
        float distanceFromCenter01 = Mathf.Abs(u - 0.5f) / 0.5f;
        int bandIndex = Mathf.RoundToInt(distanceFromCenter01 * (bandCount - 1));
        return Mathf.Clamp(bandIndex, 0, bandCount - 1);
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
        start = Mathf.Clamp(start, 0, displayBands.Length - 1);
        end = Mathf.Clamp(end, start + 1, displayBands.Length);

        float sum = 0f;
        for (int i = start; i < end; i++)
            sum += displayBands[i];

        return sum / (end - start);
    }

    private void OnDestroy()
    {
        if (spectrumTexture != null)
            Destroy(spectrumTexture);
    }
}