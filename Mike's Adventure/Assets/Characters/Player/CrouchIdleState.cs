using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Characters.Player
{
    public class CrouchIdleState : IState
    {
        public CrouchIdleState()
        {
            ActiveChildState = this;
        }

        public IState ActiveChildState { get; }

        public void SetMovement(Vector2 movementInput)
        {
            // Idle state does not care about movement input
        }
    }
}
