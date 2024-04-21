using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Characters.Player
{
    public class GroundedState : SuperState
    {
        /// <seealso cref="PlayerController.CrouchInputZone"/>
        private const float CrouchVerticalInputThreshold = -0.5f;

        public GroundedState() : base(new StandingState())
        {
            
        }

        public override void SetMovement(Vector2 movementInput)
        {
            if (movementInput.y <= CrouchVerticalInputThreshold)
            {
                ChangeCurrentState(new CrouchedState());
            }
            else
            {
                ChangeCurrentState(new StandingState());
            }

            base.SetMovement(movementInput);
        }

        public void NotifyTouchingDirections(ITouchingDirections touchingDirections)
        {
            if (CurrentState is CrouchedState crouchedState)
            {
                crouchedState.NotifyTouchingDirections(touchingDirections);
            }
        }
    }
}
