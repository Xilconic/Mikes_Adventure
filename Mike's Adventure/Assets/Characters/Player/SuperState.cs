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
        private Vector2 _movementInput;

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
            _movementInput = movementInput;
            CurrentState.SetMovement(movementInput);
        }

        public void Update()
        {
            CurrentState.Update();
        }

        public virtual void FixedUpdate()
        {
            CurrentState.FixedUpdate();
        }

        protected void ChangeCurrentState(IState newState)
        {
            CurrentState = newState;
            CurrentState.SetMovement(_movementInput);
            CurrentState.OnEnter();
        }
    }
}
