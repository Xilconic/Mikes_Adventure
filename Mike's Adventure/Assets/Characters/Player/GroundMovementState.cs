using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Characters.Player
{
    public class GroundMovementState : IState
    {
        /// <seealso cref="PlayerController.MaxRunSpeed"/>
        private const float MaxRunSpeed = 10.0f; // TODO: Make configurable from inspector
        private readonly Rigidbody2D _rigidbody;
        private Vector2 _movementInput;

        public GroundMovementState(Rigidbody2D rigidbody)
        {
            _rigidbody = rigidbody;

            ActiveChildState = this;
        }

        public IState ActiveChildState { get; }

        public bool CanJump => true;

        public void OnEnter()
        {
            
        }

        public void Update()
        {

        }

        public void FixedUpdate()
        {
            _rigidbody.AdjustVelocityX(_movementInput.x * MaxRunSpeed);
        }

        public void SetMovement(Vector2 movementInput)
        {
            _movementInput = movementInput;
        }
    }
}
