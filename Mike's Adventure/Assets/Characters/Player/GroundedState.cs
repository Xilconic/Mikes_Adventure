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
        private readonly Rigidbody2D _rigidbody;
        private readonly IAnimator _animator;
        private readonly PlayerConfiguration _configuration;
        private ITouchingDirections _touchingDirections;

        private bool IsTouchingCeiling
        {
            get
            {
                if (_touchingDirections != null)
                {
                    return _touchingDirections.IsOnCeiling;
                }
                else
                {
                    return false;
                }
            }
        }

        public GroundedState(
            Rigidbody2D rigidbody,
            IAnimator animator,
            PlayerConfiguration configuration) : 
            base(new StandingState(rigidbody, animator, configuration))
        {
            _rigidbody = rigidbody;
            _animator = animator;
            _configuration = configuration;
        }

        public override void SetMovement(Vector2 movementInput)
        {
            if (movementInput.y <= _configuration.CrouchInputZone ||
                (CurrentState is CrouchedState && IsTouchingCeiling))
            {
                ChangeCurrentState(new CrouchedState(_rigidbody, _animator, _configuration));
            }
            else
            {
                ChangeCurrentState(new StandingState(_rigidbody, _animator, _configuration));
            }

            base.SetMovement(movementInput);
        }

        public void NotifyTouchingDirections(ITouchingDirections touchingDirections)
        {
            _touchingDirections = touchingDirections;
            if (CurrentState is CrouchedState crouchedState)
            {
                crouchedState.NotifyTouchingDirections(touchingDirections);
            }
        }
    }
}
