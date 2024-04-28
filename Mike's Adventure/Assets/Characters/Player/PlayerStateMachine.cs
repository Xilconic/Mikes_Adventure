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
        private readonly Rigidbody2D _rigidbody;
        private readonly IAnimator _animator;
        private readonly PlayerConfiguration _configuration;
        
        private Vector2 _movementInput;
        private float _jumpBufferCooldown = 0;
        private float _coyoteTimeCooldown = 0;

        public PlayerStateMachine(
            Rigidbody2D rigidbody,
            IAnimator animator,
            PlayerConfiguration configuration)
        {
            _rigidbody = rigidbody;
            _animator = animator;
            _configuration = configuration;
            CurrentState = new GroundedState(_rigidbody, _animator, _configuration);
        }

        public ITime Time { get; set; } = new UnityTime();

        public IState CurrentState { get; private set; }
        public IState ActiveChildState => CurrentState.ActiveChildState;

        public void Jump()
        {
            if (CurrentState.CanJump)
            {
                var state = AirialState.CreateJumpingState(_rigidbody, _animator, _configuration);
                ChangeCurrentState(state);
                _coyoteTimeCooldown = 0;
            }
            else if (CurrentState is AirialState airialState &&
                    _coyoteTimeCooldown > 0)
            {
                airialState.ForceJump();
                _coyoteTimeCooldown = 0;
            }
            else
            {
                _jumpBufferCooldown = _configuration.JumpBuffer;
            }
        }

        public void JumpRelease()
        {
            if(_rigidbody.velocity.y > 0)
            {
                _rigidbody.AdjustVelocityY(_rigidbody.velocity.y * 0.5f);
            }
        }

        public void NotifyTouchingDirections(ITouchingDirections touchingDirections)
        {
            if (!touchingDirections.IsGrounded && CurrentState is GroundedState)
            {
                ChangeCurrentState(AirialState.CreateDefaultState(_rigidbody, _animator, _configuration));
            }
            else if (touchingDirections.IsGrounded && CurrentState is AirialState && ActiveChildState is not JumpState)
            {
                var groundedState = new GroundedState(_rigidbody, _animator, _configuration);
                ChangeCurrentState(groundedState);
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
            if (_coyoteTimeCooldown > 0)
            {
                _coyoteTimeCooldown -= Time.DeltaTime;
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

        protected void ChangeCurrentState(IState newState)
        {
            if(CurrentState is GroundedState &&
                newState is AirialState) 
            {
                _coyoteTimeCooldown = _configuration.CoyoteTimeBuffer;
            }
            CurrentState = newState;
            CurrentState.SetMovement(_movementInput);
            CurrentState.OnEnter();
        }
    }
}
