using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.GeneralScripts;
using UnityEngine;

namespace Assets.Characters.Player
{
    public class GroundMovementState : IState
    {
        /// <seealso cref="PlayerController.MaxRunSpeed"/>
        private const float MaxRunSpeed = 10.0f; // TODO: Make configurable from inspector
        /// <seealso cref="PlayerController.MaxWalkSpeed"/>
        private const float MaxWalkSpeed = 5.0f; // TODO: Make configurable from inspector
        private readonly Rigidbody2D _rigidbody;
        private readonly IAnimator _animator;
        private Vector2 _movementInput;

        public GroundMovementState(Rigidbody2D rigidbody, IAnimator animator)
        {
            _rigidbody = rigidbody;
            _animator = animator;

            ActiveChildState = this;
        }

        public IState ActiveChildState { get; }

        public bool CanJump => true;

        public void OnEnter()
        {
            
        }

        public void Update()
        {
            if(Mathf.Abs(_rigidbody.velocity.x) <= MaxWalkSpeed)
            {
                _animator.Play(AnimationClipNames.Walk);
            }
            else
            {
                _animator.Play(AnimationClipNames.Jog);
            }
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
