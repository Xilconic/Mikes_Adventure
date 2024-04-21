using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Characters.Player
{
    public class GroundedState : IState
    {
        /// <seealso cref="PlayerController.CrouchInputZone"/>
        private const float CrouchVerticalInputThreshold = -0.5f;

        public IState CurrentState { get; private set; } = new StandingState();
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
            if (movementInput.y <= CrouchVerticalInputThreshold)
            {
                CurrentState = new CrouchedState();
                CurrentState.OnEnter();
            }
            else
            {
                CurrentState = new StandingState();
                CurrentState.OnEnter();
            }

            CurrentState.SetMovement(movementInput);
        }
    }
}
