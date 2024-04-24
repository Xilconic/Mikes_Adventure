using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.GeneralScripts;
using UnityEngine;

namespace Assets.Characters.Player
{
    public class CrouchMovementState : IState
    {
        /// <seealso cref="PlayerController.MaxChrouchWalkSpeed"/>
        private const float MaxRunSpeed = 3.0f; // TODO: Make configurable from inspector
        private readonly Rigidbody2D _rigidbody;
        private readonly IAnimator _animator;
        private Vector2 _movementInput;

        public CrouchMovementState(Rigidbody2D rigidbody, IAnimator animator)
        {
            _rigidbody = rigidbody;
            _animator = animator;

            ActiveChildState = this;
        }

        public IState ActiveChildState { get; }

        public bool CanJump => true; // TODO: Determines from TouchingDirections.OnCeiling

        public void OnEnter()
        {
            
        }

        public void Update()
        {
            _animator.Play(AnimationClipNames.CrouchWalk);
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
