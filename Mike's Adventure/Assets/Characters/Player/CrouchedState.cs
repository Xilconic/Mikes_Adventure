using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Characters.Player
{
    internal class CrouchedState : IState
    {
        /// <seealso cref="PlayerController.CrouchLateralInputDeadZone"/>
        private const float CrouchLateralInputDeadZone = 0.1f;

        private ITouchingDirections _touchingDirections;

        public IState CurrentState { get; private set; } = new CrouchIdleState();
        public IState ActiveChildState => CurrentState.ActiveChildState;

        private bool IsTouchingCeiling => (!_touchingDirections?.IsOnCeiling) ?? true;

        public bool CanJump => CurrentState.CanJump && IsTouchingCeiling;

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
            if (Mathf.Abs(movementInput.x) > CrouchLateralInputDeadZone)
            {
                CurrentState = new CrouchMovementState();
                CurrentState.OnEnter();
            }
            else
            {
                CurrentState = new CrouchIdleState();
                CurrentState.OnEnter();
            }

            CurrentState.SetMovement(movementInput);
        }

        public void NotifyTouchingDirections(ITouchingDirections touchingDirections)
        {
            _touchingDirections = touchingDirections;
        }
    }
}
