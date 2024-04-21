using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Characters.Player
{
    /// <summary>
    /// Represents a state that internally has it's own state machine.
    /// </summary>
    public abstract class SuperState : IState
    {
        public SuperState(IState initialState)
        {
            CurrentState = initialState;
        }

        public IState CurrentState { get; private set; }
        public IState ActiveChildState => CurrentState.ActiveChildState;

        public virtual bool CanJump => CurrentState.CanJump;

        public void OnEnter()
        {
            CurrentState.OnEnter();
        }

        public virtual void SetMovement(Vector2 movementInput)
        {
            CurrentState.SetMovement(movementInput);
        }

        public void Update()
        {
            CurrentState.Update();
        }

        protected void ChangeCurrentState(IState newState)
        {
            CurrentState = newState;
            CurrentState.OnEnter();
        }
    }
}
