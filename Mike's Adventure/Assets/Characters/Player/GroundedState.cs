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
        private const float CrouchVerticalInputThreshold = -0.5f; // TODO: Make configurable from Inspector
        private readonly Rigidbody2D _rigidbody;

        public GroundedState(Rigidbody2D rigidbody) : base(new StandingState(rigidbody))
        {
            _rigidbody = rigidbody;
        }

        public override void SetMovement(Vector2 movementInput)
        {
            if (movementInput.y <= CrouchVerticalInputThreshold)
            {
                ChangeCurrentState(new CrouchedState(_rigidbody));
            }
            else
            {
                ChangeCurrentState(new StandingState(_rigidbody));
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
