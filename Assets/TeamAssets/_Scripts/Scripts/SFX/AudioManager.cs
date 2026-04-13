using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] soundList;
    public  static AudioManager instance { get; private set;}
    private AudioSource audioSource;

    private AudioSource[] audioEmitters = new AudioSource[10];

    public enum SoundType
    {
        WALKING,
        JUMP,
        RUNNING,
        ROLLING,
        GRAPPLE,
        WIND,
        CRASH,
        DASH,


    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        //set up emitter pool
        for (int i = 0; i < audioEmitters.Length; i++)
        {
            audioEmitters[i] = new GameObject().AddComponent<AudioSource>();
            audioEmitters[i].spatialBlend = 1.0f;
            audioEmitters[i].gameObject.SetActive(false);
            audioEmitters[i].transform.parent = transform;
        }
    }
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayOneShotSound(SoundType sound, float volume) // only use for global sounds with no pitch variation, use as mutch as possible to avoid over using pool
    {
        audioSource.PlayOneShot(soundList[(int)sound], volume);
    }

    public void PlaySound(SoundType sound, Transform target, float volume, float volumeRange, float pitch, float pitchRange)
    {
        AudioSource source = GetAvailableSource();
        AudioClip clip = soundList[(int)sound];
        if (source != null)
        {
            source.gameObject.SetActive(true);
            source.transform.parent = target;
            source.transform.localPosition = Vector3.zero;
            source.clip = clip;

            source.volume = Random.Range(volume * 10 - volumeRange * 10, volume * 10 + volumeRange * 10) / 10;
            source.pitch = Random.Range(pitch * 10 - pitchRange * 10, pitch * 10 + pitchRange * 10) / 10;

            source.Play();

            if (!target.TryGetComponent<DetachEmitter>(out var detachScript)) // if an object playing sound has the parent destroyed this added script will detatch it
            {
                target.gameObject.AddComponent<DetachEmitter>();
            }

            StartCoroutine(ReturnToPool(source, clip.length / Mathf.Abs(source.pitch)));
        }
    }

    private AudioSource GetAvailableSource()
    {
        foreach (AudioSource source in audioEmitters)
        {
            if (!source.gameObject.activeInHierarchy) { return source; }
        }
        return null; // all sources busy, probably play as global sound as back up or add a new source to the pool if not too many
    }

    private IEnumerator ReturnToPool(AudioSource source, float delay) // need to also return to pool if the objects parent is destroyed as to not also destroy the emitter. (maybe also have a 
    {
        yield return new WaitForSeconds(delay);
        source.Stop();
        source.transform.parent = transform;
        source.gameObject.SetActive(false);
    }
}
