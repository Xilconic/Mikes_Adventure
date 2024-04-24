using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.GeneralScripts;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Characters.Player
{
    public class GroundedState : SuperState
    {
        /// <seealso cref="PlayerController.CrouchInputZone"/>
        private const float CrouchVerticalInputThreshold = -0.5f; // TODO: Make configurable from Inspector
        private readonly Rigidbody2D _rigidbody;
        private readonly IAnimator _animator;

        public GroundedState(
            Rigidbody2D rigidbody,
            IAnimator animator) : 
            base(new StandingState(rigidbody, animator))
        {
            _rigidbody = rigidbody;
            _animator = animator;
        }

        public override void SetMovement(Vector2 movementInput)
        {
            if (movementInput.y <= CrouchVerticalInputThreshold)
            {
                ChangeCurrentState(new CrouchedState(_rigidbody, _animator));
            }
            else
            {
                ChangeCurrentState(new StandingState(_rigidbody, _animator));
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
