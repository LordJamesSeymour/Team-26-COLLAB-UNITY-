using Group26.Player.Movement;
using System.Collections;
using UnityEngine;
using static AudioManager;

public class PlayerAudio : MonoBehaviour
{
    public void jumpSound()
    {
        //AudioManager.instance.PlayOneShotSound(SoundType.JUMP, .3f);
        //AudioManager.instance.PlaySoundFromObject(SoundType.JUMP, transform, .3f, .1f, 1, .05f);
        //AudioManager.instance.PlaySoundAtPoint(SoundType.JUMP, transform.position, .02f, .01f, 1, .05f, 0);
    }
}
