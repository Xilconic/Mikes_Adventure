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
        private readonly Rigidbody2D _rigidbody;
        private readonly IAnimator _animator;
        private readonly PlayerConfiguration _configuration;
        private Vector2 _movementInput;

        public CrouchMovementState(Rigidbody2D rigidbody, IAnimator animator, PlayerConfiguration configuration)
        {
            _rigidbody = rigidbody;
            _animator = animator;
            _configuration = configuration;

            ActiveChildState = this;
        }

        public IState ActiveChildState { get; }

        public bool CanJump => true;

        public void OnEnter()
        {
            
        }

        public void Update()
        {
            _animator.Play(AnimationClipNames.CrouchWalk);
        }

        public void FixedUpdate()
        {
            _rigidbody.AdjustVelocityX(_movementInput.x * _configuration.MaxCrouchWalkSpeed);
        }

        public void SetMovement(Vector2 movementInput)
        {
            _movementInput = movementInput;
        }
    }
}
