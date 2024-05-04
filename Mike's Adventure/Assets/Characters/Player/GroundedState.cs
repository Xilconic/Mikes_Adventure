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
        private readonly IPlayerFacing _playerFacing;

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
            PlayerConfiguration configuration,
            IPlayerFacing playerFacing) : 
            base(new StandingState(rigidbody, animator, configuration, playerFacing))
        {
            _rigidbody = rigidbody;
            _animator = animator;
            _configuration = configuration;
            _playerFacing = playerFacing;
        }

        public override void SetMovement(Vector2 movementInput)
        {
            if (movementInput.y <= _configuration.CrouchInputZone ||
                (CurrentState is CrouchedState && IsTouchingCeiling))
            {
                ChangeCurrentState(new CrouchedState(_rigidbody, _animator, _configuration, _playerFacing));
            }
            else
            {
                ChangeCurrentState(new StandingState(_rigidbody, _animator, _configuration, _playerFacing));
            }

            base.SetMovement(movementInput);
        }

        public override void NotifyTouchingDirections(ITouchingDirections touchingDirections)
        {
            base.NotifyTouchingDirections(touchingDirections);
            if (CurrentState is CrouchedState crouchedState)
            {
                crouchedState.NotifyTouchingDirections(touchingDirections);
            }
        }
    }
}
