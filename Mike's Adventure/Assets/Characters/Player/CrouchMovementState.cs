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
        private readonly IPlayerFacing _playerFacing;

        private Vector2 _movementInput;

        public CrouchMovementState(Rigidbody2D rigidbody, IAnimator animator, PlayerConfiguration configuration, IPlayerFacing playerFacing)
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
            if(Mathf.Abs(_rigidbody.velocity.x) > _configuration.MaxCrouchWalkSpeed)
            {
                _animator.Play(AnimationClipNames.CrouchSlide);
            }
            else
            {
                _animator.Play(AnimationClipNames.CrouchWalk);
            }
        }

        public void FixedUpdate()
        {
            float targetVelocity = _movementInput.x * _configuration.MaxCrouchWalkSpeed;
            if (_configuration.AccelerationBasedMovement)
            {
                float accelerationRate;
                if (CurrentVelocityIsInSameDirectionAsPlayerInput)
                {
                    accelerationRate = _configuration.AccelerationRate;
                }
                else // Player is actively counter-acting current momentum
                {
                    targetVelocity = 0;
                    accelerationRate = _configuration.DeaccelerationRate;
                }
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
                _rigidbody.AdjustVelocityX(targetVelocity);
            }
        }

        private bool CurrentVelocityIsInSameDirectionAsPlayerInput =>
            Mathf.Sign(_movementInput.x) == Mathf.Sign(_rigidbody.velocity.x) ||
            Mathf.Abs(_rigidbody.velocity.x) < 0.01f; // If velocity of Mike is basically zero, than consider input being in same direction as current momentum

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
