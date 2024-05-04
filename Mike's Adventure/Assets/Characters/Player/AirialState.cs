using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.GeneralScripts;
using UnityEngine;

namespace Assets.Characters.Player
{
    public class AirialState : SuperState
    {
        private readonly Rigidbody2D _rigidbody;
        private readonly IAnimator _animator;
        private readonly PlayerConfiguration _configuration;
        private readonly IPlayerFacing _playerFacing;

        private AirialState(Rigidbody2D rigidbody, IAnimator animator, PlayerConfiguration configuration, IPlayerFacing playerFacing, IState childState) :
            base(childState)
        {
            _rigidbody = rigidbody;
            _animator = animator;
            _configuration = configuration;
            _playerFacing = playerFacing;
        }

        public static AirialState CreateJumpingState(Rigidbody2D rigidbody, IAnimator animator, PlayerConfiguration configuration, IPlayerFacing playerFacing) =>
            new(rigidbody, animator, configuration, playerFacing,
                new JumpState(rigidbody, animator, configuration, playerFacing));
        public static AirialState CreateDefaultState(Rigidbody2D rigidbody, IAnimator animator, PlayerConfiguration configuration, IPlayerFacing playerFacing) =>
            new(rigidbody, animator, configuration, playerFacing,
                new FallingState(rigidbody, animator, configuration, playerFacing));

        public AirialState CreateJumpingState()
        {
            if(ActiveChildState is WallSlideState)
            {
                return new AirialState(_rigidbody, _animator, _configuration, _playerFacing,
                    new WallJumpState(_rigidbody, _animator, _configuration, _playerFacing));
            }
            else
            {
                return CreateJumpingState(_rigidbody, _animator, _configuration, _playerFacing);
            }
        }

        internal void Jump()
        {
            if(CurrentState.CanJump)
            {
                if(CurrentState is WallSlideState)
                {
                    ChangeCurrentState(new WallJumpState(_rigidbody, _animator, _configuration, _playerFacing));
                }
                else
                {
                    ForceJump();
                }
            }
        }

        internal void ForceJump()
        {
            ChangeCurrentState(new JumpState(_rigidbody, _animator, _configuration, _playerFacing));
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (_rigidbody.velocity.y <= 0)
            {
                if(_touchingDirections.IsOnWall)
                {
                    ChangeCurrentState(new WallSlideState(_rigidbody, _animator, _configuration, _playerFacing));
                }
                else
                {
                    ChangeCurrentState(new FallingState(_rigidbody, _animator, _configuration, _playerFacing));
                }
            }
        }

        public override void NotifyTouchingDirections(ITouchingDirections touchingDirections)
        {
            base.NotifyTouchingDirections(touchingDirections);
            if(CurrentState is WallJumpState wallJumpState)
            {
                wallJumpState.NotifyTouchingDirection(touchingDirections);
            }
        }
    }
}
