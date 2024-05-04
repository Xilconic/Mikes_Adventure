using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.GeneralScripts;
using UnityEngine;

namespace Assets.Characters.Player
{
    public class WallSlideState : IState
    {
        private readonly Rigidbody2D _rigidbody;
        private readonly IAnimator _animator;
        private readonly PlayerConfiguration _configuration;
        private readonly IPlayerFacing _playerFacing;

        private Vector2 _movementInput;
        private bool _setWallSlideGavityScaling = false;

        public WallSlideState(Rigidbody2D rigidBody, IAnimator animator, PlayerConfiguration configuration, IPlayerFacing playerFacing)
        {
            _rigidbody = rigidBody;
            _animator = animator;
            _configuration = configuration;
            _playerFacing = playerFacing;
            ActiveChildState = this;
        }

        public IState ActiveChildState { get; }

        public bool CanJump => true;

        public void FixedUpdate()
        {
            if (_setWallSlideGavityScaling)
            {
                _rigidbody.gravityScale = _configuration.WallSlideGavityScaling;
                _setWallSlideGavityScaling = false;
            }

            var velocityX = _movementInput.x * _configuration.MaxRunSpeed;
            if (_rigidbody.velocity.y < _configuration.MaxWallSlideSpeed)
            {
                _rigidbody.velocity = new Vector2(velocityX, _configuration.MaxWallSlideSpeed);
            }
            else
            {
                _rigidbody.AdjustVelocityX(velocityX);
            }
            
        }

        public void OnEnter()
        {
            _setWallSlideGavityScaling = true;
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

        public void Update()
        {
            _animator.Play(AnimationClipNames.WallSlide);
        }
    }
}
