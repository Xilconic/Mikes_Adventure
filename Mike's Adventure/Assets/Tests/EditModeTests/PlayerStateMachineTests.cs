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
    protected const float ExpectedJumpGravityScale = 1f;

    protected GameObject _gameObject;
    protected Rigidbody2D _rigidBody2D;
    protected PlayerStateMachine _sut;
    protected TimeMock _timeMock;
    protected AnimatorMock _animator;
    protected PlayerFacingMock _playerFacing;

    // TODO: When player crouhced under obstacle and touches ceiling: Jump should not be possible (can still happen inconsistently :( )

    [SetUp]
    public void SetUp()
    {
        _gameObject = new GameObject();
        _rigidBody2D = _gameObject.AddComponent<Rigidbody2D>();
        var configuration = _gameObject.AddComponent<PlayerConfiguration>();
        configuration.AccelerationBasedMovement = true;
        _timeMock = new TimeMock();
        _animator = new AnimatorMock();
        _playerFacing = new PlayerFacingMock();
        _sut = new PlayerStateMachine(_rigidBody2D, _animator, configuration, _playerFacing)
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

    protected void SimulateJumpInputProcessing(bool shouldNotifyTouchingDirectionsNoLongerTouchingGround = false)
    {
        _sut.Jump();
        if (shouldNotifyTouchingDirectionsNoLongerTouchingGround)
        {
            _sut.NotifyTouchingDirections(new TouchingDirectionsStub { IsGrounded = false });
        }
        _sut.FixedUpdate(); // FixedUpdate() required due to physics interactions!
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

        [Test]
        public void ThenFacingIsRight()
        {
            Assert.IsTrue(_playerFacing.IsFacingRight);
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

        [Test]// Deaccelerating force: (((xValue * 0 - initialVelocityX) * 1) / 0.02) * .9 = -45 * initialVelocityX
        [TestCase(1f, -45f)]
        [TestCase(-2f, 90f)]
        public void AndRigidBodyHasVelocityOnX_WhenFixedUpdate_ThenRigidBodyHasVelocityXUnchanged(
            float initialVelocityX, float expectedForceX)
        {
            _rigidBody2D.velocity = new Vector2(initialVelocityX, 0);

            _sut.FixedUpdate();

            Assert.AreEqual(expectedForceX, _rigidBody2D.totalForce.x, 0.01);
            Assert.AreEqual(0, _rigidBody2D.totalForce.y);
            Assert.AreEqual(initialVelocityX, _rigidBody2D.velocity.x, 0.01, "Because FixedUpdate does not directly set _rigidBody.velocity");
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
            SimulateJumpInputProcessing();

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
            
            Assert.IsFalse(_playerFacing.IsFacingRight, "Player is walking to the left in the setup");
        }

        [Test]
        public void WhenSettingMovementInputTowardsTheRight_ThenPlayerFacingChangesToTheRight()
        {
            _sut.SetMovement(Vector2.right);

            Assert.IsTrue(_playerFacing.IsFacingRight);
        }

        [Test] // Target Velocity = movementVelocityX * MaxRunSpeed = movementVelocityX * 10
        [TestCase(5f)] // movementVelocityX = 0.5 => Target Velocity = 5
        [TestCase(0.1f)] // movementVelocityX = 0.01 => Target Velocity = 0.1
        [TestCase(-0.1f)] // movementVelocityX = -0.01 => Target Velocity = -0.1
        [TestCase(-5f)] // movementVelocityX = -0.5 => Target Velocity = -5
        public void AndMovementSpeedIsLessThanOrEqualsToWalkingTheshold_WhenUpdate_ThenAnimationIsSetToMikeWalk(
            float movementVelocityX)
        {
            // Note: SetMovement adjusts forces. As this test does not simulate physics, we're simulating the effects of stick movement
            _rigidBody2D.velocity = new Vector2(movementVelocityX, 0f);

            _sut.Update();

            Assert.AreEqual(AnimationClipNames.Walk, _animator.CurrentPlayingAnimationClip);
        }

        [Test] // Target Velocity = movementVelocityX * MaxRunSpeed = movementVelocityX * 10
        [TestCase(10f)] // movementVelocityX = 1.0 => Target Velocity = 10
        [TestCase(5.1f)] // movementVelocityX = 0.51 => Target Velocity = 5.1
        [TestCase(-5.1f)] // movementVelocityX = -0.51 => Target Velocity = -5.1
        [TestCase(-10f)] // movementVelocityX = -1.0 => Target Velocity = -10
        public void AndMovementSpeedIsGreaterThanWalkingTheshold_WhenUpdate_ThenAnimationIsSetToMikeJog(
            float movementVelocityX)
        {
            // Note: SetMovement adjusts forces. As this test does not simulate physics, we're simulating the effects of stick movement
            _rigidBody2D.velocity = new Vector2(movementVelocityX, 0f);

            _sut.Update();

            Assert.AreEqual(AnimationClipNames.Jog, _animator.CurrentPlayingAnimationClip);
        }

        [Test] // Accelerating force: (((xValue *10 - 0) * 1) / 0.02) * .1 = 50 * xValue
        [TestCase(1f, 50.0f)]
        [TestCase(0.1f, 5.0f)]
        [TestCase(0.01f, 0.5f)]
        [TestCase(-0.01f, -0.5f)]
        [TestCase(-0.1f, -5.0f)]
        [TestCase(-1f, -50.0f)]
        public void AndRigidBodyHasZeroVelocityOnXAndHorizontalMovementSet_WhenFixedUpdate_ThenRigidBodyHorizontalForcesApplied(
            float inputX, float expectedForceX)
        {
            const float originalVelocityY = 0;
            _rigidBody2D.velocity = new Vector2(0, originalVelocityY);

            _sut.SetMovement(new Vector2(inputX, 0));

            _sut.FixedUpdate();


            Assert.AreEqual(expectedForceX, _rigidBody2D.totalForce.x, 0.01);
            Assert.AreEqual(0, _rigidBody2D.totalForce.y, 0.01);
            Assert.AreEqual(0, _rigidBody2D.velocity.x, 0.001, "Because FixedUpdate does not directly set _rigidBody.velocity");
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
            SimulateJumpInputProcessing();

            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<JumpState>(_sut.ActiveChildState);
        }

        [Test] // Accelerating force: (((xValue *10 - 0) * 1) / 0.02) * .1 = 50 * xValue
        [TestCase(1f, 50.0f)] 
        [TestCase(0.1f, 5.0f)]
        [TestCase(0.01f, 0.5f)]
        [TestCase(-0.01f, -0.5f)]
        [TestCase(-0.1f, -5.0f)]
        [TestCase(-1f, -50.0f)]
        public void AndRigidBodyHasNonZeroVelocityOnXDueToPlayerInput_WhenJumpAndFixedUpdate_ThenRigidBodyHasHorizontalForcesApplied(
            float xValue, float expectedForceX)
        {
            _rigidBody2D.velocity = Vector2.zero;
            _sut.SetMovement(new Vector2(xValue, 0));

            SimulateJumpInputProcessing();

            Assert.AreEqual(expectedForceX, _rigidBody2D.totalForce.x, 0.01);
            Assert.AreEqual(0, _rigidBody2D.totalForce.y, 0.01);
            Assert.AreEqual(0, _rigidBody2D.velocity.x, 0.001, "Because FixedUpdate does not directly set _rigidBody.velocity");
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
        [TestCase(0.1f, -.8f, true)]
        [TestCase(-0.1f, -.8f, false)]
        public void WhenSettingSignificantDownMovementAndWithInsignificantLateralMovementAndRigidBodyHasVelocityOnX_ThenPlayerFacingUpdated(
            float xValue,
            float yValue,
            bool expectedIsFacingRight)
        {
            _sut.SetMovement(new Vector2(xValue, yValue));

            Assert.AreEqual(expectedIsFacingRight, _playerFacing.IsFacingRight);
        }

        [Test]
        public void WhenSettingSignificantDownMovementAndWithInsignificantLateralMovementAndRigidBodyHasVelocityOnX_WhenFixedUpdate_ThenRigidBodyHasVelocityZeroOnXAndUnchangedState(
            [Values(0f, 0.1f, -0.1f)] float xValue,
            [Values(-0.5f, -.8f)] float yValue)
        {
            const float originalVelocityY = 0;
            _rigidBody2D.velocity = new Vector2(1f, originalVelocityY);

            _sut.SetMovement(new Vector2(xValue, yValue));

            _sut.FixedUpdate();

            Assert.AreEqual(-45.0f, _rigidBody2D.totalForce.x, 0.01f, "Because Idle state perform a counter-acting force of ((-1 * 1) / 0.02) * 0.9 = -45.0; Target velocity is 0, so delta_v = 0-1 = -1. _rigidBody.mass = 1. Time.fixedDeltaTime = 0.02. _configuration.DeaccelerationRate = 0.9.");
            Assert.AreEqual(0, _rigidBody2D.totalForce.y);
            Assert.AreEqual(1f, _rigidBody2D.velocity.x, "Because the FixedUpdate itself does not change the velocity");
            Assert.AreEqual(originalVelocityY, _rigidBody2D.velocity.y);
            AssertInitialStateConditions();
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
            SimulateJumpInputProcessing();

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

            SimulateJumpInputProcessing();

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

        [TestCase(3.0f)]
        [TestCase(0.1f)]
        [TestCase(-0.1f)]
        [TestCase(-3.0f)]
        public void AndVelocityWithinMaxCrouchWalkSpeed_WhenUpdate_ThenAnimationIsSetToMikeCrouchWalk(
            float velocityX)
        {
            _rigidBody2D.AdjustVelocityX(velocityX);
            _sut.Update();

            Assert.AreEqual(AnimationClipNames.CrouchWalk, _animator.CurrentPlayingAnimationClip);
        }

        [TestCase(10.0f)]
        [TestCase(3.1f)]
        [TestCase(-3.1f)]
        [TestCase(-10.0f)]
        public void AndVelocityOverMaxCrouchWalkSpeed_WhenUpdate_ThenAnimationIsSetToMikeCrouchSlide(
            float velocityX)
        {
            _rigidBody2D.AdjustVelocityX(velocityX);
            _sut.Update();

            Assert.AreEqual(AnimationClipNames.CrouchSlide, _animator.CurrentPlayingAnimationClip);
        }

        [TestCase(0.707f, -.707f, true)]
        [TestCase(-0.707f, -.707f, false)]
        public void WhenSettingSignificantDownMovementAndWithSignificantLateralMovementAndRigidBodyHasVelocityOnX_ThenPlayerFacingUpdated(
            float xValue,
            float yValue,
            bool expectedIsFacingRight)
        {
            _sut.SetMovement(new Vector2(xValue, yValue));

            Assert.AreEqual(expectedIsFacingRight, _playerFacing.IsFacingRight);
        }

        [Test] // Accelerating force: (((xValue * 3 - 0) * 1) / 0.02) * .1 = 15 * xValue
        [TestCase(0.11f, -0.5f, 1.65f)]
        [TestCase(0.707f, -.707f, 10.60f)]
        [TestCase(-0.11f, -.707f, -1.65f)]
        [TestCase(-.707f, -.707f, -10.60f)]
        public void WhenSettingSignificantDownMovementAndWithSignificantLateralMovementAndRigidBodyHasZeroVelocityOnX_WhenFixedUpdate_ThenRigidBodyHasHorizontalForcesApplied(
            float xValue, float yValue, float expectedForceX)
        {
            const float originalVelocityY = 0;
            _rigidBody2D.velocity = new Vector2(0, originalVelocityY);

            _sut.SetMovement(new Vector2(xValue, yValue));

            _sut.FixedUpdate();

            Assert.AreEqual(expectedForceX, _rigidBody2D.totalForce.x, 0.01);
            Assert.AreEqual(0, _rigidBody2D.totalForce.y, 0.01);
            Assert.AreEqual(0, _rigidBody2D.velocity.x, 0.001, "Because FixedUpdate does not directly set _rigidBody.velocity");
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
        public void WhenSettingPartialDownMovementInputBeforeThresholdAndTouchingCeiling_ThenCurrentStateIsGroundedStateAndActiveChildStateIsCrouchIdleState(
            float yValue)
        {
            _sut.NotifyTouchingDirections(new TouchingDirectionsStub { IsOnCeiling = true });

            var movementInput = new Vector2(0, yValue);

            _sut.SetMovement(movementInput);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<CrouchIdleState>(_sut.ActiveChildState);
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
        [TestCase(-0.01f)]
        [TestCase(-0.49f)]
        public void WhenSettingPartialDownMovementInputBeforeThresholdAndWithLateralComponentAndTouchingCeiling_ThenCurrentStateIsGroundedStateAndActiveChildStateIsCrouchMovementState(
            float yValue)
        {
            _sut.NotifyTouchingDirections(new TouchingDirectionsStub { IsOnCeiling = true });

            var movementInput = new Vector2(0.2f, yValue);

            _sut.SetMovement(movementInput);

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<CrouchMovementState>(_sut.ActiveChildState);
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
            SimulateJumpInputProcessing();

            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<JumpState>(_sut.ActiveChildState);
        }

        [Test] // Note: Higher airial mobility, so movement input is translated into higher speeds! Accelerating force: (((xValue * 10 - 0) * 1) / 0.02) * .1 = 50 * xValue
        [TestCase(0.11f, -0.5f, 5.50f)]
        [TestCase(0.707f, -.707f, 35.35f)]
        [TestCase(-0.11f, -.707f, -5.50f)]
        [TestCase(-.707f, -.707f, -35.35f)]
        public void AndRigidBodyHasNonZeroVelocityOnXDueToPlayerInput_WhenJumpAndFixedUpdate_ThenRigidBodyHasHorizontalForcesApplied(
            float xValue, float yValue, float expectedForceX)
        {
            _rigidBody2D.velocity = Vector2.zero;
            _sut.SetMovement(new Vector2(xValue, yValue));

            SimulateJumpInputProcessing();

            Vector2 totalForce = _rigidBody2D.totalForce;
            Assert.AreEqual(expectedForceX, totalForce.x, 0.01f, "Because FixedUpdate applies force to _rigidBody");
            Assert.AreEqual(0, totalForce.y);
            Assert.AreEqual(0, _rigidBody2D.velocity.x, 0.001, "Because FixedUpdate does not set velocity");
        }

        [Test]
        public void WhenJumpButtonPressedWhileOnCeiling_ThenStateRemainsUnchanged()
        {
            var touchingDirections = new TouchingDirectionsStub
            {
                IsOnCeiling = true
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            SimulateJumpInputProcessing();

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
            _sut.FixedUpdate();
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
        public void ThenRigidBodyHasGravityScaleTwo()
        {
            Assert.AreEqual(2f, _rigidBody2D.gravityScale);
        }

        [Test]
        [TestCase(1f, true)]
        [TestCase(-1f, false)]
        public void WhenSettingMovement_ThenPlayerFacingUpdated(
            float xValue,
            bool expectedIsFacingRight)
        {
            _sut.SetMovement(new Vector2(xValue, 0));

            Assert.AreEqual(expectedIsFacingRight, _playerFacing.IsFacingRight);
        }

        [Test] // Accelerating force: (((xValue * 10 - 0) * 1) / 0.02) * .1 = 50 * xValue
        [TestCase(1.0f, 0.0f, 0.01f, 50.0f)]
        [TestCase(0.707f, 0.707f, 1.23f, 35.35f)]
        [TestCase(0.01f, 0.707f, 1.23f, 0.50f)]
        [TestCase(0.0f, 0.707f, 1.11f, 0f)]
        [TestCase(-0.01f, 0.707f, 3.21f, -0.5f)]
        [TestCase(-0.707f, 0.707f, 0.01f, -35.35f)]
        [TestCase(-1.0f, 0.0f, 1.23f, -50.0f)]
        public void WhenSettingMovementAndRigidBodyHasZeroVelocityOnX_WhenFixedUpdate_ThenRigidBodyHasHorizontalForcesApplied(
            float xValue, float yValue, float originalVelocityY, float expectedForceX)
        {
            _rigidBody2D.velocity = new Vector2(0, originalVelocityY);

            _sut.SetMovement(new Vector2(xValue, yValue));

            _sut.FixedUpdate();

            Assert.AreEqual(expectedForceX, _rigidBody2D.totalForce.x, 0.01);
            Assert.AreEqual(0, _rigidBody2D.totalForce.y, 0.01);
            Assert.AreEqual(0, _rigidBody2D.velocity.x, 0.001, "Because FixedUpdate does not directly set _rigidBody.velocity");
            Assert.AreEqual(originalVelocityY, _rigidBody2D.velocity.y);
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
        public void WhenCoyoteTimeActiveAndJumpButtonPressed_ThenCurrentStateIsAirialStateAndActiveChildStateIsJumpStateAndGravityScaleResetToOne(
            float secondsPassedSinceEnteringFallingState)
        {
            _timeMock.DeltaTime = secondsPassedSinceEnteringFallingState;
            _sut.Update(); // Time progresses
            Assert.IsFalse(_sut.ActiveChildState.CanJump);

            SimulateJumpInputProcessing();

            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<JumpState>(_sut.ActiveChildState);

            Assert.AreEqual(1f, _rigidBody2D.gravityScale);
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
            Assert.IsFalse(_sut.ActiveChildState.CanJump);

            SimulateJumpInputProcessing();

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

            SimulateJumpInputProcessing();

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

            SimulateJumpInputProcessing();

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
            SimulateJumpInputProcessing();
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
            SimulateJumpInputProcessing();
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

        [Test]
        public void WhenTouchesWall_ThenCurrentStateIsAirialAndActiveChildStateIsWallSlideState()
        {
            // ... simulate falling / grazing past ledge:
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = false, // Need to still be airborne
                WallDirection = WallDirection.Left,
            };
            _sut.NotifyTouchingDirections(touchingDirections);
            _sut.FixedUpdate();

            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<WallSlideState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenFallsIntoWall_ThenCurrentStateIsAirialAndActiveChildStateIsWallSlideState()
        {
            // ... while simulating player movement into a wall:
            _sut.SetMovement(Vector2.right);
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = false, // Need to still be airborne
                WallDirection = WallDirection.Left,
            };
            _sut.NotifyTouchingDirections(touchingDirections);
            _sut.FixedUpdate();

            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<WallSlideState>(_sut.ActiveChildState);
        }
    }

    public class GivenActiveChildStateIsJumpState : PlayerStateMachineTests
    {
        protected override void AdditionalSetUp()
        {
            base.AdditionalSetUp();

            SimulateJumpInputProcessing(true);
        }

        protected override void AssertInitialStateConditions()
        {
            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<JumpState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenUpdate_ThenAnimationIsSetToMikeJump()
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
        public void ThenRigidBodyHasGravityScalingOne()
        {
            Assert.AreEqual(ExpectedJumpGravityScale, _rigidBody2D.gravityScale);
        }

        [Test]
        [TestCase(1f, true)]
        [TestCase(-1f, false)]
        public void WhenSettingMovement_ThenPlayerFacingUpdated(
            float xValue,
            bool expectedIsFacingRight)
        {
            _sut.SetMovement(new Vector2(xValue, 0));

            Assert.AreEqual(expectedIsFacingRight, _playerFacing.IsFacingRight);
        }

        [Test]
        public void AndUpwardVelocityHasDecreasedDuePassageOfTime_WhenJumpingAndFixedUpdate_ThenRigidBodyVelocityYUnchanged()
        {
            _rigidBody2D.velocity = new Vector2(0, 5.0f);
            _timeMock.DeltaTime = 0.1f;
            _sut.FixedUpdate();

            _sut.FixedUpdate(); // FixedUpdate() required due to physics interactions!

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

        [Test] // Accelerating force: (((xValue * 10 - 0) * 1) / 0.02) * .1 = 50 * xValue
        [TestCase(1.0f, 0.0f, 50.0f)]
        [TestCase(0.707f, 0.707f, 35.35f)]
        [TestCase(0.01f, 0.707f, 0.5f)]
        [TestCase(0.0f, 0.707f, 0f)]
        [TestCase(-0.01f, 0.707f, -0.5f)]
        [TestCase(-0.707f, 0.707f, -35.35f)]
        [TestCase(-1.0f, 0.0f, -50.0f)]
        public void AndSettingMovementAndRigidBodyHasZeroVelocityOnX_WhenFixedUpdate_ThenRigidBodyHasHorizontalForcesApplied(
            float xValue, float yValue, float expectedForceX)
        {
            float originalVelocityY = _rigidBody2D.velocity.y;
            _rigidBody2D.velocity = new Vector2(0, originalVelocityY);

            _sut.SetMovement(new Vector2(xValue, yValue));

            _sut.FixedUpdate();

            Assert.AreEqual(expectedForceX, _rigidBody2D.totalForce.x, 0.01);
            Assert.AreEqual(0, _rigidBody2D.totalForce.y, 0.01);
            Assert.AreEqual(0, _rigidBody2D.velocity.x, 0.001, "Because FixedUpdate does not directly set _rigidBody.velocity");
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

        [Test(Description = "When in the JumpState, touching the ground means nothing as we're still going upwards. We're just brushing alongside an edge, and therefore should continue to remain in JumpState.")]
        public void WhenHittingGround_ThenStateIsUnchanged()
        {
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = true,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            AssertInitialStateConditions();
        }

        [Test(Description = "When in the JumpState, touching the ground means nothing as we're still going upwards. We're just brushing alongside an edge, and therefore should continue to remain in JumpState.")]
        [TestCase(-0.01f)]
        [TestCase(-0.49f)]
        public void WhenHittingGroundWithNonSignificantDownwardMovementInput_ThenStateIsUnchanged(
            float yValue)
        {
            _sut.SetMovement(new Vector2(0, yValue));
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = true,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            AssertInitialStateConditions();
        }

        [Test(Description = "When in the JumpState, touching the ground means nothing as we're still going upwards. We're just brushing alongside an edge, and therefore should continue to remain in JumpState.")]
        public void WhenHittingGroundWithNonZeroLateralMovementInput_ThenStateIsUnchanged()
        {
            _sut.SetMovement(Vector2.right);
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = true,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            AssertInitialStateConditions();
        }

        [Test(Description = "When in the JumpState, touching the ground means nothing as we're still going upwards. We're just brushing alongside an edge, and therefore should continue to remain in JumpState.")]
        public void WhenHittingGroundWithSignificantDownMovementAndWithInsignificantLateralMovement_ThenStateIsUnchanged(
            [Values(0f, 0.1f, -0.1f)] float xValue,
            [Values(-0.5f, -.8f)] float yValue)
        {
            _sut.SetMovement(new Vector2(xValue, yValue));

            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = true,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            AssertInitialStateConditions();
        }

        [Test(Description = "When in the JumpState, touching the ground means nothing as we're still going upwards. We're just brushing alongside an edge, and therefore should continue to remain in JumpState.")]
        public void WhenHittingGroundWithSignificantDownMovementAndSignificantLateralMovement_ThenStateIsUnchanged(
            [Values(0.11f, 0.707f, -0.11f, -0.707f)] float xValue,
            [Values(-0.5f, -.707f)] float yValue)
        {
            _sut.SetMovement(new Vector2(xValue, yValue));

            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = true,
            };
            _sut.NotifyTouchingDirections(touchingDirections);

            AssertInitialStateConditions();
        }

        [Test]
        public void WhenTouchingWall_ThenStateIsUnchaged()
        {
            // ... simulating jumping / scraping past a ledge:
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = false, // Need to still be airborne
                WallDirection = WallDirection.Left,
            };
            _sut.NotifyTouchingDirections(touchingDirections);
            _sut.FixedUpdate();

            AssertInitialStateConditions();
        }

        [Test]
        public void WhenJumpingUpAndIntoWall_ThenStateIsUnchaged()
        {
            // ... while simulating player movement into a wall:
            _sut.SetMovement(Vector2.right);
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = false, // Need to still be airborne
                WallDirection = WallDirection.Right,
            };
            _sut.NotifyTouchingDirections(touchingDirections);
            _sut.FixedUpdate();

            AssertInitialStateConditions();
        }
    }

    public class GivenActiveChildStateIsWallSlideState : PlayerStateMachineTests
    {
        protected override void AdditionalSetUp()
        {
            base.AdditionalSetUp();

            // Jump...
            SimulateJumpInputProcessing(true);
            Assert.IsInstanceOf<JumpState>(_sut.ActiveChildState);

            // ... then go into FallingState...
            _rigidBody2D.velocity = new Vector2(0, -4f);
            _sut.FixedUpdate();
            Assert.IsInstanceOf<FallingState>(_sut.ActiveChildState);

            // ... while simulating player movement into a wall:
            _sut.SetMovement(Vector2.right);
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = false, // Need to still be airborne
                WallDirection = WallDirection.Right,
            };
            _sut.NotifyTouchingDirections(touchingDirections);
            _sut.FixedUpdate();
        }

        protected override void AssertInitialStateConditions()
        {
            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<WallSlideState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenUpdate_ThenAnimationIsSetToMikeWallSlide()
        {
            _sut.Update();

            Assert.AreEqual(AnimationClipNames.WallSlide, _animator.CurrentPlayingAnimationClip);
        }

        [Test]
        public void WhenFixedUpdate_ThenRigidBodyHasGravityScalingHalf()
        {
            _sut.FixedUpdate();

            Assert.AreEqual(0.5f, _rigidBody2D.gravityScale);
            Assert.AreEqual(-3.0f, _rigidBody2D.velocity.y, "Because PlayerConfiguration has a max wall slide speed of -3");
        }

        [Test]
        public void ThenPlayerFacingUpdated()
        {
            Assert.IsTrue(_playerFacing.IsFacingRight, "Is sliding wall on right side of player");
        }

        [Test] // Accelerating force: (((xValue * 10 - 0) * 1) / 0.02) * .1 = 50 * xValue
        [TestCase(1.0f, 0.0f, 0.01f, 50.0f)]
        [TestCase(0.707f, 0.707f, 1.23f, 35.35f)]
        [TestCase(0.01f, 0.707f, 1.23f, 0.5f)]
        [TestCase(0.0f, 0.707f, 1.11f, 0f)]
        [TestCase(-0.01f, 0.707f, 3.21f, -0.5f)]
        [TestCase(-0.707f, 0.707f, 0.01f, -35.35f)]
        [TestCase(-1.0f, 0.0f, 1.23f, -50.0f)]
        public void WhenSettingMovementAndRigidBodyHasZeroVelocityOnX_WhenFixedUpdate_ThenRigidBodyHasHorizontalForcesApplied(
            float xValue, float yValue, float originalVelocityY, float expectedForceX)
        {
            _rigidBody2D.velocity = new Vector2(0, originalVelocityY);

            _sut.SetMovement(new Vector2(xValue, yValue));

            Vector2 originalAppliedForces = _rigidBody2D.totalForce; // Test Setup causes initial forces to be applied; We're only interested in the additive forces from the next FixedUpdate:
            _sut.FixedUpdate();

            Assert.AreEqual(expectedForceX, _rigidBody2D.totalForce.x - originalAppliedForces.x, 0.01);
            Assert.AreEqual(0, _rigidBody2D.totalForce.y - originalAppliedForces.y, 0.01);
            Assert.AreEqual(0, _rigidBody2D.velocity.x, 0.001, "Because FixedUpdate does not directly set _rigidBody.velocity");
            Assert.AreEqual(originalVelocityY, _rigidBody2D.velocity.y);
        }

        [Test]
        public void WhenStillFallingAndTouchingWall_ThenStateIsUnchanged()
        {
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = false,
                WallDirection = WallDirection.Right,
            };
            _sut.NotifyTouchingDirections(touchingDirections);
            _sut.FixedUpdate();

            AssertInitialStateConditions();
        }

        [Test]
        public void WhenStillFallingAndNoLongerTouchingWall_ThenCurrentStateIsAirialStateAndActiveChildStateIsFallingState()
        {
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = false,
                WallDirection = WallDirection.None,
            };
            _sut.NotifyTouchingDirections(touchingDirections);
            _sut.FixedUpdate();

            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<FallingState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenHittingGroundWithNoMovementInput_ThenCurrentStateIsGroundedStateAndActiveChildStateIsIdleState()
        {
            _sut.SetMovement(Vector2.zero);
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = true,
                WallDirection = WallDirection.Right,
            };
            _sut.NotifyTouchingDirections(touchingDirections);
            _sut.FixedUpdate();

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
                WallDirection = WallDirection.Right
            };
            _sut.NotifyTouchingDirections(touchingDirections);
            _sut.FixedUpdate();

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
                WallDirection = WallDirection.Right,
            };
            _sut.NotifyTouchingDirections(touchingDirections);
            _sut.FixedUpdate();

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
                WallDirection = WallDirection.Right,
            };
            _sut.NotifyTouchingDirections(touchingDirections);
            _sut.FixedUpdate();

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
                WallDirection = WallDirection.Right,
            };
            _sut.NotifyTouchingDirections(touchingDirections);
            _sut.FixedUpdate();

            Assert.IsInstanceOf<GroundedState>(_sut.CurrentState);
            Assert.IsInstanceOf<CrouchMovementState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenJump_ThenCurrentStateIsAirialStateAndActiveChildStateIsWallJumpState()
        {
            _sut.Jump();
            _sut.NotifyTouchingDirections(new TouchingDirectionsStub
            {
                IsGrounded = false,
                WallDirection = WallDirection.Right,
            });
            _sut.FixedUpdate();

            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<WallJumpState>(_sut.ActiveChildState);
        }
    }

    public class GivenActiveChildStateIsWallJumpStateFromLeftWall : PlayerStateMachineTests
    {
        private Vector2 _preWallJumpForces;

        protected override void AdditionalSetUp()
        {
            base.AdditionalSetUp();

            // Jump...
            SimulateJumpInputProcessing(true);
            Assert.IsInstanceOf<JumpState>(_sut.ActiveChildState);

            // ... then go into FallingState...
            _rigidBody2D.velocity = new Vector2(0, -4f);
            _sut.FixedUpdate();
            Assert.IsInstanceOf<FallingState>(_sut.ActiveChildState);

            // ... while simulating player movement into a wall to reach WallSlideState
            _sut.SetMovement(Vector2.left);
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = false, // Need to still be airborne
                WallDirection = WallDirection.Left,
            };
            _sut.NotifyTouchingDirections(touchingDirections);
            _sut.FixedUpdate();
            Assert.IsInstanceOf<WallSlideState>(_sut.ActiveChildState);
            _sut.FixedUpdate(); // Execute 1 flame in WallSlideState.FixedUpdate
            Assert.AreEqual(-3f, _rigidBody2D.velocity.y, 0.01f);

            _preWallJumpForces = _rigidBody2D.totalForce;

            // ... and finally Jump:
            _sut.Jump();
            _sut.NotifyTouchingDirections(new TouchingDirectionsStub
            {
                IsGrounded = false,
                WallDirection = WallDirection.Left,
            });
            _sut.FixedUpdate(); // FixedUpdate() required due to physics interactions!
        }

        protected override void AssertInitialStateConditions()
        {
            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<WallJumpState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenUpdate_ThenAnimationIsSetToMikeJump()
        {
            _sut.Update();

            Assert.AreEqual(AnimationClipNames.Jump, _animator.CurrentPlayingAnimationClip);
        }

        [Test]
        public void WhenFixedUpdate_ThenRigidBodyHasGravityScalingOneAndJumpJumpVelocitySet()
        {
            Assert.AreEqual(ExpectedJumpGravityScale, _rigidBody2D.gravityScale);
        }

        [Test]
        public void WehnFixedUpdate_ThenRigidBodyHasJumpVelocitiesSet()
        {
            // Jump strength of 10, at 45 degrees towards the right:
            // Total Accelerating force: 10 (PlayerConfiguration.JumpImpulse)
            // Force at 45 degrees to the right for x: 10 * .707 = 7.07
            // Force at 45 degrees to the right for y: 10 * .707 = 7.07
            Assert.AreEqual(0, _rigidBody2D.totalForce.x - _preWallJumpForces.x, 0.01, "Because Impulse forces do not affect RigidBody2D.totalForce");
            Assert.AreEqual(0, _rigidBody2D.totalForce.y - _preWallJumpForces.y, 0.01, "Because Impulse forces do not affect RigidBody2D.totalForce");
            Assert.AreEqual(7.07f, _rigidBody2D.velocity.x, 0.001, "Because FixedUpdate applied the force as an impulse, causing an immediate adjustment of the velocity");
            Assert.AreEqual(7.07f, _rigidBody2D.velocity.y, 0.001, "Because FixedUpdate applied the force as an impulse, causing an immediate adjustment of the velicity");
        }

        [Test]
        public void ThenPlayerFacingIsRight()
        {
            Assert.IsTrue(_playerFacing.IsFacingRight);
        }

        //TODO: Write regression tests:
        //TODO: Transition into falling
        //TODO: Cannot jump again while in WallJumpState
        //TODO: WallJumping into another wall does
        //TODO: While WallJumping, touch another wall an jump again: do nothing (Only WallSlide currently allows wall jumping)
        //TODO: Player movement input is ignored while in this state.
    }

    public class GivenActiveChildStateIsWallJumpStateFromRightWall : PlayerStateMachineTests
    {
        private Vector2 _preWallJumpForces;

        protected override void AdditionalSetUp()
        {
            base.AdditionalSetUp();

            // Jump...
            SimulateJumpInputProcessing(true);
            Assert.IsInstanceOf<JumpState>(_sut.ActiveChildState);

            // ... then go into FallingState...
            _rigidBody2D.velocity = new Vector2(0, -4f);
            _sut.FixedUpdate();
            Assert.IsInstanceOf<FallingState>(_sut.ActiveChildState);

            // ... while simulating player movement into a wall to reach WallSlideState
            _sut.SetMovement(Vector2.right);
            var touchingDirections = new TouchingDirectionsStub
            {
                IsGrounded = false, // Need to still be airborne
                WallDirection = WallDirection.Right,
            };
            _sut.NotifyTouchingDirections(touchingDirections);
            _sut.FixedUpdate();
            Assert.IsInstanceOf<WallSlideState>(_sut.ActiveChildState);
            _sut.FixedUpdate(); // Execute 1 flame in WallSlideState.FixedUpdate
            Assert.AreEqual(-3f, _rigidBody2D.velocity.y, 0.01f);

            _preWallJumpForces = _rigidBody2D.totalForce;

            // ... and finally Jump:
            _sut.Jump();
            _sut.NotifyTouchingDirections(new TouchingDirectionsStub
            {
                IsGrounded = false,
                WallDirection = WallDirection.Right,
            });
            _sut.FixedUpdate(); // FixedUpdate() required due to physics interactions!
        }

        protected override void AssertInitialStateConditions()
        {
            Assert.IsInstanceOf<AirialState>(_sut.CurrentState);
            Assert.IsInstanceOf<WallJumpState>(_sut.ActiveChildState);
        }

        [Test]
        public void WhenUpdate_ThenAnimationIsSetToMikeJump()
        {
            _sut.Update();

            Assert.AreEqual(AnimationClipNames.Jump, _animator.CurrentPlayingAnimationClip);
        }

        [Test]
        public void WhenFixedUpdate_ThenRigidBodyHasGravityScalingOne()
        {
            _sut.FixedUpdate();

            Assert.AreEqual(ExpectedJumpGravityScale, _rigidBody2D.gravityScale);
        }

        [Test]
        public void WehnFixedUpdate_ThenRigidBodyHasJumpVelocitiesSet()
        {
            // Jump strength of 10, at 45 degrees towards the right:
            // Total Accelerating force: 10 (PlayerConfiguration.JumpImpulse)
            // Force at 45 degrees to the left for x: 10 * -.707 = -7.07
            // Force at 45 degrees to the left for y: 10 * .707 = 7.07
            Assert.AreEqual(0, _rigidBody2D.totalForce.x - _preWallJumpForces.x, 0.01, "Because Impulse forces do not affect RigidBody2D.totalForce");
            Assert.AreEqual(0, _rigidBody2D.totalForce.y - _preWallJumpForces.y, 0.01, "Because Impulse forces do not affect RigidBody2D.totalForce");
            Assert.AreEqual(-7.07f, _rigidBody2D.velocity.x, 0.001, "Because FixedUpdate applied the force as an impulse, causing an immediate adjustment of the velocity");
            Assert.AreEqual(7.07f, _rigidBody2D.velocity.y, 0.001, "Because FixedUpdate applied the force as an impulse, causing an immediate adjustment of the velicity");
        }

        [Test]
        public void ThenPlayerFacingIsLeft()
        {
            Assert.IsFalse(_playerFacing.IsFacingRight);
        }

        //TODO: Write regression tests:
        //TODO: Transition into falling
        //TODO: Cannot jump again while in WallJumpState
        //TODO: WallJumping into another wall does
        //TODO: While WallJumping, touch another wall an jump again: do nothing (Only WallSlide currently allows wall jumping)
        //TODO: Player movement input is ignored while in this state.
    }
}
