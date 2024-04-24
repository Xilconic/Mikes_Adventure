﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Characters.Player
{
    public class CrouchIdleState : IState
    {
        private readonly Rigidbody2D _rigidbody;

        public CrouchIdleState(Rigidbody2D rigidbody)
        {
            _rigidbody = rigidbody;

            ActiveChildState = this;
        }

        public IState ActiveChildState { get; }

        public bool CanJump => true; // TODO: Determines from TouchingDirections.OnCeiling

        public void OnEnter()
        {
            
        }

        public void Update()
        {
            
        }

        public void FixedUpdate()
        {
            _rigidbody.AdjustVelocityX(0);
        }

        public void SetMovement(Vector2 movementInput)
        {
            // Idle state does not care about movement input
        }
    }
}
