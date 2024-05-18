using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.GeneralScripts;
using UnityEngine;

namespace Assets.Characters.Player
{
    public class CrouchIdleState : IState
    {
        private readonly Rigidbody2D _rigidbody;
        private readonly IAnimator _animator;
        private readonly PlayerConfiguration _configuration;
        private readonly IPlayerFacing _playerFacing;

        public CrouchIdleState(Rigidbody2D rigidbody, IAnimator animator, PlayerConfiguration configuration, IPlayerFacing playerFacing)
        {
            _rigidbody = rigidbody;
            _animator = animator;
            _configuration = configuration;
            _playerFacing = playerFacing;

            ActiveChildState = this;
        }

        public IState ActiveChildState { get; }

        public bool CanJump => true;

        public void OnEnter()
        {
            
        }

        public void Update()
        {
            _animator.Play(AnimationClipNames.CrouchIdle);
        }

        public void FixedUpdate()
        {
            if (_configuration.AccelerationBasedMovement)
            {
                // Player is actively counter-acting current momentum
                float targetVelocity = 0;
                float accelerationRate = _configuration.DeaccelerationRate;
                // Formula:
                // v_new = v_current + a * delta_t
                //     Substituting Sum(Forces) = m * a):
                // v_new = v_current + (Sum(Forces) / m) * delta_t
                // targetVelocity = v_current + (Sum(Forces) * delta_t) / m
                // ((targetVelocity - v_current) * m) * delta_t = Sum(Forces)
                float velocityDifference = targetVelocity - _rigidbody.velocity.x;
                var movementForce = (velocityDifference * _rigidbody.mass) / Time.fixedDeltaTime;
                _rigidbody.AddForce(movementForce * accelerationRate * Vector2.right, ForceMode2D.Force);
            }
            else
            {
                _rigidbody.AdjustVelocityX(0);
            }
        }

        public void SetMovement(Vector2 movementInput)
        {
            // Idle state only cares about changing player facing, due to input dead-zone:
            if (movementInput.x > 0 && !_playerFacing.IsFacingRight)
            {
                _playerFacing.IsFacingRight = true;
            }
            else if (movementInput.x < 0 && _playerFacing.IsFacingRight)
            {
                _playerFacing.IsFacingRight = false;
            }
        }
    }
}
