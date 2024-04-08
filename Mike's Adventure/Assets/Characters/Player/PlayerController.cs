using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    //TODO: I do not know why, but running on a flat surface can cause the character to bounce upwards :/

    private Rigidbody2D _rb;
    private Animator _animator;
    private Vector2 _movementInput = Vector2.zero;
    private string _currentAnimationClipName;

    private bool _isFacingRight = true;
    private bool IsFacingRight
    {
        get => _isFacingRight;
        set
        {
            if (_isFacingRight != value)
            {
                // Flip x-orientation:
                transform.localScale *= new Vector2(-1, 1);
            }
            _isFacingRight = value;
        }
    }

    [Tooltip("Determines the maximum run-speed")]
    public float MaxRunSpeed = 10f;

    [Tooltip("Determines the maximum walk-speed")]
    public float MaxWalkSpeed = 5f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        Debug.Assert(MaxRunSpeed > 0, "'MaxRunSpeed' must be greater than 0!");
        Debug.Assert(MaxWalkSpeed > 0, "'MaxWalkSpeed' must be greater than 0!");
        Debug.Assert(MaxWalkSpeed <= MaxRunSpeed, "'MaxWalkSpeed' must be less than or equal to 'MaxRunSpeed'!");
    }

    private void Start()
    {
        ChangeAnimationState(AnimationClipNames.Idle);
    }

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        _rb.velocity = new Vector2(_movementInput.x * MaxRunSpeed, _rb.velocity.y);
        if(_rb.velocity.x == 0)
        {
            ChangeAnimationState(AnimationClipNames.Idle);
        }
        else if(Mathf.Abs(_rb.velocity.x) <= MaxWalkSpeed)
        {
            ChangeAnimationState(AnimationClipNames.Walk);
        }
        else
        {
            ChangeAnimationState(AnimationClipNames.Jog);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _movementInput = context.ReadValue<Vector2>();

        if (_movementInput.x > 0 && !IsFacingRight)
        {
            IsFacingRight = true;
        }
        else if (_movementInput.x < 0 && IsFacingRight)
        {
            IsFacingRight = false;
        }
    }

    private void ChangeAnimationState(string newAnimationClipName)
    {
        if(_currentAnimationClipName != newAnimationClipName)
        {
            _animator.Play(newAnimationClipName);
            _currentAnimationClipName = newAnimationClipName;
        }
    }
}
