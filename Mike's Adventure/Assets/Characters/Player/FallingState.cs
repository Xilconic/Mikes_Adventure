using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.GeneralScripts;

namespace Assets.Characters.Player
{
    public class FallingState : IState
    {
        private readonly ITime _time;

        /// <seealso cref="PlayerController.CoyoteTimeBuffer"/> 
        private const float CoyoteTimeBuffer = 0.1f; // TODO: Make configurable form Inspector
        private float _coyoteTimeCooldown = 0;

        public FallingState(ITime time)
        {
            ActiveChildState = this;
            _time = time;
        }

        public IState ActiveChildState { get; }

        public bool CanJump => _coyoteTimeCooldown > 0;

        public void SetMovement(Vector2 movementInput)
        {
            // TODO: Do something with movement
        }

        public void OnEnter()
        {
            _coyoteTimeCooldown = CoyoteTimeBuffer;
        }

        public void Update()
        {
            if(_coyoteTimeCooldown > 0)
            {
                _coyoteTimeCooldown -= _time.DeltaTime;
            }
        }

        public void FixedUpdate()
        {

        }
    }
}
