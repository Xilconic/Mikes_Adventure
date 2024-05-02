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

        private Vector2 _movementInput;
        private bool setWallSlideGavityScaling = false;

        public WallSlideState(Rigidbody2D rigidBody, IAnimator animator, PlayerConfiguration configuration)
        {
            _rigidbody = rigidBody;
            _animator = animator;
            _configuration = configuration;
            ActiveChildState = this;
        }

        public IState ActiveChildState { get; }

        public bool CanJump => false;

        public void FixedUpdate()
        {
            if (setWallSlideGavityScaling)
            {
                _rigidbody.gravityScale = _configuration.WallSlideGavityScaling;
                setWallSlideGavityScaling = false;
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
            setWallSlideGavityScaling = true;
        }

        public void SetMovement(Vector2 movementInput)
        {
            _movementInput = movementInput;
        }

        public void Update()
        {
            _animator.Play(AnimationClipNames.WallSlide);
        }
    }
}
