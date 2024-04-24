using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.GeneralScripts;
using UnityEngine;

namespace Assets.Characters.Player
{
    public class PlayerStateMachine
    {
        /// <seealso cref="PlayerController.JumpBuffer">
        private const float JumpBuffer = 0.1f;
        private readonly Rigidbody2D _rigidbody;
        private readonly IAnimator _animator;
        private float _jumpBufferCooldown = 0;

        private Vector2 _movementInput;

        public PlayerStateMachine(
            Rigidbody2D rigidbody,
            IAnimator animator)
        {
            _rigidbody = rigidbody;
            _animator = animator;
            CurrentState = new GroundedState(_rigidbody, _animator);
        }

        public ITime Time { get; set; } = new UnityTime();

        public IState CurrentState { get; private set; }
        public IState ActiveChildState => CurrentState.ActiveChildState;

        public void Jump()
        {
            if (CurrentState.CanJump)
            {
                var state = new AirialState(_rigidbody, _animator, Time);
                CurrentState = state;
                CurrentState.OnEnter();
                state.Jump();
            }
            else
            {
                _jumpBufferCooldown = JumpBuffer;
            }
        }

        public void JumpRelease()
        {
            if(CurrentState is AirialState airialState)
            {
                airialState.JumpRelease();
            }
        }

        public void NotifyTouchingDirections(ITouchingDirections touchingDirections)
        {
            if (!touchingDirections.IsGrounded && CurrentState is GroundedState)
            {
                CurrentState = new AirialState(_rigidbody, _animator, Time);
                CurrentState.OnEnter();
            }
            else if (touchingDirections.IsGrounded && CurrentState is AirialState)
            {
                var groundedState = new GroundedState(_rigidbody, _animator);
                CurrentState = groundedState;
                CurrentState.OnEnter();
                if (_jumpBufferCooldown > 0)
                {
                    Jump();
                }
                else
                {
                    CurrentState.SetMovement(_movementInput);
                }
                
            }

            if (CurrentState is GroundedState groundState)
            {
                groundState.NotifyTouchingDirections(touchingDirections);
            }
        }

        public void SetMovement(Vector2 movementInput)
        {
            _movementInput = movementInput;
            CurrentState.SetMovement(movementInput);
        }

        /// <summary>
        /// Method intended to be called inside <c>MonoBehavior.Update()</c>.
        /// </summary>
        /// <seealse cref="MonoBehaviour.Update"/>
        public void Update()
        {
            CurrentState.Update();

            if(_jumpBufferCooldown > 0)
            {
                _jumpBufferCooldown -= Time.DeltaTime;
            }
        }

        /// <summary>
        /// Method intended to be called inside <c>MonoBehavior.FixedUpdate()</c>.
        /// </summary>
        /// <seealse cref="MonoBehaviour.FixedUpdate"/>
        public void FixedUpdate()
        {
            CurrentState.FixedUpdate();
        }
    }
}
