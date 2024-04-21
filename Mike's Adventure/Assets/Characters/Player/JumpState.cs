using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Characters.Player
{
    public class JumpState : IState
    {
        public JumpState()
        {
            ActiveChildState = this;
        }

        public IState ActiveChildState { get; }

        public bool CanJump => false;

        public void OnEnter()
        {
            
        }

        public void Update()
        {
            
        }

        public void SetMovement(Vector2 movementInput)
        {
            // TODO: Do something with movement input
        }
    }
}
