using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(TouchingDirections))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Animator _animator;
    private TouchingDirections _touchingDirections;
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

    private bool IsRisingInAir => _rb.velocity.y > 0;

    [Tooltip("Determines the maximum run-speed")]
    public float MaxRunSpeed = 10f;

    [Tooltip("Determines the maximum walk-speed")]
    public float MaxWalkSpeed = 5f;

    [Tooltip("Determines the jump strength")]
    public float JumpImpulse = 10f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _touchingDirections = GetComponent<TouchingDirections>();
        Debug.Assert(MaxRunSpeed > 0, "'MaxRunSpeed' must be greater than 0!");
        Debug.Assert(MaxWalkSpeed > 0, "'MaxWalkSpeed' must be greater than 0!");
        Debug.Assert(MaxWalkSpeed <= MaxRunSpeed, "'MaxWalkSpeed' must be less than or equal to 'MaxRunSpeed'!");
        Debug.Assert(JumpImpulse > 0, "'JumpImpulse' must be greater than 0!");
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
        if(_touchingDirections.IsGrounded &&
            !IsRisingInAir) //Not rising into the air
        {
            if (_rb.velocity.x == 0)
            {
                ChangeAnimationState(AnimationClipNames.Idle);
            }
            else if (Mathf.Abs(_rb.velocity.x) <= MaxWalkSpeed)
            {
                ChangeAnimationState(AnimationClipNames.Walk);
            }
            else
            {
                ChangeAnimationState(AnimationClipNames.Jog);
            }
        }
        else
        {
            if (_rb.velocity.y < 0)
            {
                ChangeAnimationState(AnimationClipNames.Falling);
            }
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

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started &&
            _touchingDirections.IsGrounded)
        {
            ChangeAnimationState(AnimationClipNames.Jump);
            _rb.velocity = new Vector2(_rb.velocity.x, JumpImpulse);
        }
        // Allow for 'short hopping' on release:
        if (context.canceled && IsRisingInAir)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, _rb.velocity.y * 0.5f);
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
