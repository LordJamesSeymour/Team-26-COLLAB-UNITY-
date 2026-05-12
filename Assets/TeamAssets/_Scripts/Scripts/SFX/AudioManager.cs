using DG.Tweening;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource)), ExecuteAlways]
public class AudioManager : MonoBehaviour
{
    [SerializeField] private SoundList[] soundList;
    public  static AudioManager instance { get; private set;}
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
        UI_BUTTON
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

        SceneManager.sceneLoaded += OnSceneLoaded;

        //set up emitter pool
        for (int i = 0; i < audioEmitters.Length; i++)
        {
            audioEmitters[i] = new GameObject().AddComponent<AudioSource>();
            audioEmitters[i].spatialBlend = 1.0f;
            audioEmitters[i].gameObject.SetActive(false);
            audioEmitters[i].transform.parent = transform;
            audioEmitters[i].outputAudioMixerGroup = GetComponent<AudioSource>().outputAudioMixerGroup;
            //audioEmitters[i].outputAudioMixerGroup = Resources.Load("GameAudio") as AudioMixer mixer.FindMatchingGroups(OutputMixer)[0];
        }
    }
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        dm = new datamanager(6);
        try
        {
            dm.LoadGameData();
            float effectdb = Mathf.Clamp(Mathf.Log10(Mathf.Max(0.0001f, dm.GetGameData().settings.soundEffectsVolume / 100)) * 20, -80, 0);
            audioMixer.SetFloat(m_effects, effectdb);
            float musicdb = Mathf.Clamp(Mathf.Log10(Mathf.Max(0.0001f, dm.GetGameData().settings.backgroundMusicVolume / 100)) * 20, -80, 0);
            audioMixer.SetFloat(m_music, musicdb);
        }
        catch (Exception e) { Debug.Log("None Found"); }
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode != LoadSceneMode.Single) return;
        StopAllCoroutines();
        for (int i = 0; i < audioEmitters.Length; i++)
        {
            if (audioEmitters[i] != null) Destroy(audioEmitters[i].gameObject);
            audioEmitters[i] = new GameObject().AddComponent<AudioSource>();
            audioEmitters[i].spatialBlend = 1.0f;
            audioEmitters[i].gameObject.SetActive(false);
            audioEmitters[i].transform.parent = transform;
            audioEmitters[i].outputAudioMixerGroup = GetComponent<AudioSource>().outputAudioMixerGroup;
            //audioEmitters[i].outputAudioMixerGroup = Resources.Load("GameAudio") as AudioMixer mixer.FindMatchingGroups(OutputMixer)[0];
        }
    }

    public void PlayOneShotSound(SoundType sound, float volume) // only use for global sounds with no pitch variation, use as mutch as possible to avoid over using pool
    {
        audioSource.PlayOneShot(SelectRandomSound(sound), volume);
    }

    public AudioSource PlaySoundFromObject(SoundType sound, Transform target, float volume = 1, float volumeRange = 0, float pitch = 1, float pitchRange = 0, float spatialBlend = 1)
    {
        AudioSource source = GetAvailableSource();
        AudioClip clip = SelectRandomSound(sound);

        if (source == null) { return null; }

        source.loop = false;
        source.gameObject.SetActive(true);
        source.transform.parent = target;
        source.transform.localPosition = Vector3.zero;
        source.clip = clip;

        source.volume = UnityEngine.Random.Range(volume * 10 - volumeRange * 10, volume * 10 + volumeRange * 10) / 10;
        source.pitch = UnityEngine.Random.Range(pitch * 10 - pitchRange * 10, pitch * 10 + pitchRange * 10) / 10;

        source.spatialBlend = spatialBlend;

        source.Play();

        if (!target.TryGetComponent<DetachEmitter>(out var detachScript)) // if an object playing sound has the parent destroyed this added script will detatch it
        {
            target.gameObject.AddComponent<DetachEmitter>();
        }

        StartCoroutine(ReturnToPool(source, clip.length / Mathf.Abs(source.pitch)));

        return source;

    }

    public AudioSource PlaySoundFromObjectOnLoop(SoundType sound, Transform target, float volume = 1, float volumeRange = 0, float pitch = 1, float pitchRange = 0, float spatialBlend = 1)
    {
        AudioSource source = GetAvailableSource();
        AudioClip clip = SelectRandomSound(sound);

        if (source == null) { return null; } 
        
        source.loop = true;
        source.gameObject.SetActive(true);
        source.transform.parent = target;
        source.transform.localPosition = Vector3.zero;
        source.clip = clip;

        source.volume = UnityEngine.Random.Range(volume * 10 - volumeRange * 10, volume * 10 + volumeRange * 10) / 10;
        source.pitch = UnityEngine.Random.Range(pitch * 10 - pitchRange * 10, pitch * 10 + pitchRange * 10) / 10;

        source.spatialBlend = spatialBlend;

        source.Play();

        if (!target.TryGetComponent<DetachEmitter>(out var detachScript)) // if an object playing sound has the parent destroyed this added script will detatch it
        {
            target.gameObject.AddComponent<DetachEmitter>();
        }

        return source; // need to 
    }

    public void EndSound(AudioSource end)
    {
        StartCoroutine(ReturnToPool(end, 0));
    }

    public AudioSource PlaySoundAtPoint(SoundType sound, Vector3 target, float volume = 1, float volumeRange = 0, float pitch = 1, float pitchRange = 0, float spatialBlend = 1)
    {
        AudioSource source = GetAvailableSource();
        AudioClip clip = SelectRandomSound(sound);

        if (source == null) { return null; }

        source.loop = false;
        source.gameObject.SetActive(true);
        source.transform.position = target;
        source.clip = clip;

        source.volume = UnityEngine.Random.Range(volume * 10 - volumeRange * 10, volume * 10 + volumeRange * 10) / 10;
        source.pitch = UnityEngine.Random.Range(pitch * 10 - pitchRange * 10, pitch * 10 + pitchRange * 10) / 10;

        source.spatialBlend = spatialBlend;

        source.Play();
        source.transform.parent = null;

        StartCoroutine(ReturnToPool(source, clip.length / Mathf.Abs(source.pitch))); // can add .1f to these for safety if there is cutoff issues

        return source;

    }

    private AudioClip SelectRandomSound(SoundType sound)
    {
        AudioClip[] clips = soundList[(int)sound].Sounds;
        return clips[UnityEngine.Random.Range(0, clips.Length)];
    }

    private AudioSource GetAvailableSource()
    {
        foreach (AudioSource source in audioEmitters)
        {
            if (!source.gameObject.activeSelf) { return source; }
        }
        return null; // all sources busy, probably play as global sound as back up or add a new source to the pool if not too many
    }

    private IEnumerator ReturnToPool(AudioSource source, float delay) // need to also return to pool if the objects parent is destroyed as to not also destroy the emitter. (maybe also have a 
    {
        yield return new WaitForSeconds(delay);

        // fade out

        float startVol = source.volume;
        while (source.volume > 0)
        {
            source.volume -= startVol * (Time.deltaTime / .05f); // .05s fade
            yield return null;
        }


        source.transform.parent = transform;
        source.gameObject.SetActive(false);
        //source.Pause();
        if (source.clip != null) { source.Stop(); }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref soundList, names.Length);
        for (int i = 0; i < soundList.Length; i++)
        {
            soundList[i].name = names[i];
        }
    }
#endif
}

[Serializable]
public struct SoundList
{
    public AudioClip[] Sounds { get => sounds; }
    [HideInInspector] public string name;
    [SerializeField] private AudioClip[] sounds;
}