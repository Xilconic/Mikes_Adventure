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
[RequireComponent(typeof(PlayerConfiguration))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Animator _animator;
    private TouchingDirections _touchingDirections;
    private PlayerConfiguration _configuration;
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

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _touchingDirections = GetComponent<TouchingDirections>();
        _configuration = GetComponent<PlayerConfiguration>();

        _playerStateMachine = new PlayerStateMachine(_rb, new UnityAnimator(_animator), _configuration);
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
