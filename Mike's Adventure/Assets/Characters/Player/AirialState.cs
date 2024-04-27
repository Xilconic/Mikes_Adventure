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
        private readonly Rigidbody2D _rigidbody2;
        private readonly IAnimator _animator;
        private readonly ITime _time;

        private AirialState(Rigidbody2D rigidbody, IAnimator animator, ITime time, IState childState) :
            base(childState)
        {
            _rigidbody2 = rigidbody;
            _animator = animator;
            _time = time;
        }

        public static AirialState CreateJumpingState(Rigidbody2D rigidbody, IAnimator animator, ITime time) =>
            new AirialState(rigidbody, animator, time, new JumpState(rigidbody, animator));
        public static AirialState CreateDefaultState(Rigidbody2D rigidbody, IAnimator animator, ITime time) =>
            new AirialState(rigidbody, animator, time, new FallingState(rigidbody, animator, time));

        internal void Jump()
        {
            if(CurrentState.CanJump)
            {
                ForceJump();
            }
        }

        internal void ForceJump()
        {
            ChangeCurrentState(new JumpState(_rigidbody2, _animator));
        }

        public override void FixedUpdate()
        {
            if(_rigidbody2.velocity.y <= 0)
            {
                ChangeCurrentState(new FallingState(_rigidbody2, _animator, _time));
            }
            base.FixedUpdate();
        }
    }
}
