using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource)), ExecuteAlways]
public class AudioManager : MonoBehaviour
{
    [SerializeField] private SoundList[] soundList;
    public  static AudioManager instance { get; private set;}
    private AudioSource audioSource;
    private AudioSource[] audioEmitters = new AudioSource[10];

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string effectsVolumeName = "EffectsVolume";
    [SerializeField] private string musicVolumeName = "MusicVolume";

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

        dm = new datamanager(6);
        try
        {
            dm.LoadGameData();
            audioMixer.SetFloat(effectsVolumeName, dm.GetGameData().settings.soundEffectsVolume);
            audioMixer.SetFloat(musicVolumeName, dm.GetGameData().settings.backgroundMusicVolume);
        }
        catch (Exception e) { Debug.Log("None Found"); }

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
        source.transform.parent = transform;
        source.gameObject.SetActive(false);
        source.Pause();
        if (source.clip != null) { source.Stop(); }
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