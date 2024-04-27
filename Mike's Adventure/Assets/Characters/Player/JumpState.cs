using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.GeneralScripts;
using UnityEngine;

namespace Assets.Characters.Player
{
    public class JumpState : IState
    {
        /// <seealso cref="PlayerController.MaxRunSpeed"/> 
        private const float MaxRunSpeed = 10.0f; // TODO: Make configurable from Inspector; And ensure keep consistent with GroundedMovementState

        /// <seealso cref="PlayerController.JumpImpulse"/>
        private const float JumpImpulse = 10f; // TODO: Make configurable from Inspector

        private readonly Rigidbody2D _rigidbody;
        private readonly IAnimator _animator;

        private Vector2 _movementInput;

        public JumpState(Rigidbody2D rigidbody, IAnimator animator)
        {
            _rigidbody = rigidbody;
            _animator = animator;

            ActiveChildState = this;
        }

        public IState ActiveChildState { get; }

        public bool CanJump => false;

        public void OnEnter()
        {
            _rigidbody.AdjustVelocityY(JumpImpulse);
        }

        public void Update()
        {
            _animator.Play(AnimationClipNames.Jump);
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
