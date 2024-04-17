using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Characters.Player
{
    internal class StandingState : IState
    {
        public IState CurrentState { get; private set; } = new IdleState();
        public IState ActiveChildState => CurrentState.ActiveChildState;

        public void SetMovement(Vector2 movementInput)
        {
            if (movementInput.x != 0)
            {
                CurrentState = new GroundMovementState();
            }
            else
            {
                CurrentState = new IdleState();
            }
        }
    }
}
