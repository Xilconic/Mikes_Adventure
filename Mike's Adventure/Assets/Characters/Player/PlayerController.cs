using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Characters.Player;
using Assets.GeneralScripts;
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

    private PlayerStateMachine _playerStateMachine;

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

    [Tooltip("Jump input buffer after leaving ground, in seconds")]
    public float CoyoteTimeBuffer = 0.1f;

    [Tooltip("Determines the maximum Wall-Slide velocity")]
    public float MaxWallSlideVelocity = -3f;

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

        _playerStateMachine = new PlayerStateMachine(_rb, new UnityAnimator(_animator));
    }

    private void Update()
    {
        _playerStateMachine.Update();
    }

    private void FixedUpdate()
    {
        _playerStateMachine.NotifyTouchingDirections(_touchingDirections);
        _playerStateMachine.FixedUpdate();
        
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
        _playerStateMachine.SetMovement(_movementInput);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            _playerStateMachine.Jump();
        }

        if (context.canceled)
        {
            _playerStateMachine.JumpRelease();
        }
    }
}
