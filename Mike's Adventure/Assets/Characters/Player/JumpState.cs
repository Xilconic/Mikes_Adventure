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
        private readonly Rigidbody2D _rigidbody;
        private readonly IAnimator _animator;
        private readonly PlayerConfiguration _configuration;

        private Vector2 _movementInput;
        private bool shouldPerformJumpImpulse = false;
        private bool setJumpingGravityScale = false;

        public JumpState(Rigidbody2D rigidbody, IAnimator animator, PlayerConfiguration configuration)
        {
            _rigidbody = rigidbody;
            _animator = animator;
            _configuration = configuration;

            ActiveChildState = this;
        }

        public IState ActiveChildState { get; }

        public bool CanJump => false;

        public void OnEnter()
        {
            shouldPerformJumpImpulse = true;
            setJumpingGravityScale = true;
        }

        public void Update()
        {
            _animator.Play(AnimationClipNames.Jump);
        }

        public void FixedUpdate()
        {
            var velocityX = _movementInput.x * _configuration.MaxRunSpeed;
            if (setJumpingGravityScale)
            {
                _rigidbody.gravityScale = _configuration.JumpingGravityScale;
            }
            if (shouldPerformJumpImpulse)
            {
                _rigidbody.velocity = new Vector2(velocityX, _configuration.JumpImpulse);
                shouldPerformJumpImpulse = false;
            }
            else
            {
                _rigidbody.AdjustVelocityX(velocityX);
            }
        }

        public void SetMovement(Vector2 movementInput)
        {
            _movementInput = movementInput;
        }
    }
}
