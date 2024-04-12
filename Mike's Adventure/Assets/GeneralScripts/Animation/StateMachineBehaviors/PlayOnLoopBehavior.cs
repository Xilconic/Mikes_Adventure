using UnityEngine;

public class PlayOnLoopBehavior : PlayAudioClipBehavior
{
    [Tooltip("Determines if 'SoundToPlay' is played when the state is entered.")]
    public bool PlayOnEnter = true;

    [Tooltip("Determines how frequent 'SoundToPlay' is played, (0.0, float.Max] seconds")]
    public float SoundToPlayLoopCooldown = 1.0f;
    private float _currentCooldown = 0;

    [Tooltip("Determines if 'SoundToPlay' need to stop immediately when the state is exited.")]
    public bool StopOnExit = false;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _currentCooldown = SoundToPlayLoopCooldown;
        if (PlayOnEnter)
        {
            // Assumption: It's always the animator owner that should be playing the sound
            PlaySoundAtAnimator(animator);
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _currentCooldown -= Time.deltaTime;
        if(_currentCooldown <= 0)
        {
            // Assumption: It's always the animator owner that should be playing the sound
            PlaySoundAtAnimator(animator);

            _currentCooldown = SoundToPlayLoopCooldown;
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (StopOnExit)
        {
            StopSoundAtAnimator(animator);
        }
    }
}
