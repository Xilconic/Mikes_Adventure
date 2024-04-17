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
        public IState CurrentState { get; private set; } = new GroundedState();
        public object ActiveChildState => CurrentState.ActiveChildState;

        public void SetMovement(Vector2 movementInput)
        {
            CurrentState.SetMovement(movementInput);
        }
    }
}
