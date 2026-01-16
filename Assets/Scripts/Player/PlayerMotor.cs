using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("Player/Player Motor")]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerMovementConfig))]
public class PlayerMotor : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private PlayerMovementConfig config;

    private float currentMoveSpeed;
    private float knockbackLockTimer;

    public bool IsFacingRight => rb != null ? rb.transform.localScale.x >= 0f : transform.localScale.x >= 0f;
    public bool IsKnockbackLocked => knockbackLockTimer > 0f;

    public void Init(PlayerMovementConfig cfg, Animator animator, Rigidbody2D body)
    {
        config = cfg;
        anim = animator;
        rb = body;
        currentMoveSpeed = config != null ? config.moveSpeed : 10f;
    }

    public bool IsGrounded()
    {
        if (config == null || config.groundCheck == null) return false;
        return Physics2D.OverlapCircle(config.groundCheck.position, 0.2f, config.groundLayer);
    }

    public bool IsWalled()
    {
        if (config == null || config.wallCheck == null) return false;
        return Physics2D.OverlapCircle(config.wallCheck.position, 0.1f, config.wallLayer);
    }

    public void TickSlowEffect(PlayerStatesList pState)
    {
        if (config == null) return;

        if (pState != null && pState.slowed)
        {
            currentMoveSpeed = config.moveSpeed * config.slowRatio;
            if (anim != null) anim.speed = config.slowRatio;
        }
        else
        {
            currentMoveSpeed = config.moveSpeed;
            if (anim != null) anim.speed = 1.0f;
        }
    }

    public void FixedTickHorizontal(float horizontal, bool blockMovement)
    {
        if (rb == null) return;
        if (blockMovement) return;
        if (rb.bodyType == RigidbodyType2D.Static) return;

        if (knockbackLockTimer > 0f)
        {
            knockbackLockTimer -= Time.fixedDeltaTime;
            return;
        }

        float targetX = horizontal * currentMoveSpeed;

        float lerpRate = 18f;
        if (config != null)
        {
            // Preserve momentum in air more than on ground.
            lerpRate = IsGrounded() ? config.groundHorizontalLerp : config.airHorizontalLerp;
        }

        // Exponential smoothing (frame-rate independent).
        float t = lerpRate <= 0f ? 1f : (1f - Mathf.Exp(-lerpRate * Time.fixedDeltaTime));
        float newX = Mathf.Lerp(rb.velocity.x, targetX, t);

        rb.velocity = new Vector2(newX, rb.velocity.y);
    }

    public void ApplyKnockback(Vector2 velocity, float lockSeconds)
    {
        if (rb == null) return;
        rb.velocity = velocity;
        knockbackLockTimer = Mathf.Max(knockbackLockTimer, lockSeconds);
    }

    public void FaceDirection(float directionX)
    {
        if (rb == null) return;
        if (Mathf.Abs(directionX) < 0.0001f) return;

        Vector3 localScale = rb.transform.localScale;
        float sign = Mathf.Sign(directionX);
        localScale.x = Mathf.Abs(localScale.x) * sign;
        rb.transform.localScale = localScale;
    }

    public void Flip(float horizontal, bool blockFlip)
    {
        if (rb == null) return;
        if (blockFlip) return;
        if (knockbackLockTimer > 0f) return;

        if (horizontal < 0f) FaceDirection(-1f);
        else if (horizontal > 0f) FaceDirection(1f);
    }
}

