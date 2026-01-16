using UnityEngine;

public enum PlayerMovementState
{
    Grounded = 0,
    Airborne = 1,
    WallSliding = 2,
    Dashing = 3,
    WallJumping = 4,
}

[DisallowMultipleComponent]
[AddComponentMenu("Player/Player State Machine Runtime")]
public class PlayerStateMachine : MonoBehaviour
{
    public PlayerMovementState Current { get; private set; } = PlayerMovementState.Grounded;

    public bool IsGrounded => Current == PlayerMovementState.Grounded;
    public bool IsAirborne => Current == PlayerMovementState.Airborne;
    public bool IsWallSliding => Current == PlayerMovementState.WallSliding;
    public bool IsDashing => Current == PlayerMovementState.Dashing;
    public bool IsWallJumping => Current == PlayerMovementState.WallJumping;

    public void Set(PlayerMovementState state)
    {
        Current = state;
    }
}

