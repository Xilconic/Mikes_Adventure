using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.GeneralScripts;
using UnityEngine;

namespace Assets.Characters.Player
{
    internal class StandingState : SuperState
    {
        private readonly Rigidbody2D _rigidbody;
        private readonly IAnimator _animator;
        private readonly PlayerConfiguration _configuration;
        private readonly IPlayerFacing _playerFacing;

        public StandingState(
            Rigidbody2D rigidbody,
            IAnimator animator,
            PlayerConfiguration configuration,
            IPlayerFacing playerFacing) : 
            base(new IdleState(rigidbody, animator))
        {
            _rigidbody = rigidbody;
            _animator = animator;
            _configuration = configuration;
            _playerFacing = playerFacing;
        }

        public override void SetMovement(Vector2 movementInput)
        {
            if (movementInput.x != 0)
            {
                ChangeCurrentState(new GroundMovementState(_rigidbody, _animator, _configuration, _playerFacing));
            }
            else
            {
                ChangeCurrentState(new IdleState(_rigidbody, _animator));
            }

            base.SetMovement(movementInput);
        }
    }
}
