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
        public AirialState(ITime time) : base(new FallingState(time))
        {

        }

        internal void Jump()
        {
            if(CurrentState.CanJump)
            {
                ChangeCurrentState(new JumpState());
            }
        }
    }
}
