using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Characters.Player
{
    public class FallingState : IState
    {
        public FallingState()
        {
            ActiveChildState = this;
        }

        public IState ActiveChildState { get; }

        public void SetMovement(Vector2 movementInput)
        {
            // TODO: Do something with movement
        }
    }
}
