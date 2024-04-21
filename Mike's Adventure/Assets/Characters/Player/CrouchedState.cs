using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Characters.Player
{
    internal class CrouchedState : SuperState
    {
        /// <seealso cref="PlayerController.CrouchLateralInputDeadZone"/>
        private const float CrouchLateralInputDeadZone = 0.1f;
        private ITouchingDirections _touchingDirections;

        public CrouchedState() : base(new CrouchIdleState())
        {
            
        }

        private bool IsTouchingCeiling => (!_touchingDirections?.IsOnCeiling) ?? true;

        public override bool CanJump => base.CanJump && IsTouchingCeiling;

        public override void SetMovement(Vector2 movementInput)
        {
            if (Mathf.Abs(movementInput.x) > CrouchLateralInputDeadZone)
            {
                ChangeCurrentState(new CrouchMovementState());
            }
            else
            {
                ChangeCurrentState(new CrouchIdleState());
            }

            base.SetMovement(movementInput);
        }

        public void NotifyTouchingDirections(ITouchingDirections touchingDirections)
        {
            _touchingDirections = touchingDirections;
        }
    }
}
