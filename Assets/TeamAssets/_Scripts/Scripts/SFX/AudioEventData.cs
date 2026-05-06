using UnityEngine;

[CreateAssetMenu(fileName = "NewAudioEvent", menuName = "Audio/AudioEventData")]
public class AudioEventData : ScriptableObject
{
    public SFXType Type;
    public AudioManager.SoundType sound;
    [Range(0,1)] public float volume = 1;
    [Range(0, .5f)] public float volumeRange = 0;
    [Range(0, 2)] public float pitch = 1;
    [Range(0, .5f)] public float pitchRange = 0;
    [Range(0, 1)] public float spatialBlend = 1;

    public enum SFXType
    {
        OneShot,
        FromObject,
        FromObjectOnLoop,
        FromPoint
    }
}
