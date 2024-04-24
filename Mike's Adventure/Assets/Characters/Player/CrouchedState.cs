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
        /// <seealso cref="PlayerController.CrouchLateralInputDeadZone"/>
        private const float CrouchLateralInputDeadZone = 0.1f; // TODO: Make configurable from Inspector
        private readonly Rigidbody2D _rigidBody;
        private readonly IAnimator _animator;
        private ITouchingDirections _touchingDirections;

        public CrouchedState(Rigidbody2D rigidbody, IAnimator animator) : base(new CrouchIdleState(rigidbody, animator))
        {
            _rigidBody = rigidbody;
            _animator = animator;
        }

        private bool IsTouchingCeiling => (!_touchingDirections?.IsOnCeiling) ?? true;

        public override bool CanJump => base.CanJump && IsTouchingCeiling;

        public override void SetMovement(Vector2 movementInput)
        {
            if (Mathf.Abs(movementInput.x) > CrouchLateralInputDeadZone)
            {
                ChangeCurrentState(new CrouchMovementState(_rigidBody, _animator));
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
