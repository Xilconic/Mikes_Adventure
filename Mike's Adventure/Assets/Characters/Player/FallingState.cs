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
        /// <seealso cref="PlayerController.MaxRunSpeed"/> 
        private const float MaxRunSpeed = 10.0f; // TODO: Make configurable from Inspector; And ensure keep consistent with GroundedMovementState
        /// <seealso cref="PlayerController.CoyoteTimeBuffer"/> 
        private const float CoyoteTimeBuffer = 0.1f; // TODO: Make configurable from Inspector
        private float _coyoteTimeCooldown = 0;

        private readonly ITime _time;
        private readonly Rigidbody2D _rigidbody;

        private Vector2 _movementInput;
        

        public FallingState(
            Rigidbody2D rigidbody,
            ITime time)
        {
            ActiveChildState = this;
            _time = time;
            _rigidbody = rigidbody;
        }

        public IState ActiveChildState { get; }

        public bool CanJump => _coyoteTimeCooldown > 0;

        public void SetMovement(Vector2 movementInput)
        {
            _movementInput = movementInput;
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
            _rigidbody.AdjustVelocityX(_movementInput.x * MaxRunSpeed);
        }
    }
}
