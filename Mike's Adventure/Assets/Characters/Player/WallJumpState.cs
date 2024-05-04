using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.GeneralScripts;
using UnityEngine;

namespace Assets.Characters.Player
{
    public class WallJumpState : IState
    {
        private readonly Rigidbody2D _rigidbody;
        private readonly IAnimator _animator;
        private readonly PlayerConfiguration _configuration;
        private readonly IPlayerFacing _playerFacing;

        private ITouchingDirections _touchingDirections;
        private bool _shouldPerformJumpImpulse = false;
        private bool _setJumpingGravityScale = false;
        private WallDirection _jumpedOffWall;

        public WallJumpState(Rigidbody2D rigidbody, IAnimator animator, PlayerConfiguration configuration, IPlayerFacing playerFacing)
        {
            _rigidbody = rigidbody;
            _animator = animator;
            _configuration = configuration;
            _playerFacing = playerFacing;

            ActiveChildState = this;
        }

        public IState ActiveChildState { get; }

        public bool CanJump => false;

        public void FixedUpdate()
        {
            if (_setJumpingGravityScale)
            {
                _rigidbody.gravityScale = _configuration.JumpingGravityScale;
                _setJumpingGravityScale = false;
            }
            if (_shouldPerformJumpImpulse)
            {
                float direction = 0f;
                switch (_touchingDirections.WallDirection)
                {
                    case WallDirection.Left:
                        _playerFacing.IsFacingRight = true;
                        direction = 1f;
                        break;
                    case WallDirection.Right:
                        _playerFacing.IsFacingRight = false;
                        direction = -1f;
                        break;
                }
                _rigidbody.velocity = new Vector2(direction * _configuration.JumpImpulse * 0.707f, _configuration.JumpImpulse * 0.707f);
                _shouldPerformJumpImpulse = false;
            }
        }

        public void OnEnter()
        {
            _shouldPerformJumpImpulse = true;
            _setJumpingGravityScale = true;
        }

        public void SetMovement(Vector2 movementInput)
        {
            
        }

        public void Update()
        {
            _animator.Play(AnimationClipNames.Jump);
        }

        internal void NotifyTouchingDirection(ITouchingDirections touchingDirections)
        {
            _touchingDirections = touchingDirections;
        }
    }
}
