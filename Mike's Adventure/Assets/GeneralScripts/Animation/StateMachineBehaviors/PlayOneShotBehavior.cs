using UnityEngine;

public class PlayOneShotBehavior : PlayAudioClipBehavior
{
    [Tooltip("Determines if 'SoundToPlay' is played when the state is entered.")]
    public bool PlayOnEnter = true;

    [Tooltip("Determines if 'SoundToPlay' is played when the state is exited.")]
    public bool PlayOnExit = false;

    [Tooltip("Determines if 'SoundToPlay' is played after a delay since the state was entered.")]
    public bool PlayOnDelay = false;
    [Tooltip("Determines the length, in seconds, of the delay.")]
    public float PlayDelay = 0.25f;
    private float _timeSinceEntered = 0f;
    private bool _hasPlayedDelayedSound = false;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (PlayOnEnter)
        {
            // Assumption: It's always the animator owner that should be playing the sound
            PlaySoundAtAnimator(animator);
        }

        _timeSinceEntered = 0f;
        _hasPlayedDelayedSound = false;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (PlayOnDelay && !_hasPlayedDelayedSound)
        {
            _timeSinceEntered += Time.deltaTime;
            if (_timeSinceEntered > PlayDelay)
            {
                _hasPlayedDelayedSound = true;

                // Assumption: It's always the animator owner that should be playing the sound
                PlaySoundAtAnimator(animator);
            }
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (PlayOnExit)
        {
            // Assumption: It's always the animator owner that should be playing the sound
            PlaySoundAtAnimator(animator);
        }
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
