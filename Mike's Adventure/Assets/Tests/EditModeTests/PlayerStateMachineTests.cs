﻿using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assets.Characters.Player;
using Assets.Tests;

public class PlayerStateMachineTests
{   protected PlayerStateMachine _sut;

    [SetUp]
    public virtual void SetUp()
    {
        _sut = new PlayerStateMachine();
    }

    public class GivenNewInstance : PlayerStateMachineTests
    {
        [Test]
        public void ThenCurrentStateIsGroundedState()
        {
            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
        }

        [Test]
        public void ThenActiveChildStateIsIdleState()
        {

            Assert.IsInstanceOf<IdleState>(_sut.ActiveChildState);
        }
    }

    public class GivenActiveChildStateIsIdleState : PlayerStateMachineTests
    {
        public override void SetUp()
        {
            base.SetUp();

            Assert.IsInstanceOf<IdleState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenSettingZeroMovementInput_ThenCurrentStateIsGroundedStateAndActiveChildStateIsIdleState()
        {
            var movementInput = Vector2.zero;

            _sut.SetMovement(movementInput);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<IdleState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenSettingUpwardMovementInput_ThenCurrentStateIsGroundedStateAndActiveChildStateIsIdleState()
        {
            var movementInput = Vector2.up;

            _sut.SetMovement(movementInput);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<IdleState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenSettingNonZeroMovementInput_ThenCurrentStateIsGroundedStateAndActiveChildStateIsGroundMovementState()
        {
            var movementInput = Vector2.left;

            _sut.SetMovement(movementInput);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<GroundMovementState>(_sut.ActiveChildState);
        }

        [Test]
        [TestCase(-0.01f)]
        [TestCase(-0.49f)]
        public void WhenSettingPartialDownMovementInputBeforeThreshold_ThenCurrentStateIsGroundedStateAndActiveChildStateIsIdleState(
            float yValue)
        {
            var movementInput = new Vector2(0, yValue);

            _sut.SetMovement(movementInput);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<IdleState>(_sut.ActiveChildState);
        }

        [Test]
        [TestCase(-0.50f)]
        [TestCase(-1.0f)]
        public void WhenSettingPartialDownMovementInputOnOrBeyondThreshold_ThenCurrentStateIsGroundedStateAndActiveChildStateIsCrouchIdleState(
            float yValue)
        {
            var movementInput = new Vector2(0, yValue);

            _sut.SetMovement(movementInput);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<CrouchIdleState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenSettingSignificantDownMovementAndWithInsignificantLateralMovement_ThenCurrentStateIsGroundedStateAndActiveChildStateIsCrouchIdleState(
            [Values(0f, 0.1f, -0.1f)] float xValue,
            [Values(-0.5f, -.8f)] float yValue)
        {
            var movementInput = new Vector2(xValue, yValue);

            _sut.SetMovement(movementInput);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<CrouchIdleState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenSettingSignificantDownMovementAndSignificantLateralMovement_ThenCurrentStateIsGroundedStateAndActiveChildStateIsCrouchMovementState(
            [Values(0.11f, 0.707f, -0.11f, -0.707f)] float xValue,
            [Values(-0.5f, -.707f)] float yValue)
        {
            var movementInput = new Vector2(xValue, yValue);

            _sut.SetMovement(movementInput);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<CrouchMovementState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenRemainsGrounded_ThenStateRemainsUnchanged()
        {
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = true,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<IdleState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenStartsToFall_ThenCurrentStateIsAirialStateAndActiveChildStateIsFallingState()
        {
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = false,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<FallingState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenJumpButtonPressed_ThenCurrentStateIsAirialStateAndActiveChildStateIsJumpState()
        {
            _sut.Jump();

            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<JumpState>(_sut.ActiveChildState);
        }
    }

    public class GivenActiveChildStateIsGroundMovementState : PlayerStateMachineTests
    {
        public override void SetUp()
        {
            base.SetUp();

            var movementInput = Vector2.left;

            _sut.SetMovement(movementInput);
            Assert.IsInstanceOf<GroundMovementState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenSettingZeroMovementInput_ThenCurrentStateIsGroundedStateAndActiveChildStateIsIdleState()
        {
            _sut.SetMovement(Vector2.zero);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<IdleState>(_sut.ActiveChildState);
        }

        [Test]
        [TestCase(-0.01f)]
        [TestCase(-0.49f)]
        public void WhenSettingPartialDownMovementInputBeforeThreshold_ThenCurrentStateIsGroundedStateAndActiveChildStateIsIdleState(
            float yValue)
        {
            var movementInput = new Vector2(0, yValue);

            _sut.SetMovement(movementInput);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<IdleState>(_sut.ActiveChildState);
        }

        [Test]
        [TestCase(-0.01f)]
        [TestCase(-0.49f)]
        public void WhenSettingPartialDownMovementInputBeforeThresholdAndWithLateralComponent_ThenCurrentStateIsGroundedStateAndActiveChildStateIsGroundMovementState(
            float yValue)
        {
            var movementInput = new Vector2(0.2f, yValue);

            _sut.SetMovement(movementInput);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<GroundMovementState>(_sut.ActiveChildState);
        }

        [Test]
        [TestCase(-0.50f)]
        [TestCase(-1.0f)]
        public void WhenSettingPartialDownMovementInputOnOrBeyondThreshold_ThenCurrentStateIsGroundedStateAndActiveChildStateIsCrouchIdleState(
            float yValue)
        {
            var movementInput = new Vector2(0, yValue);

            _sut.SetMovement(movementInput);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<CrouchIdleState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenSettingSignificantDownMovementAndWithInsignificantLateralMovement_ThenCurrentStateIsGroundedStateAndActiveChildStateIsCrouchIdleState(
            [Values(0f, 0.1f, -0.1f)] float xValue,
            [Values(-0.5f, -.8f)] float yValue)
        {
            var movementInput = new Vector2(xValue, yValue);

            _sut.SetMovement(movementInput);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<CrouchIdleState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenSettingSignificantDownMovementAndSignificantLateralMovement_ThenCurrentStateIsGroundedStateAndActiveChildStateIsCrouchMovementState(
            [Values(0.11f, 0.707f, -0.11f, -0.707f)] float xValue,
            [Values(-0.5f, -.707f)] float yValue)
        {
            var movementInput = new Vector2(xValue, yValue);

            _sut.SetMovement(movementInput);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<CrouchMovementState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenRemainsGrounded_ThenStateRemainsUnchanged()
        {
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = true,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<GroundMovementState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenStartsToFall_ThenCurrentStateIsAirialStateAndActiveChildStateIsFallingState()
        {
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = false,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<FallingState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenJumpButtonPressed_ThenCurrentStateIsAirialStateAndActiveChildStateIsJumpState()
        {
            _sut.Jump();

            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<JumpState>(_sut.ActiveChildState);
        }
    }

    public class GivenActiveChildStateIsCrouchIdelState : PlayerStateMachineTests
    {
        public override void SetUp()
        {
            base.SetUp();

            var movementInput = Vector2.down;

            _sut.SetMovement(movementInput);
            Assert.IsInstanceOf<CrouchIdleState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenSettingZeroMovementInput_ThenCurrentStateIsGroundedStateAndActiveChildStateIsIdleState()
        {
            _sut.SetMovement(Vector2.zero);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<IdleState>(_sut.ActiveChildState);
        }

        [Test]
        [TestCase(-0.01f)]
        [TestCase(-0.49f)]
        public void WhenSettingPartialDownMovementInputBeforeThreshold_ThenCurrentStateIsGroundedStateAndActiveChildStateIsIdleState(
            float yValue)
        {
            var movementInput = new Vector2(0, yValue);

            _sut.SetMovement(movementInput);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<IdleState>(_sut.ActiveChildState);
        }

        [Test]
        [TestCase(-0.01f)]
        [TestCase(-0.49f)]
        public void WhenSettingPartialDownMovementInputBeforeThresholdAndWithLateralComponent_ThenCurrentStateIsGroundedStateAndActiveChildStateIsGroundMovementState(
            float yValue)
        {
            var movementInput = new Vector2(0.2f, yValue);

            _sut.SetMovement(movementInput);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<GroundMovementState>(_sut.ActiveChildState);
        }

        [Test]
        [TestCase(-0.50f)]
        [TestCase(-1.0f)]
        public void WhenSettingPartialDownMovementInputOnOrBeyondThreshold_ThenCurrentStateIsGroundedStateAndActiveChildStateIsCrouchIdleState(
            float yValue)
        {
            var movementInput = new Vector2(0, yValue);

            _sut.SetMovement(movementInput);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<CrouchIdleState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenSettingSignificantDownMovementAndWithInsignificantLateralMovement_ThenCurrentStateIsGroundedStateAndActiveChildStateIsCrouchIdleState(
            [Values(0f, 0.1f, -0.1f)] float xValue,
            [Values(-0.5f, -.8f)] float yValue)
        {
            var movementInput = new Vector2(xValue, yValue);

            _sut.SetMovement(movementInput);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<CrouchIdleState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenSettingSignificantDownMovementAndSignificantLateralMovement_ThenCurrentStateIsGroundedStateAndActiveChildStateIsCrouchMovementState(
            [Values(0.11f, 0.707f, -0.11f, -0.707f)] float xValue,
            [Values(-0.5f, -.707f)] float yValue)
        {
            var movementInput = new Vector2(xValue, yValue);

            _sut.SetMovement(movementInput);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<CrouchMovementState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenRemainsGrounded_ThenStateRemainsUnchanged()
        {
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = true,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<CrouchIdleState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenStartsToFall_ThenCurrentStateIsAirialStateAndActiveChildStateIsFallingState()
        {
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = false,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<FallingState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenJumpButtonPressed_ThenCurrentStateIsAirialStateAndActiveChildStateIsJumpState()
        {
            _sut.Jump();

            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<JumpState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenJumpButtonPressedWhileOnCeiling_ThenStateRemainsUnchanged()
        {
            var touchingDirections = new TouchingDirectionsStub
            {
                IsOnCeiling = true
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            _sut.Jump();

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<CrouchIdleState>(_sut.ActiveChildState);
        }
    }

    public class GivenActiveChildStateIsCrouchMovementState : PlayerStateMachineTests
    {
        public override void SetUp()
        {
            base.SetUp();

            var movementInput = new Vector2(0.707f, -0.707f); // 45 degrees down right

            _sut.SetMovement(movementInput);
            Assert.IsInstanceOf<CrouchMovementState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenSettingZeroMovementInput_ThenCurrentStateIsGroundedStateAndActiveChildStateIsIdleState()
        {
            _sut.SetMovement(Vector2.zero);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<IdleState>(_sut.ActiveChildState);
        }

        [Test]
        [TestCase(-0.01f)]
        [TestCase(-0.49f)]
        public void WhenSettingPartialDownMovementInputBeforeThreshold_ThenCurrentStateIsGroundedStateAndActiveChildStateIsIdleState(
            float yValue)
        {
            var movementInput = new Vector2(0, yValue);

            _sut.SetMovement(movementInput);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<IdleState>(_sut.ActiveChildState);
        }

        [Test]
        [TestCase(-0.01f)]
        [TestCase(-0.49f)]
        public void WhenSettingPartialDownMovementInputBeforeThresholdAndWithLateralComponent_ThenCurrentStateIsGroundedStateAndActiveChildStateIsGroundMovementState(
            float yValue)
        {
            var movementInput = new Vector2(0.2f, yValue);

            _sut.SetMovement(movementInput);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<GroundMovementState>(_sut.ActiveChildState);
        }

        [Test]
        [TestCase(-0.50f)]
        [TestCase(-1.0f)]
        public void WhenSettingPartialDownMovementInputOnOrBeyondThreshold_ThenCurrentStateIsGroundedStateAndActiveChildStateIsCrouchIdleState(
            float yValue)
        {
            var movementInput = new Vector2(0, yValue);

            _sut.SetMovement(movementInput);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<CrouchIdleState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenSettingSignificantDownMovementAndWithInsignificantLateralMovement_ThenCurrentStateIsGroundedStateAndActiveChildStateIsCrouchIdleState(
            [Values(0f, 0.1f, -0.1f)] float xValue,
            [Values(-0.5f, -.8f)] float yValue)
        {
            var movementInput = new Vector2(xValue, yValue);

            _sut.SetMovement(movementInput);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<CrouchIdleState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenSettingSignificantDownMovementAndSignificantLateralMovement_ThenCurrentStateIsGroundedStateAndActiveChildStateIsCrouchMovementState(
            [Values(0.11f, 0.707f, -0.11f, -0.707f)] float xValue,
            [Values(-0.5f, -.707f)] float yValue)
        {
            var movementInput = new Vector2(xValue, yValue);

            _sut.SetMovement(movementInput);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<CrouchMovementState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenRemainsGrounded_ThenStateRemainsUnchanged()
        {
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = true,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<CrouchMovementState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenStartsToFall_ThenCurrentStateIsAirialStateAndActiveChildStateIsFallingState()
        {
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = false,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<FallingState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenJumpButtonPressed_ThenCurrentStateIsAirialStateAndActiveChildStateIsJumpState()
        {
            _sut.Jump();

            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<JumpState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenJumpButtonPressedWhileOnCeiling_ThenStateRemainsUnchanged()
        {
            var touchingDirections = new TouchingDirectionsStub
            {
                IsOnCeiling = true
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            _sut.Jump();

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<CrouchMovementState>(_sut.ActiveChildState);
        }
    }

    public class GivenActiveChildStateIsFallingState : PlayerStateMachineTests
    {
        public override void SetUp()
        {
            base.SetUp();

            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = false,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            Assert.IsInstanceOf<FallingState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenStillFalling_ThenCurrentStateIsAirialStateAndActiveChildStateIsFallingState()
        {
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = false,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<FallingState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenHittingGround_ThenCurrentStateIsGroundedStateAndActiveChildStateIsIdleState()
        {
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = true,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<IdleState>(_sut.ActiveChildState);
        }

        [Test]
        [TestCase(-0.01f)]
        [TestCase(-0.49f)]
        public void WhenHittingGroundWithNonSignificantDownwardMovementInput_ThenCurrentStateIsGroundedStateAndActiveChildStateIsIdleState(
            float yValue)
        {
            _sut.SetMovement(new Vector2(0, yValue));
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = true,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<IdleState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenHittingGroundWithNonZeroLateralMovementInput_ThenCurrentStateIsGroundedStateAndActiveChildStateIsIdleState()
        {
            _sut.SetMovement(Vector2.right);
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = true,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<GroundMovementState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenHittingGroundWithSignificantDownMovementAndWithInsignificantLateralMovement_ThenCurrentStateIsGroundedStateAndActiveChildStateIsCrouchIdleState(
            [Values(0f, 0.1f, -0.1f)] float xValue,
            [Values(-0.5f, -.8f)] float yValue)
        {
            _sut.SetMovement(new Vector2(xValue, yValue));

            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = true,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<CrouchIdleState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenHittingGroundWithSignificantDownMovementAndSignificantLateralMovement_ThenCurrentStateIsGroundedStateAndActiveChildStateIsCrouchMovementState(
            [Values(0.11f, 0.707f, -0.11f, -0.707f)] float xValue,
            [Values(-0.5f, -.707f)] float yValue)
        {
            _sut.SetMovement(new Vector2(xValue, yValue));

            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = true,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<CrouchMovementState>(_sut.ActiveChildState);
        }

        // TODO: Introduce interface abstracting UnityEngine.Time static methods, implementation based on that class, and another Mock implementation for unit testing
        // TODO: Coyote time: cooldown active + jump => Go into Jump State
        // TODO: Coyoted time: cooldown inactive + jump => Remain in current state
        // TODO: Jump input buffer: Jump, wait small time, Hit ground => Go into Jump State
        // TODO: Jump input buffer: Jump, wait, Hit ground => remain in Idle state
    }

    public class GivenActiveChildStateIsJumpState : PlayerStateMachineTests
    {
        public override void SetUp()
        {
            base.SetUp();

            _sut.Jump();

            Assert.IsInstanceOf<JumpState>(_sut.ActiveChildState);
        }

        // TODO: velocity.y < 0 => Transition into falling
        // TODO: Jumping does not again set y velocity
        // TODO: Jump input buffer: Jump, wait small time, Hit ground => Go into Jump State
        // TODO: Jump input buffer: Jump, wait, Hit ground => remain in Idle state
        // TODO: Release Jump: Half vertical velocity

        [Test]
        public void WhenStillInTheAir_ThenCurrentStateIsAirialStateAndActiveChildStateIsFallingState()
        {
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = false,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<JumpState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenHittingGround_ThenCurrentStateIsGroundedStateAndActiveChildStateIsIdleState()
        {
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = true,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<IdleState>(_sut.ActiveChildState);
        }

        [Test]
        [TestCase(-0.01f)]
        [TestCase(-0.49f)]
        public void WhenHittingGroundWithNonSignificantDownwardMovementInput_ThenCurrentStateIsGroundedStateAndActiveChildStateIsIdleState(
            float yValue)
        {
            _sut.SetMovement(new Vector2(0, yValue));
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = true,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<IdleState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenHittingGroundWithNonZeroLateralMovementInput_ThenCurrentStateIsGroundedStateAndActiveChildStateIsIdleState()
        {
            _sut.SetMovement(Vector2.right);
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = true,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<GroundMovementState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenHittingGroundWithSignificantDownMovementAndWithInsignificantLateralMovement_ThenCurrentStateIsGroundedStateAndActiveChildStateIsCrouchIdleState(
            [Values(0f, 0.1f, -0.1f)] float xValue,
            [Values(-0.5f, -.8f)] float yValue)
        {
            _sut.SetMovement(new Vector2(xValue, yValue));

            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = true,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<CrouchIdleState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenHittingGroundWithSignificantDownMovementAndSignificantLateralMovement_ThenCurrentStateIsGroundedStateAndActiveChildStateIsCrouchMovementState(
            [Values(0.11f, 0.707f, -0.11f, -0.707f)] float xValue,
            [Values(-0.5f, -.707f)] float yValue)
        {
            _sut.SetMovement(new Vector2(xValue, yValue));

            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = true,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<CrouchMovementState>(_sut.ActiveChildState);
        }
    }
}
