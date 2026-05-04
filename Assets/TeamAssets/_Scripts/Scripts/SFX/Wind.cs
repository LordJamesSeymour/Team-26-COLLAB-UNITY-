using Group26.Player.Movement;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using static Unity.VisualScripting.Member;

public class Wind : MonoBehaviour
{
    Rigidbody rb;
    AudioSource source;
    float minSpeed = 22;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        StartCoroutine(playWind()); 
    }

    IEnumerator playWind()
    {
        yield return new WaitForSeconds(.01f);
        source = AudioManager.instance.PlaySoundFromObjectOnLoop(AudioManager.SoundType.WIND, transform, .2f, .02f, 1, .3f, 0);
        while (true)
        {
            float vel = rb.linearVelocity.magnitude;

            source.volume = Mathf.Clamp((vel - minSpeed) / 8, 0, 2);
            source.pitch = Mathf.Clamp((vel - minSpeed) / 13, .4f, .8f);

            yield return new WaitForSeconds(.01f);
        }
    }
}
