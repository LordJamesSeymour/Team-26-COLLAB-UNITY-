using Group26.Player.Movement;
using System.Collections;
using UnityEngine;
using static AudioManager;

public class PlayerAudio : MonoBehaviour
{
    private Coroutine m_stepSound;

    public void jumpSound()
    {
        //AudioManager.instance.PlayOneShotSound(SoundType.JUMP, .3f);
        //AudioManager.instance.PlaySoundFromObject(SoundType.JUMP, transform, .3f, .1f, 1, .05f);
        AudioManager.instance.PlaySoundAtPoint(SoundType.JUMP, transform.position, .02f, .01f, 1, .05f, 0);
    }

    public void walkSound(PlayerController.MovementState state)
    {
        if (m_stepSound != null) { return; }
        
        m_stepSound = StartCoroutine(playStep(state));
    }

    IEnumerator playStep(PlayerController.MovementState state) // would prefer to play this by calling the sound functions on the animation 
    {
        AudioManager.instance.PlaySoundAtPoint(SoundType.STEP, transform.position, .1f, .05f, 1, .15f, 0);
        if (state == PlayerController.MovementState.walking)
            yield return new WaitForSeconds(.35f);
        else if (state == PlayerController.MovementState.wallRunning)
            yield return new WaitForSeconds(.2f);
        m_stepSound = null;
    }
}
