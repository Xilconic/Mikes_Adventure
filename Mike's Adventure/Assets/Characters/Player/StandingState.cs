using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Characters.Player
{
    internal class StandingState : SuperState
    {
        private readonly Rigidbody2D _rigidbody;

        public StandingState(Rigidbody2D rigidbody) : base(new IdleState(rigidbody))
        {
            _rigidbody = rigidbody;
        }

        public override void SetMovement(Vector2 movementInput)
        {
            if (movementInput.x != 0)
            {
                ChangeCurrentState(new GroundMovementState(_rigidbody));
            }
            else
            {
                ChangeCurrentState(new IdleState(_rigidbody));
            }

            base.SetMovement(movementInput);
        }
    }
}
