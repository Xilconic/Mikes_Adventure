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
        private readonly IPlayerFacing _playerFacing;

        private Vector2 _movementInput;
        private bool _shouldPerformJumpImpulse = false;
        private bool _setJumpingGravityScale = false;

        public JumpState(Rigidbody2D rigidbody, IAnimator animator, PlayerConfiguration configuration, IPlayerFacing playerFacing)
        {
            _rigidbody = rigidbody;
            _animator = animator;
            _configuration = configuration;
            _playerFacing = playerFacing;

            ActiveChildState = this;
        }

        public IState ActiveChildState { get; }

        public bool CanJump => false;

        public void OnEnter()
        {
            _shouldPerformJumpImpulse = true;
            _setJumpingGravityScale = true;
        }

        public void Update()
        {
            _animator.Play(AnimationClipNames.Jump);
        }

        public void FixedUpdate()
        {
            var velocityX = _movementInput.x * _configuration.MaxRunSpeed;
            if (_setJumpingGravityScale)
            {
                _rigidbody.gravityScale = _configuration.JumpingGravityScale;
            }
            if (_shouldPerformJumpImpulse)
            {
                _rigidbody.velocity = new Vector2(velocityX, _configuration.JumpImpulse);
                _shouldPerformJumpImpulse = false;
            }
            else
            {
                _rigidbody.AdjustVelocityX(velocityX);
            }
        }

        public void SetMovement(Vector2 movementInput)
        {
            _movementInput = movementInput;

            if (_movementInput.x > 0 && !_playerFacing.IsFacingRight)
            {
                _playerFacing.IsFacingRight = true;
            }
            else if (_movementInput.x < 0 && _playerFacing.IsFacingRight)
            {
                _playerFacing.IsFacingRight = false;
            }
        }
    }
}
