using UnityEngine;
using static AudioManager;

public class AnimationEventSFX : MonoBehaviour
{

    public void PlaySound(AudioEventData data)
    {
        Vector3 TargetPos = transform.position;
        Transform TargetObj = transform;

        switch (data.Type)
        {
            case AudioEventData.SFXType.OneShot:
                AudioManager.instance.PlayOneShotSound(data.sound, data.volume);
                break;
            case AudioEventData.SFXType.FromObject:
                AudioManager.instance.PlaySoundFromObject(data.sound, TargetObj, data.volume, data.volumeRange, data.pitch, data.pitchRange, data.spatialBlend);
                break;
            case AudioEventData.SFXType.FromObjectOnLoop:
                AudioManager.instance.PlaySoundFromObjectOnLoop(data.sound, TargetObj, data.volume, data.volumeRange, data.pitch, data.pitchRange, data.spatialBlend);
                break;
            case AudioEventData.SFXType.FromPoint:
                AudioManager.instance.PlaySoundAtPoint(data.sound, TargetPos, data.volume, data.volumeRange, data.pitch, data.pitchRange, data.spatialBlend);
                break;
        }
    }

}
