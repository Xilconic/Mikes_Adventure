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
        private readonly Rigidbody2D _rigidbody;
        private readonly IAnimator _animator;
        private readonly PlayerConfiguration _configuration;

        private Vector2 _movementInput;
        private bool setFallingGravityScale = false;
        
        public FallingState(
            Rigidbody2D rigidbody,
            IAnimator animator,
            PlayerConfiguration configuration)
        {
            ActiveChildState = this;
            _rigidbody = rigidbody;
            _animator = animator;
            _configuration = configuration;
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
                _rigidbody.gravityScale = _configuration.FallingGravityScale;
                setFallingGravityScale = false;
            }
            _rigidbody.AdjustVelocityX(_movementInput.x * _configuration.MaxRunSpeed);
        }
    }
}
