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

    [Tooltip("Jumping gravity scale")]
    public float JumpingGravityScale = 1f;

    [Tooltip("Falling gravity scale")]
    public float FallingGravityScale = 2f;

    /// <remarks>
    /// <para>Average Clicks-per-second for people is about 6.5, which would mean that a jump buffer of ~0.15 on average would leave no gaps when spamming the button.</para>
    /// <para>UX Research indicated that a <=0.1s response time is typically experienced as 'instantaneous'.</para>
    /// </remarks>
    [Tooltip("Jump input buffer, in seconds")]
    public float JumpBuffer = 0.1f;
    private float _jumpInputCooldown = 0;

    [Tooltip("Jump input buffer after leaving ground, in seconds")]
    public float CoyoteTimeBuffer = 0.1f;
    private float _coyoteTimeCooldown = 0;
    private bool _touchedGround = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _touchingDirections = GetComponent<TouchingDirections>();

        Debug.Assert(MaxRunSpeed > 0, "'MaxRunSpeed' must be greater than 0!");
        Debug.Assert(MaxWalkSpeed > 0, "'MaxWalkSpeed' must be greater than 0!");
        Debug.Assert(MaxWalkSpeed <= MaxRunSpeed, "'MaxWalkSpeed' must be less than or equal to 'MaxRunSpeed'!");
        Debug.Assert(JumpImpulse > 0, "'JumpImpulse' must be greater than 0!");
        Debug.Assert(JumpingGravityScale > 0, "'JumpingGravityScale' must be greater than 0!");
        Debug.Assert(FallingGravityScale > 0, "'FallingGravityScale' must be greater than 0!");
        Debug.Assert(JumpBuffer >= 0, "'JumpBuffer' must be greater than or equal to 0!");
    }

    private void Start()
    {
        ChangeAnimationState(AnimationClipNames.Idle);
    }

    private void Update()
    {
        _jumpInputCooldown = Mathf.Max(0, _jumpInputCooldown - Time.deltaTime);
        _coyoteTimeCooldown = Mathf.Max(-1, _coyoteTimeCooldown - Time.deltaTime);
    }

    private void FixedUpdate()
    {
        _rb.velocity = new Vector2(_movementInput.x * MaxRunSpeed, _rb.velocity.y);
        if (_touchingDirections.IsGrounded)
        {
            _touchedGround = true;

            if (_jumpInputCooldown > 0)
            {
                _jumpInputCooldown = 0;
                ChangeAnimationState(AnimationClipNames.Jump);
                _rb.gravityScale = JumpingGravityScale;
                _rb.velocity = new Vector2(_rb.velocity.x, JumpImpulse);
            }
            else if (IsRisingInAir)
            {
                // Keep using the 'Jump' animation clip while going up in the air
                // TODO: What if we have effects that knock us up in the air? Enemy attacks, traps, bouncy pads? Then we should be setting a 'air rising' animation when not in the middle of the 'jump squat' animation.
            }
            else
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
        }
        else
        {
            // Went from touching the ground to no longer touching the ground:
            if (_touchedGround) 
            {
                _coyoteTimeCooldown = CoyoteTimeBuffer;
            }
            else if (_coyoteTimeCooldown > 0 && _jumpInputCooldown > 0)
            {
                _jumpInputCooldown = 0;
                ChangeAnimationState(AnimationClipNames.Jump);
                _rb.gravityScale = JumpingGravityScale;
                _rb.velocity = new Vector2(_rb.velocity.x, JumpImpulse);
            }
            
            if (!IsRisingInAir)
            {
                _rb.gravityScale = FallingGravityScale;
                ChangeAnimationState(AnimationClipNames.Falling);
            }

            _touchedGround = false;
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
        if(context.started)
        {
            _jumpInputCooldown = JumpBuffer;
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
