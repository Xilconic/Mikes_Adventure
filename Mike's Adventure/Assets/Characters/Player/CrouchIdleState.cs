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
        private readonly IPlayerFacing _playerFacing;

        public CrouchIdleState(Rigidbody2D rigidbody, IAnimator animator, IPlayerFacing playerFacing)
        {
            _rigidbody = rigidbody;
            _animator = animator;
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
            _rigidbody.AdjustVelocityX(0);
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
