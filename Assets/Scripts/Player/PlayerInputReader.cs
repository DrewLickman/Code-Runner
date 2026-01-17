using UnityEngine;

public struct PlayerInputIntent
{
    public float Horizontal;
    public float Vertical;
    public bool JumpDown;
    public bool JumpHeld;
    public bool JumpUp;
    public bool DashDown;
    public bool AttackDown;
}

public class PlayerInputReader : MonoBehaviour
{
    public PlayerInputIntent Current { get; private set; }

    public void Tick(bool ignoreUserInput)
    {
        if (ignoreUserInput)
        {
            Current = default;
            return;
        }

        bool jumpDown = Input.GetButtonDown("Jump");
        Current = new PlayerInputIntent
        {
            AttackDown = Input.GetButtonDown("Attack"),
            Vertical = Input.GetAxisRaw("Vertical"),
            Horizontal = Input.GetAxisRaw("Horizontal"),
            JumpDown = jumpDown,
            JumpHeld = Input.GetButton("Jump"),
            JumpUp = Input.GetButtonUp("Jump"),
            DashDown = Input.GetButtonDown("Dash"),
        };
    }
}

