using System.Collections;
using System.Collections.Generic;
using Assets.Characters.Player;
using Assets.GeneralScripts;
using UnityEngine;

public class PlayerConfiguration : MonoBehaviour
{
    [Tooltip("Determines the maximum run-speed")]
    public float MaxRunSpeed = 10f;
    [Tooltip("Determines the maximum walk-speed")]
    public float MaxWalkSpeed = 5f;
    [Tooltip("Determines the lateral input dead-zone")]
    public float LateralInputDeadZone = 0.1f;

    [Tooltip("Determines the maximum walk-speed while crouched")]
    public float MaxCrouchWalkSpeed = 3f;
    [Tooltip("Determines the y-input level for detecting 'crouch input'")]
    public float CrouchInputZone = -0.5f;
    [Tooltip("Determines the crouch lateral input dead-zone")]
    public float CrouchLateralInputDeadZone = 0.1f;

    [Tooltip("Determines the jump strength")]
    public float JumpImpulse = 10f;

    [Tooltip("Wall Sliding gravity scale")]
    public float WallSlideGavityScaling = 0.5f;
    [Tooltip("Determines the maximum Wall-Sliding speed")]
    public float MaxWallSlideSpeed = -3f;

    [Tooltip("Jumping gravity scale")]
    public float JumpingGravityScale = 1f;

    [Tooltip("Falling gravity scale")]
    public float FallingGravityScale = 2f;

    /// <remarks>
    /// <para>Average Clicks-per-second for people is about 6.5, which would mean that a jump buffer of ~0.15 on average would leave no gaps when spamming the button.</para>
    /// <para>UX Research indicated that a <=0.1s response time is typically experienced as 'instantaneous'.</para>
    /// </remarks>
    [Tooltip("Jump input buffer, in seconds")]
    public float JumpBuffer = 0.1f;

    [Tooltip("Jump input buffer after leaving ground, in seconds")]
    public float CoyoteTimeBuffer = 0.1f;

    private void Awake()
    {
        Debug.Assert(MaxRunSpeed > 0, "'MaxRunSpeed' must be greater than 0!");
        Debug.Assert(MaxWalkSpeed > 0, "'MaxWalkSpeed' must be greater than 0!");
        Debug.Assert(MaxWalkSpeed <= MaxRunSpeed, "'MaxWalkSpeed' must be less than or equal to 'MaxRunSpeed'!");
        Debug.Assert(LateralInputDeadZone >= 0, "'LateralInputDeadZone' must be greater than 0!");
        Debug.Assert(JumpImpulse > 0, "'JumpImpulse' must be greater than 0!");
        Debug.Assert(JumpingGravityScale > 0, "'JumpingGravityScale' must be greater than 0!");
        Debug.Assert(FallingGravityScale > 0, "'FallingGravityScale' must be greater than 0!");
        Debug.Assert(WallSlideGavityScaling > 0, "'WallSlideGavityScaling' must be less than or equal to 0!");
        Debug.Assert(JumpBuffer >= 0, "'JumpBuffer' must be greater than or equal to 0!");
        Debug.Assert(MaxCrouchWalkSpeed > 0, "'MaxChrouchWalkSpeed' must be greater than 0!");
        Debug.Assert(-1f <= CrouchInputZone && CrouchInputZone <= 0.0f, "'CrouchInputZone' must be in range [-1.0, 0.0]!");
        Debug.Assert(CrouchLateralInputDeadZone >= 0, "'CrouchLateralInputDeadZone' must be greater than 0!");
        Debug.Assert(MaxWallSlideSpeed < 0, "'MaxWallSlideSpeed' must be less than 0!");
    }
}
