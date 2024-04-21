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
        IState ActiveChildState { get; }

        /// <summary>
        /// Indicates if Jumping is allowed or not.
        /// </summary>
        bool CanJump { get; }

        void SetMovement(Vector2 movementInput);

        /// <summary>
        /// Method intended to be called when this state is entered.
        /// </summary>
        void OnEnter();

        /// <summary>
        /// Method intended to be called inside <c>MonoBehavior.Update()</c>.
        /// </summary>
        /// <seealse cref="MonoBehaviour.Update"/>
        void Update();

        /// <summary>
        /// Method intended to be called inside <c>MonoBehavior.FixedUpdate()</c>.
        /// </summary>
        /// <seealse cref="MonoBehaviour.FixedUpdate"/>
        void FixedUpdate();
    }
}
