using Group26.Player.Movement;
using UnityEngine;
using static AudioManager;

public class PlayerAudio : MonoBehaviour
{
    PlayerController controller;
    PlayerController.MovementState prevState;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.state == PlayerController.MovementState.air && prevState == PlayerController.MovementState.walking)
        {
            //AudioManager.instance.PlayOneShotSound(SoundType.JUMP, .3f);
            //AudioManager.instance.PlaySoundFromObject(SoundType.JUMP, transform, .3f, .1f, 1, .05f);
            AudioManager.instance.PlaySoundAtPoint(SoundType.JUMP, transform.position, .3f, .1f, 1, .05f, 0);
        }

        prevState = controller.state;
    }
}
