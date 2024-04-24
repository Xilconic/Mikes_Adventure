using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assets.Characters.Player;
using Assets.Tests;
using Assets.Tests.EditModeTests;
using Assets.GeneralScripts;
using UnityEngine.PlayerLoop;

public class PlayerStateMachineTests
{
    protected GameObject _gameObject;
    protected Rigidbody2D _rigidBody2D;
    protected PlayerStateMachine _sut;
    protected TimeMock _timeMock;
    protected AnimatorMock _animator;

    [SetUp]
    public void SetUp()
    {
        _gameObject = new GameObject();
        _rigidBody2D = _gameObject.AddComponent<Rigidbody2D>();
        _timeMock = new TimeMock();
        _animator = new AnimatorMock();
        _sut = new PlayerStateMachine(_rigidBody2D, _animator)
        {
            Time = _timeMock,
        };

        AdditionalSetUp();

        AssertInitialStateConditions();
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(_gameObject);
    }

    protected virtual void AdditionalSetUp()
    {

    }

    protected virtual void AssertInitialStateConditions()
    {
        Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
        Assert.IsInstanceOf<IdleState>(_sut.ActiveChildState);
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
        [Test]
        public void WhenUpdate_ThenAnimationIsSetToMikeIdle()
        {
            _sut.Update();

            Assert.AreEqual(AnimationClipNames.Idle, _animator.CurrentPlayingAnimationClip);
        }

        [Test]
        [TestCase(1f)]
        [TestCase(-1f)]
        public void AndRigidBodyHasVelocityOnX_WhenFixedUpdate_ThenRigidBodyHasVelocityZeroOnX(
            float someVelocity)
        {
            _rigidBody2D.velocity = new Vector2(someVelocity, 0);

            _sut.FixedUpdate();

            Assert.AreEqual(0, _rigidBody2D.velocity.x);
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

            AssertInitialStateConditions();
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
        protected override void AdditionalSetUp()
        {
            base.AdditionalSetUp();

            var movementInput = Vector2.left;

            _sut.SetMovement(movementInput);
        }

        protected override void AssertInitialStateConditions()
        {
            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<GroundMovementState>(_sut.ActiveChildState);
        }

        [Test]
        [TestCase(0.5f)]
        [TestCase(0.01f)]
        [TestCase(-0.01f)]
        [TestCase(-0.5f)]
        public void AndMovementSpeedIsLessThanOrEqualsToWalkingTheshold_WhenUpdate_ThenAnimationIsSetToMikeWalk(
            float movementInputX)
        {
            _sut.SetMovement(new Vector2(movementInputX, 0f));
            _sut.FixedUpdate();

            _sut.Update();

            Assert.AreEqual(AnimationClipNames.Walk, _animator.CurrentPlayingAnimationClip);
        }

        [Test]
        [TestCase(1.0f)]
        [TestCase(0.51f)]
        [TestCase(-0.51f)]
        [TestCase(-1.0f)]
        public void AndMovementSpeedIsGreaterThanWalkingTheshold_WhenUpdate_ThenAnimationIsSetToMikeJog(
            float movementInputX)
        {
            _sut.SetMovement(new Vector2(movementInputX, 0f));
            _sut.FixedUpdate();

            _sut.Update();

            Assert.AreEqual(AnimationClipNames.Jog, _animator.CurrentPlayingAnimationClip);
        }

        [Test]
        [TestCase(1f, 10.0f)]
        [TestCase(0.1f, 1.0f)]
        [TestCase(0.01f, 0.1f)]
        [TestCase(-0.01f, -0.1f)]
        [TestCase(-0.1f, -1.0f)]
        [TestCase(-1f, -10.0f)]
        public void AndRigidBodyHasZeroVelocityOnXAndHorizontalMovementSet_WhenFixedUpdate_ThenRigidBodyHasVelocityOnX(
            float inputX, float expectedVelocityX)
        {
            const float originalVelocityY = 0;
            _rigidBody2D.velocity = new Vector2(0, originalVelocityY);

            _sut.SetMovement(new Vector2(inputX, 0));

            _sut.FixedUpdate();

            Assert.AreEqual(expectedVelocityX, _rigidBody2D.velocity.x, 0.001f);
            Assert.AreEqual(originalVelocityY, _rigidBody2D.velocity.y, 0.001f);
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

            AssertInitialStateConditions();
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

    public class GivenActiveChildStateIsCrouchIdleState : PlayerStateMachineTests
    {
        protected override void AdditionalSetUp()
        {
            base.AdditionalSetUp();

            var movementInput = Vector2.down;

            _sut.SetMovement(movementInput);
        }

        protected override void AssertInitialStateConditions()
        {
            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<CrouchIdleState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenUpdate_ThenAnimationIsSetToMikeWalk()
        {
            _sut.Update();

            Assert.AreEqual(AnimationClipNames.CrouchIdle, _animator.CurrentPlayingAnimationClip);
        }

        [Test]
        public void WhenSettingSignificantDownMovementAndWithInsignificantLateralMovementAndRigidBodyHasVelocityOnX_WhenFixedUpdate_ThenRigidBodyHasVelocityZeroOnX(
            [Values(0f, 0.1f, -0.1f)] float xValue,
            [Values(-0.5f, -.8f)] float yValue)
        {
            const float originalVelocityY = 0;
            _rigidBody2D.velocity = new Vector2(1f, originalVelocityY);

            _sut.SetMovement(new Vector2(xValue, yValue));

            _sut.FixedUpdate();

            Assert.AreEqual(0, _rigidBody2D.velocity.x);
            Assert.AreEqual(originalVelocityY, _rigidBody2D.velocity.y);
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

            AssertInitialStateConditions();
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

            AssertInitialStateConditions();
        }
    }

    public class GivenActiveChildStateIsCrouchMovementState : PlayerStateMachineTests
    {
        protected override void AdditionalSetUp()
        {
            base.AdditionalSetUp();

            var movementInput = new Vector2(0.707f, -0.707f); // 45 degrees down right

            _sut.SetMovement(movementInput);
        }

        protected override void AssertInitialStateConditions()
        {
            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<CrouchMovementState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenUpdate_ThenAnimationIsSetToMikeWalk()
        {
            _sut.Update();

            Assert.AreEqual(AnimationClipNames.CrouchWalk, _animator.CurrentPlayingAnimationClip);
        }

        [Test]
        [TestCase(0.11f, -0.5f, 0.33f)]
        [TestCase(0.707f, -.707f, 2.121f)]
        [TestCase(-0.11f, -.707f, -0.33f)]
        [TestCase(-.707f, -.707f, -2.121f)]
        public void WhenSettingSignificantDownMovementAndWithSignificantLateralMovementAndRigidBodyHasZeroVelocityOnX_WhenFixedUpdate_ThenRigidBodyHasVelocityOnX(
            float xValue, float yValue, float expectedVelocityX)
        {
            const float originalVelocityY = 0;
            _rigidBody2D.velocity = new Vector2(0, originalVelocityY);

            _sut.SetMovement(new Vector2(xValue, yValue));

            _sut.FixedUpdate();

            Assert.AreEqual(expectedVelocityX, _rigidBody2D.velocity.x, 0.001);
            Assert.AreEqual(originalVelocityY, _rigidBody2D.velocity.y);
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

            AssertInitialStateConditions();
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

            AssertInitialStateConditions();
        }
    }

    public class GivenActiveChildStateIsFallingState : PlayerStateMachineTests
    {
        protected override void AdditionalSetUp()
        {
            base.AdditionalSetUp();

            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = false,
            };
            _sut.NotifyTouchingDirections(touchingDirections);
        }

        protected override void AssertInitialStateConditions()
        {
            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<FallingState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenUpdate_ThenAnimationIsSetToMikeWalk()
        {
            _sut.Update();

            Assert.AreEqual(AnimationClipNames.Falling, _animator.CurrentPlayingAnimationClip);
        }

        [Test]
        [TestCase(1.0f, 0.0f, 0.01f, 10.0f)]
        [TestCase(0.707f, 0.707f, 1.23f, 7.07f)]
        [TestCase(0.01f, 0.707f, 1.23f, 0.1f)]
        [TestCase(0.0f, 0.707f, 1.11f, 0f)]
        [TestCase(-0.01f, 0.707f, 3.21f, -0.1f)]
        [TestCase(-0.707f, 0.707f, 0.01f, -7.07f)]
        [TestCase(-1.0f, 0.0f, 1.23f, -10.0f)]
        public void WhenSettingMovementAndRigidBodyHasZeroVelocityOnX_WhenFixedUpdate_ThenRigidBodyHasVelocityOnX(
            float xValue, float yValue, float originalVelocityY, float expectedVelocityX)
        {
            _rigidBody2D.velocity = new Vector2(0, originalVelocityY);

            _sut.SetMovement(new Vector2(xValue, yValue));

            _sut.FixedUpdate();

            Assert.AreEqual(expectedVelocityX, _rigidBody2D.velocity.x, 0.001);
            Assert.AreEqual(originalVelocityY, _rigidBody2D.velocity.y, 0.001);
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

        [Test]
        [TestCase(0f)]
        [TestCase(0.09999f)]
        public void WhenCoyoteTimeActiveAndJumpButtonPressed_ThenCurrentStateIsAirialStateAndActiveChildStateIsJumpState(
            float secondsPassedSinceEnteringFallingState)
        {
            _timeMock.DeltaTime = secondsPassedSinceEnteringFallingState;
            _sut.Update(); // Time progresses
            Assert.IsTrue(_sut.ActiveChildState.CanJump);

            _sut.Jump();

            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<JumpState>(_sut.ActiveChildState);
        }

        [Test]
        [TestCase(0)] // 0 frames => 0s
        [TestCase(5)] // 5 frames => 5/60s = 0.83333...s
        [TestCase(6)] // 6 frames => 6/60s = ~0.1...s (suffers from limited precision rounding errors)
        public void WhenCoyoteTimeActiveWhileMultiple60fpsFramesPassAndJumpButtonPressed_ThenCurrentStateIsAirialStateAndActiveChildStateIsJumpState(
            int numberOfStandardFramesPassed)
        {
            _timeMock.DeltaTime = 1.0f/60.0f;
            for (int i = 0; i < numberOfStandardFramesPassed; i++)
            {
                _sut.Update(); // Time progresses
            }
            Assert.IsTrue(_sut.ActiveChildState.CanJump);

            _sut.Jump();

            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<JumpState>(_sut.ActiveChildState);
        }

        [Test]
        [TestCase(1f)]
        [TestCase(float.MaxValue)]
        public void WhenCoyoteTimeClosedAgainAndJumpButtonPressed_ThenStateRemainsUnchanged(
            float secondsPassedSinceEnteringFallingState)
        {
            _timeMock.DeltaTime = secondsPassedSinceEnteringFallingState;
            _sut.Update(); // Time progresses
            Assert.IsFalse(_sut.ActiveChildState.CanJump);

            _sut.Jump();

            AssertInitialStateConditions();
        }

        [Test]
        [TestCase(7)] // 7 frames => 7/60s = 0.1s
        [TestCase(60)] // 60 frames => 1s
        public void WhenCoyoteTimeClosedAgainWhileMultiple60fpsFramesPassAndJumpButtonPressed_ThenStateRemainsUnchanged(
            int numberOfStandardFramesPassed)
        {
            _timeMock.DeltaTime = 1.0f / 60.0f;
            for (int i = 0; i < numberOfStandardFramesPassed; i++)
            {
                _sut.Update(); // Time progresses
            }
            Assert.IsFalse(_sut.ActiveChildState.CanJump);

            _sut.Jump();

            AssertInitialStateConditions();
        }

        [Test]
        [TestCase(0f)]
        [TestCase(0.09999f)]
        public void WhenJumpButtonPressedMidairAndLandsWithJumpBufferActive_ThenCurrentStateIsAirialStateAndActiveChildStateIsJumpState(
            float secondsPassedSinceJumpButtonPressed)
        {
            _timeMock.DeltaTime = 1f;
            _sut.Update(); // Pass time beyond Coyote Time
            _sut.Jump();
            AssertInitialStateConditions();

            _timeMock.DeltaTime = secondsPassedSinceJumpButtonPressed;
            _sut.Update();
            AssertInitialStateConditions();

            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = true,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<JumpState>(_sut.ActiveChildState);
        }

        [Test]
        [TestCase(1f)]
        [TestCase(float.MaxValue)]
        public void WhenJumpButtonPressedMidairAndLandsWithJumpBufferInactive_ThenCurrentStateIsGroundedAndActiveChildStateIsIdleState(
            float secondsPassedSinceJumpButtonPressed)
        {
            _timeMock.DeltaTime = 1f;
            _sut.Update(); // Pass time beyond Coyote Time
            _sut.Jump();
            AssertInitialStateConditions();

            _timeMock.DeltaTime = secondsPassedSinceJumpButtonPressed;
            _sut.Update();
            AssertInitialStateConditions();

            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = true,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<IdleState>(_sut.ActiveChildState);
        }
    }

    public class GivenActiveChildStateIsJumpState : PlayerStateMachineTests
    {
        protected override void AdditionalSetUp()
        {
            base.AdditionalSetUp();

            _sut.Jump();
        }

        protected override void AssertInitialStateConditions()
        {
            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<JumpState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenUpdate_ThenAnimationIsSetToMikeWalk()
        {
            _sut.Update();

            Assert.AreEqual(AnimationClipNames.Jump, _animator.CurrentPlayingAnimationClip);
        }

        [Test]
        public void ThenRigidBodyHasVelocityY()
        {
            Assert.AreEqual(10.0f, _rigidBody2D.velocity.y);
        }

        [Test]
        public void AndUpwardVelocityHasDecreasedDuePassageOfTime_WhenJumping_ThenRigidBodyVelocityYUnchanged()
        {
            _rigidBody2D.velocity = new Vector2(0, 5.0f);
            _timeMock.DeltaTime = 0.1f;
            _sut.FixedUpdate();

            _sut.Jump();

            Assert.AreEqual(5.0f, _rigidBody2D.velocity.y);
            AssertInitialStateConditions();
        }

        [Test]
        [TestCase(0.0f)]
        [TestCase(-1.0f)]
        public void AndUpwardVelocityBecameDownOrNegativeDuePassageofTime_WhenFixedUpdate_ThenCurrentStateIsAirialStateAndActiveChildStateIsFallingState(
            float newVelocityY)
        {
            _rigidBody2D.velocity = new Vector2(0, newVelocityY);
            _timeMock.DeltaTime = 0.1f;
            _sut.FixedUpdate();

            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<FallingState>(_sut.ActiveChildState);
        }

        [Test]
        [TestCase(9.0f, 4.5f)]
        [TestCase(0.1f, 0.05f)]
        public void AndUpwardVelocityHasDecreasedDuePassageofTime_WhenReleasingJump_ThenRigidBodyVelocityYHalved(
            float velocityY, float expectedVelocityY)
        {
            _rigidBody2D.velocity = new Vector2(0, velocityY);
            _timeMock.DeltaTime = 0.1f;
            _sut.FixedUpdate();

            _sut.JumpRelease();

            Assert.AreEqual(expectedVelocityY, _rigidBody2D.velocity.y, 0.001f);
            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<JumpState>(_sut.ActiveChildState);
        }

        [Test]
        [TestCase(0.0f)]
        [TestCase(-1.0f)]
        public void AndUpwardVelocityBecameDownOrNegativeDuePassageofTime_WhenReleasingJump_ThenRigidBodyVelocityYUnchanged(
            float newVelocityY)
        {
            _rigidBody2D.velocity = new Vector2(0, newVelocityY);
            _timeMock.DeltaTime = 0.1f;
            _sut.FixedUpdate();

            _sut.JumpRelease();

            Assert.AreEqual(newVelocityY, _rigidBody2D.velocity.y, 0.001f);
            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<FallingState>(_sut.ActiveChildState);
        }

        [Test]
        [TestCase(1.0f, 0.0f, 10.0f)]
        [TestCase(0.707f, 0.707f, 7.07f)]
        [TestCase(0.01f, 0.707f, 0.1f)]
        [TestCase(0.0f, 0.707f, 0f)]
        [TestCase(-0.01f, 0.707f, -0.1f)]
        [TestCase(-0.707f, 0.707f, -7.07f)]
        [TestCase(-1.0f, 0.0f, -10.0f)]
        public void AndSettingMovementAndRigidBodyHasZeroVelocityOnX_WhenFixedUpdate_ThenRigidBodyHasVelocityOnX(
            float xValue, float yValue, float expectedVelocityX)
        {
            float originalVelocityY = _rigidBody2D.velocity.y;
            _rigidBody2D.velocity = new Vector2(0, originalVelocityY);

            _sut.SetMovement(new Vector2(xValue, yValue));

            _sut.FixedUpdate();

            Assert.AreEqual(expectedVelocityX, _rigidBody2D.velocity.x, 0.001);
            Assert.AreEqual(originalVelocityY, _rigidBody2D.velocity.y, 0.001);
        }

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

    // TODO: Implement WallSlide state and transitions and effects
    // TODO: Implement WallJump state and transitions and effects
}
