using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Characters.Player
{
    public class AirialState : IState
    {
        public IState CurrentState { get; private set; } = new FallingState();
        public IState ActiveChildState => CurrentState.ActiveChildState;

        public void SetMovement(Vector2 movementInput)
        {
            // TODO: Do something with movement
        }

        internal void Jump()
        {
            CurrentState = new JumpState();
        }
    }
}
