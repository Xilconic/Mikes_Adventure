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
        private Vector2 _movementInput;
        private ITouchingDirections _touchingDirections;

        public ITime Time { get; set; } = new UnityTime();
        public IState CurrentState { get; private set; } = new GroundedState();
        public IState ActiveChildState => CurrentState.ActiveChildState;

        private bool IsTouchingCeiling => (!_touchingDirections?.IsOnCeiling) ?? true;

        public void Jump()
        {
            if (IsTouchingCeiling && CurrentState.CanJump)
            {
                var state = new AirialState(Time);
                CurrentState = state;
                CurrentState.OnEnter();
                state.Jump();
            }
        }

        public void NotifyTouchingDirections(ITouchingDirections touchingDirections)
        {
            _touchingDirections = touchingDirections;

            if (!touchingDirections.IsGrounded && CurrentState is GroundedState)
            {
                CurrentState = new AirialState(Time);
                CurrentState.OnEnter();
            }
            else if (touchingDirections.IsGrounded && CurrentState is AirialState)
            {
                CurrentState = new GroundedState();
                CurrentState.OnEnter();
                CurrentState.SetMovement(_movementInput);
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
        }
    }
}
