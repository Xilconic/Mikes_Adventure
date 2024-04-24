﻿using System;
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

        public AirialState(Rigidbody2D rigidbody, IAnimator animator, ITime time) : 
            base(new FallingState(rigidbody, animator, time))
        {
            _rigidbody2 = rigidbody;
            _animator = animator;
            _time = time;
        }

        internal void Jump()
        {
            if(CurrentState.CanJump)
            {
                ChangeCurrentState(new JumpState(_rigidbody2, _animator));
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
                ChangeCurrentState(new FallingState(_rigidbody2, _animator, _time));
            }
            base.FixedUpdate();
        }
    }
}
