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

        public AirialState(Rigidbody2D rigidbody, ITime time) : base(new FallingState(rigidbody, time))
        {
            _rigidbody2 = rigidbody;
        }

        internal void Jump()
        {
            if(CurrentState.CanJump)
            {
                ChangeCurrentState(new JumpState(_rigidbody2));
            }
        }
    }
}
