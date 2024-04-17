using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Characters.Player
{
    public class PlayerStateMachine
    {
        private Vector2 _movementInput;
        private ITouchingDirections _touchingDirections;

        public IState CurrentState { get; private set; } = new GroundedState();
        public object ActiveChildState => CurrentState.ActiveChildState;

        public void Jump()
        {
            if (!_touchingDirections?.IsOnCeiling ?? true)
            {
                var state = new AirialState();
                CurrentState = state;
                state.Jump();
            }
        }

        public void NotifyTouchingDirections(ITouchingDirections touchingDirections)
        {
            _touchingDirections = touchingDirections;

            if (!touchingDirections.IsGrounded && CurrentState is GroundedState)
            {
                CurrentState = new AirialState();
            }
            else if (touchingDirections.IsGrounded && CurrentState is AirialState)
            {
                CurrentState = new GroundedState();
                CurrentState.SetMovement(_movementInput);
            }
        }

        public void SetMovement(Vector2 movementInput)
        {
            _movementInput = movementInput;
            CurrentState.SetMovement(movementInput);
        }
    }
}
