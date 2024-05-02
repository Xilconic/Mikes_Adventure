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

        private AirialState(Rigidbody2D rigidbody, IAnimator animator, PlayerConfiguration configuration, IState childState) :
            base(childState)
        {
            _rigidbody = rigidbody;
            _animator = animator;
            _configuration = configuration;
        }

        public static AirialState CreateJumpingState(Rigidbody2D rigidbody, IAnimator animator, PlayerConfiguration configuration) =>
            new(rigidbody, animator, configuration, 
                new JumpState(rigidbody, animator, configuration));
        public static AirialState CreateDefaultState(Rigidbody2D rigidbody, IAnimator animator, PlayerConfiguration configuration) =>
            new(rigidbody, animator, configuration,
                new FallingState(rigidbody, animator, configuration));

        internal void Jump()
        {
            if(CurrentState.CanJump)
            {
                ForceJump();
            }
        }

        internal void ForceJump()
        {
            ChangeCurrentState(new JumpState(_rigidbody, _animator, _configuration));
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (_rigidbody.velocity.y <= 0)
            {
                if(_touchingDirections.IsOnWall)
                {
                    ChangeCurrentState(new WallSlideState(_rigidbody, _animator, _configuration));
                }
                else
                {
                    ChangeCurrentState(new FallingState(_rigidbody, _animator, _configuration));
                }
            }
        }
    }
}
