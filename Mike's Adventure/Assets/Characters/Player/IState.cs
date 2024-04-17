using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Characters.Player
{
    public interface IState
    {
        IState ActiveChildState { get;}

        void SetMovement(Vector2 movementInput);
    }
}
