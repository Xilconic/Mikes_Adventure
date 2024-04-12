﻿using UnityEngine;

public abstract class PlayAudioClipBehavior : StateMachineBehaviour
{
    [Tooltip("Determines the AudioClip to be played.")]
    public AudioClip SoundToPlay;

    [Tooltip("Determines the volume of 'SoundToPlay' when played, in range [0.0, 1.0].")]
    public float Volume = 1f;

    protected void PlaySoundAtAnimator(Animator animator) =>
        AudioSource.PlayClipAtPoint(SoundToPlay, animator.gameObject.transform.position, Volume);
}