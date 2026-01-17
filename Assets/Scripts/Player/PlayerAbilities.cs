using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("Player/Player Abilities")]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerMotor))]
[RequireComponent(typeof(PlayerMovementConfig))]
[RequireComponent(typeof(PlayerStateMachine))]
public class PlayerAbilities : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerMotor motor;
    private PlayerMovementConfig config;
    private PlayerStateMachine stateMachine;
    private PlayerSwingController swing;

    private bool canDoubleJump = true;

    private bool isWallSliding;
    private bool isWallJumping;
    private float wallJumpingCounter;
    private float wallJumpingDirection;

    private bool canDash = true;
    private bool isDashing;
    private float timeSinceDash = 1f;

    public bool IsWallSliding => isWallSliding;
    public bool IsWallJumping => isWallJumping;
    public bool IsDashing => isDashing;

    public float TimeSinceDash => timeSinceDash;
    public float DashingCooldown => config != null ? config.dashingCooldown : 0.75f;

    public void Init(PlayerMovementConfig cfg, PlayerMotor m, PlayerStateMachine sm, Rigidbody2D body)
    {
        config = cfg;
        motor = m;
        stateMachine = sm;
        rb = body;
        swing = GetComponent<PlayerSwingController>();
    }

    public void Tick(PlayerInputIntent input, PlayerStatesList pState)
    {
        if (rb == null || motor == null || config == null) return;

        timeSinceDash += Time.deltaTime;

        // If we're in knockback, don't allow movement abilities to override it.
        // We still update UI/state so the game remains consistent.
        if (motor.IsKnockbackLocked)
        {
            UpdateDashUi();
            UpdateStateMachine();
            return;
        }

        // While dashing, everything else is blocked (matches old behavior).
        if (isDashing)
        {
            stateMachine?.Set(PlayerMovementState.Dashing);
            return;
        }

        ResetDoubleJump();
        WallSlide(input.Horizontal);
        WallJump(input.JumpDown);
        TryJump(input.JumpDown);
        ReduceJumpHeightOnRelease(input.JumpUp);
        TryDash(input.DashDown);
        UpdateDashUi();

        // Keep state machine in sync with locomotion.
        UpdateStateMachine();
    }

    // Matches the old PlayerMovement behavior: hitting an enemy refreshes air jump + dash.
    public void OnEnemyHit()
    {
        canDoubleJump = true;
        canDash = true;
        timeSinceDash += 1f;
    }

    private void ResetDoubleJump()
    {
        if (motor.IsGrounded()) canDoubleJump = true;
    }

    private void TryJump(bool jumpPressed)
    {
        if (!jumpPressed || !canDoubleJump)
        {
            return;
        }

        bool wasGrounded = motor.IsGrounded();
        if (!wasGrounded) canDoubleJump = false;

        rb.velocity = new Vector2(rb.velocity.x, config.jumpingPower);

        if (wasGrounded)
        {
            if (config.jumpSound != null) config.jumpSound.Play();
        }
        else if (!isWallJumping)
        {
            if (config.doubleJumpSound != null) config.doubleJumpSound.Play();
        }
    }

    private void ReduceJumpHeightOnRelease(bool jumpReleased)
    {
        if (!jumpReleased) return;
        if (swing != null && swing.ShouldIgnoreJumpCut) return;
        if (rb.velocity.y <= 0f) return;

        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
    }

    private void TryDash(bool dashPressed)
    {
        if (!dashPressed || !canDash || isWallSliding) return;

        StartCoroutine(Dash());
        if (config.dashSound != null) config.dashSound.Play();
        timeSinceDash = 0f;
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        rb.velocity = new Vector2(transform.localScale.x * config.dashingPower, 0f);

        yield return new WaitForSeconds(config.dashingTime);

        rb.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(config.dashingCooldown);

        if (timeSinceDash >= config.dashingCooldown) canDash = true;
    }

    private void WallSlide(float horizontal)
    {
        if (motor.IsWalled() && !motor.IsGrounded() && horizontal != 0f)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -config.wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void WallJump(bool jumpPressed)
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = -Mathf.Sign(transform.localScale.x);
            wallJumpingCounter = config.wallJumpingTime;
            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if (!jumpPressed || wallJumpingCounter <= 0f) return;

        isWallJumping = true;
        if (config.wallJumpSound != null) config.wallJumpSound.Play();

        rb.velocity = new Vector2(wallJumpingDirection * config.wallJumpingPower.x, config.wallJumpingPower.y);
        wallJumpingCounter = 0f;
        Invoke(nameof(StopWallJumping), config.wallJumpingDuration);

        // Ensure facing is synced through the motor (prevents post-walljump flip desync).
        motor.FaceDirection(wallJumpingDirection);
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    private void UpdateDashUi()
    {
        if (config.dashIcon == null) return;
        float cooldown = Mathf.Max(0.0001f, config.dashingCooldown);
        config.dashIcon.fillAmount = timeSinceDash != 0 ? Mathf.Clamp01(timeSinceDash / cooldown) : 0;
    }

    private void UpdateStateMachine()
    {
        if (stateMachine == null || motor == null || rb == null) return;

        if (isDashing)
        {
            stateMachine.Set(PlayerMovementState.Dashing);
        }
        else if (isWallJumping)
        {
            stateMachine.Set(PlayerMovementState.WallJumping);
        }
        else if (isWallSliding)
        {
            stateMachine.Set(PlayerMovementState.WallSliding);
        }
        else if (motor.IsGrounded())
        {
            stateMachine.Set(PlayerMovementState.Grounded);
        }
        else
        {
            stateMachine.Set(PlayerMovementState.Airborne);
        }
    }
}

