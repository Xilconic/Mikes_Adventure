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
        private readonly ITime _time;

        public AirialState(Rigidbody2D rigidbody, ITime time) : base(new FallingState(rigidbody, time))
        {
            _rigidbody2 = rigidbody;
            _time = time;
        }

        internal void Jump()
        {
            if(CurrentState.CanJump)
            {
                ChangeCurrentState(new JumpState(_rigidbody2));
            }
        }

        internal void JumpRelease()
        {
            if(CurrentState is JumpState jumpState)
            {
                jumpState.JumpRelease();
            }
        }

        public override void FixedUpdate()
        {
            if(_rigidbody2.velocity.y <= 0)
            {
                ChangeCurrentState(new FallingState(_rigidbody2, _time));
            }
            base.FixedUpdate();
        }
    }
}
