using UnityEngine;

public class StateMachineLoopSFX : StateMachineBehaviour
{
    [SerializeField] private AudioEventData data;
    AudioSource source;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        source = AudioManager.instance.PlaySoundFromObjectOnLoop(data.sound, animator.gameObject.transform, data.volume, data.volumeRange, data.pitch, data.pitchRange, data.spatialBlend);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        AudioManager.instance.EndSound(source);
    }
}
