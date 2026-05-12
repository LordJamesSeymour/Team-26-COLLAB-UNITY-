using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]

[Serializable]
public class SoundEntry
{
    public AudioManager.SoundType soundType;
    public AudioClip[] sounds;
}

public class AudioManager : MonoBehaviour
{
    [SerializeField] private SoundEntry[] soundList;
    public static AudioManager instance { get; private set; }

    private AudioSource audioSource;
    private AudioSource[] audioEmitters = new AudioSource[10];

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] string m_effects;
    [SerializeField] string m_music;

    public datamanager dm;

    public enum SoundType
    {
        STEP,
        JUMP,
        ROLL,
        GRAPPLE,
        WIND,
        CRASH,
        DASH,
        LAND,
        WALL_RUN,
        UI_BUTTON,
        COLLECTABLE
    }

    private void Awake()
    {
        if (!Application.isPlaying) { return; }

        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        SceneManager.sceneLoaded += OnSceneLoaded;

        for (int i = 0; i < audioEmitters.Length; i++)
        {
            audioEmitters[i] = new GameObject($"AudioEmitter_{i}").AddComponent<AudioSource>();
            audioEmitters[i].spatialBlend = 1.0f;
            audioEmitters[i].gameObject.SetActive(false);
            audioEmitters[i].transform.parent = transform;
            audioEmitters[i].outputAudioMixerGroup = GetComponent<AudioSource>().outputAudioMixerGroup;
        }
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        dm = new datamanager(6);
        try
        {
            dm.LoadGameData();

            float effectdb = Mathf.Clamp(
                Mathf.Log10(Mathf.Max(0.0001f, dm.GetGameData().settings.soundEffectsVolume / 100)) * 20,
                -80,
                0
            );
            audioMixer.SetFloat(m_effects, effectdb);

            float musicdb = Mathf.Clamp(
                Mathf.Log10(Mathf.Max(0.0001f, dm.GetGameData().settings.backgroundMusicVolume / 100)) * 20,
                -80,
                0
            );
            audioMixer.SetFloat(m_music, musicdb);
        }
        catch (Exception)
        {
            Debug.Log("None Found");
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode != LoadSceneMode.Single) return;

        StopAllCoroutines();

        for (int i = 0; i < audioEmitters.Length; i++)
        {
            if (audioEmitters[i] != null) Destroy(audioEmitters[i].gameObject);

            audioEmitters[i] = new GameObject($"AudioEmitter_{i}").AddComponent<AudioSource>();
            audioEmitters[i].spatialBlend = 1.0f;
            audioEmitters[i].gameObject.SetActive(false);
            audioEmitters[i].transform.parent = transform;
            audioEmitters[i].outputAudioMixerGroup = GetComponent<AudioSource>().outputAudioMixerGroup;
        }
    }

    public void PlayOneShotSound(SoundType sound, float volume)
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        AudioClip clip = SelectRandomSound(sound);
        if (clip == null) return;

        audioSource.PlayOneShot(clip, volume);
    }

    public AudioSource PlaySoundFromObject(
        SoundType sound,
        Transform target,
        float volume = 1,
        float volumeRange = 0,
        float pitch = 1,
        float pitchRange = 0,
        float spatialBlend = 1)
    {
        AudioSource source = GetAvailableSource();
        AudioClip clip = SelectRandomSound(sound);

        if (source == null || clip == null) { return null; }

        source.loop = false;
        source.gameObject.SetActive(true);
        source.transform.parent = target;
        source.transform.localPosition = Vector3.zero;
        source.clip = clip;

        source.volume = UnityEngine.Random.Range(volume * 10 - volumeRange * 10, volume * 10 + volumeRange * 10) / 10;
        source.pitch = UnityEngine.Random.Range(pitch * 10 - pitchRange * 10, pitch * 10 + pitchRange * 10) / 10;
        source.spatialBlend = spatialBlend;

        source.Play();

        if (!target.TryGetComponent<DetachEmitter>(out var detachScript))
        {
            target.gameObject.AddComponent<DetachEmitter>();
        }

        StartCoroutine(ReturnToPool(source, clip.length / Mathf.Abs(source.pitch)));

        return source;
    }

    public AudioSource PlaySoundFromObjectOnLoop(
        SoundType sound,
        Transform target,
        float volume = 1,
        float volumeRange = 0,
        float pitch = 1,
        float pitchRange = 0,
        float spatialBlend = 1)
    {
        AudioSource source = GetAvailableSource();
        AudioClip clip = SelectRandomSound(sound);

        if (source == null || clip == null) { return null; }

        source.loop = true;
        source.gameObject.SetActive(true);
        source.transform.parent = target;
        source.transform.localPosition = Vector3.zero;
        source.clip = clip;

        source.volume = UnityEngine.Random.Range(volume * 10 - volumeRange * 10, volume * 10 + volumeRange * 10) / 10;
        source.pitch = UnityEngine.Random.Range(pitch * 10 - pitchRange * 10, pitch * 10 + pitchRange * 10) / 10;
        source.spatialBlend = spatialBlend;

        source.Play();

        if (!target.TryGetComponent<DetachEmitter>(out var detachScript))
        {
            target.gameObject.AddComponent<DetachEmitter>();
        }

        return source;
    }

    public void EndSound(AudioSource end)
    {
        if (end != null)
        {
            StartCoroutine(ReturnToPool(end, 0));
        }
    }

    public AudioSource PlaySoundAtPoint(
        SoundType sound,
        Vector3 target,
        float volume = 1,
        float volumeRange = 0,
        float pitch = 1,
        float pitchRange = 0,
        float spatialBlend = 1)
    {
        AudioSource source = GetAvailableSource();
        AudioClip clip = SelectRandomSound(sound);

        if (source == null || clip == null) { return null; }

        source.loop = false;
        source.gameObject.SetActive(true);
        source.transform.position = target;
        source.clip = clip;

        source.volume = UnityEngine.Random.Range(volume * 10 - volumeRange * 10, volume * 10 + volumeRange * 10) / 10;
        source.pitch = UnityEngine.Random.Range(pitch * 10 - pitchRange * 10, pitch * 10 + pitchRange * 10) / 10;
        source.spatialBlend = spatialBlend;

        source.Play();
        source.transform.parent = null;

        StartCoroutine(ReturnToPool(source, clip.length / Mathf.Abs(source.pitch)));

        return source;
    }

    private AudioClip SelectRandomSound(SoundType sound)
    {
        if (soundList == null || soundList.Length == 0)
        {
            Debug.LogError("AudioManager soundList is null or empty.");
            return null;
        }

        foreach (var entry in soundList)
        {
            if (entry == null) continue;
            if (entry.soundType != sound) continue;

            if (entry.sounds == null || entry.sounds.Length == 0)
            {
                Debug.LogError($"No clips assigned for sound type {sound}.");
                return null;
            }

            return entry.sounds[UnityEngine.Random.Range(0, entry.sounds.Length)];
        }

        Debug.LogError($"No SoundEntry found for sound type {sound}.");
        return null;
    }

    private AudioSource GetAvailableSource()
    {
        foreach (AudioSource source in audioEmitters)
        {
            if (!source.gameObject.activeSelf) { return source; }
        }

        return null;
    }

    private IEnumerator ReturnToPool(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);

        float startVol = source.volume;
        while (source.volume > 0)
        {
            source.volume -= startVol * (Time.deltaTime / .05f);
            yield return null;
        }

        source.transform.parent = transform;
        source.gameObject.SetActive(false);

        if (source.clip != null) { source.Stop(); }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}