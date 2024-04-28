using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.GeneralScripts;

namespace Assets.Characters.Player
{
    internal class CrouchedState : SuperState
    {
        private readonly Rigidbody2D _rigidBody;
        private readonly IAnimator _animator;
        private readonly PlayerConfiguration _configuration;
        private ITouchingDirections _touchingDirections;

        public CrouchedState(Rigidbody2D rigidbody, IAnimator animator, PlayerConfiguration configuration) : 
            base(new CrouchIdleState(rigidbody, animator))
        {
            _rigidBody = rigidbody;
            _animator = animator;
            _configuration = configuration;
        }

        private bool IsTouchingCeiling => (!_touchingDirections?.IsOnCeiling) ?? true;

        public override bool CanJump => base.CanJump && IsTouchingCeiling;

        public override void SetMovement(Vector2 movementInput)
        {
            if (Mathf.Abs(movementInput.x) > _configuration.CrouchLateralInputDeadZone)
            {
                ChangeCurrentState(new CrouchMovementState(_rigidBody, _animator, _configuration));
            }
            else
            {
                ChangeCurrentState(new CrouchIdleState(_rigidBody, _animator));
            }

            base.SetMovement(movementInput);
        }

        public void NotifyTouchingDirections(ITouchingDirections touchingDirections)
        {
            _touchingDirections = touchingDirections;
        }
    }
}
