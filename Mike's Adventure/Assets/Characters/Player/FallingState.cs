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
        /// <seealso cref="PlayerController.FallingGravityScale"/> 
        private const float FallingGravityScale = 2.0f; //TODO: Make configurable from Inspector
        
        private readonly ITime _time;
        private readonly Rigidbody2D _rigidbody;
        private readonly IAnimator _animator;

        private Vector2 _movementInput;
        private bool setFallingGravityScale = false;
        
        public FallingState(
            Rigidbody2D rigidbody,
            IAnimator animator,
            ITime time)
        {
            ActiveChildState = this;
            _time = time;
            _rigidbody = rigidbody;
            _animator = animator;
        }

        public IState ActiveChildState { get; }

        public bool CanJump => false;

        public void SetMovement(Vector2 movementInput)
        {
            _movementInput = movementInput;
        }

        public void OnEnter()
        {
            setFallingGravityScale = true;
        }

        public void Update()
        {
            _animator.Play(AnimationClipNames.Falling);
        }

        public void FixedUpdate()
        {
            if (setFallingGravityScale)
            {
                _rigidbody.gravityScale = 2.0f;
                setFallingGravityScale = false;
            }
            _rigidbody.AdjustVelocityX(_movementInput.x * MaxRunSpeed);
        }
    }
}
