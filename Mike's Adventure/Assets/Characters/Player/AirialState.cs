using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.GeneralScripts;
using UnityEngine;

namespace Assets.Characters.Player
{
    public class AirialState : IState
    {
        private readonly ITime _time;

        public AirialState(ITime time)
        {
            _time = time;
            CurrentState = new FallingState(_time);
        }

        public IState CurrentState { get; private set; }
        public IState ActiveChildState => CurrentState.ActiveChildState;

        public bool CanJump => CurrentState.CanJump;

        public void OnEnter()
        {
            CurrentState.OnEnter();
        }

        public void Update()
        {
            CurrentState.Update();
        }

        public void SetMovement(Vector2 movementInput)
        {
            // TODO: Do something with movement
        }

        internal void Jump()
        {
            if(CurrentState.CanJump)
            {
                CurrentState = new JumpState();
                CurrentState.OnEnter();
            }
        }
    }
}
