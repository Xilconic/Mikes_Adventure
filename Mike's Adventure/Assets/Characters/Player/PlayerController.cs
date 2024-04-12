using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(TouchingDirections))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Animator _animator;
    private TouchingDirections _touchingDirections;
    [SerializeField, ReadOnlyField]
    private Vector2 _movementInput = Vector2.zero;
    [SerializeField, ReadOnlyField]
    private Vector2 _normalizedMovementInput = Vector2.zero;
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

    /// <remarks>
    /// Due to inaccuracies of the physica simulation, checking for '> 0' was giving 
    /// 'in mid air' readings when walking left. Adding a bit of deadzone before being
    /// considered in the air.
    /// </remarks>
    private bool IsRisingInAir => _rb.velocity.y > 1e-5;
    private bool HasJumpBeenTriggeredRecently => _jumpInputCooldown > 0;
    private bool HasLeftPlatformRecently => _coyoteTimeCooldown > 0;

    [Tooltip("Determines the maximum run-speed")]
    public float MaxRunSpeed = 10f;
    [Tooltip("Determines the maximum walk-speed")]
    public float MaxWalkSpeed = 5f;
    [Tooltip("Determines the lateral input dead-zone")]
    public float LateralInputDeadZone = 0.1f;

    [Tooltip("Determines the maximum walk-speed while crouched")]
    public float MaxChrouchWalkSpeed = 3f;
    [Tooltip("Determines the y-input level for detecting 'crouch input'")]
    public float CrouchInputZone = -0.5f;
    [Tooltip("Determines the crouch lateral input dead-zone")]
    public float CrouchLateralInputDeadZone = 0.1f;

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

    [Tooltip("Determines the maximum Wall-Slide velocity")]
    public float MaxWallSlideVelocity = -3f;

    [field: SerializeField, ReadOnlyField, Tooltip("Indicates the state the PlayerController is in.")]
    private State PlayerState { get; set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _touchingDirections = GetComponent<TouchingDirections>();

        Debug.Assert(MaxRunSpeed > 0, "'MaxRunSpeed' must be greater than 0!");
        Debug.Assert(MaxWalkSpeed > 0, "'MaxWalkSpeed' must be greater than 0!");
        Debug.Assert(MaxWalkSpeed <= MaxRunSpeed, "'MaxWalkSpeed' must be less than or equal to 'MaxRunSpeed'!");
        Debug.Assert(LateralInputDeadZone >= 0, "'LateralInputDeadZone' must be greater than 0!");
        Debug.Assert(JumpImpulse > 0, "'JumpImpulse' must be greater than 0!");
        Debug.Assert(JumpingGravityScale > 0, "'JumpingGravityScale' must be greater than 0!");
        Debug.Assert(FallingGravityScale > 0, "'FallingGravityScale' must be greater than 0!");
        Debug.Assert(JumpBuffer >= 0, "'JumpBuffer' must be greater than or equal to 0!");
        Debug.Assert(MaxChrouchWalkSpeed > 0, "'MaxChrouchWalkSpeed' must be greater than 0!");
        Debug.Assert(-1f <= CrouchInputZone && CrouchInputZone <= 0.0f, "'CrouchInputZone' must be in range [-1.0, 0.0]!");
        Debug.Assert(CrouchLateralInputDeadZone >= 0, "'CrouchLateralInputDeadZone' must be greater than 0!");
        Debug.Assert(MaxWallSlideVelocity <= 0, "'MaxWallSlideVelocity' must be less than or equal to 0!");
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
        if (_touchingDirections.IsGrounded)
        {
            _touchedGround = true;

            if (HasJumpBeenTriggeredRecently && !_touchingDirections.IsOnCeiling)
            {
                _rb.AdjustVelocityX(_movementInput.x * MaxRunSpeed);
                PerformJump();
            }
            else if (IsRisingInAir)
            {
                // Keep using the 'Jump' animation clip while going up in the air
                // TODO: What if we have effects that knock us up in the air? Enemy attacks, traps, bouncy pads? Then we should be setting a 'air rising' animation when not in the middle of the 'jump squat' animation.
                _rb.AdjustVelocityX(_movementInput.x * MaxRunSpeed);
            }
            else
            {
                if (_normalizedMovementInput.y < CrouchInputZone ||
                    (PlayerState == State.Crouching && _touchingDirections.IsOnCeiling))
                {
                    PlayerState = State.Crouching;
                    // Have a stable dead-zone for crouching to allow for standing still while crouched:
                    if (Mathf.Abs(_movementInput.x) <= CrouchLateralInputDeadZone)
                    {
                        _rb.AdjustVelocityX(0);
                        ChangeAnimationState(AnimationClipNames.CrouchIdle);
                    }
                    else
                    {
                        _rb.AdjustVelocityX(_movementInput.x * MaxChrouchWalkSpeed);
                        ChangeAnimationState(AnimationClipNames.CrouchWalk);
                    }
                }
                else
                {
                    PlayerState = State.Grounded;
                    _rb.AdjustVelocityX(_movementInput.x * MaxRunSpeed);
                    if (Mathf.Abs(_rb.velocity.x) > MaxWalkSpeed)
                    {
                        ChangeAnimationState(AnimationClipNames.Jog);
                    }
                    else if (Mathf.Abs(_rb.velocity.x) > 1e-5)
                    {
                        ChangeAnimationState(AnimationClipNames.Walk);
                    }
                    else
                    {
                        ChangeAnimationState(AnimationClipNames.Idle);
                    }
                }
            }
        }
        else
        {
            _rb.AdjustVelocityX(_movementInput.x * MaxRunSpeed);
            if(!IsRisingInAir)
            {
                _rb.gravityScale = FallingGravityScale;
            }

            if (_touchingDirections.IsOnWall)
            {
                PlayerState = State.InMidAir;
                ChangeAnimationState(AnimationClipNames.WallSlide);

                _rb.AdjustVelocityY(MathF.Max(MaxWallSlideVelocity, _rb.velocity.y));
            }
            else
            {
                PlayerState = State.InMidAir;
                // Went from touching the ground to no longer touching the ground:
                if (_touchedGround)
                {
                    _coyoteTimeCooldown = CoyoteTimeBuffer;
                }
                else if (HasLeftPlatformRecently && HasJumpBeenTriggeredRecently)
                {
                    PerformJump();
                }

                if (!IsRisingInAir)
                {
                    ChangeAnimationState(AnimationClipNames.Falling);
                }
            }
            
            _touchedGround = false;
        }
    }

    private void PerformJump()
    {
        _jumpInputCooldown = 0;
        ChangeAnimationState(AnimationClipNames.Jump);
        _rb.gravityScale = JumpingGravityScale;
        _rb.AdjustVelocityY(JumpImpulse);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _movementInput = context.ReadValue<Vector2>();
        _normalizedMovementInput = _movementInput.normalized;

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
            _rb.AdjustVelocityY(_rb.velocity.y * 0.5f);
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

    private enum State
    {
        Grounded,
        Crouching,
        InMidAir
    }
}
