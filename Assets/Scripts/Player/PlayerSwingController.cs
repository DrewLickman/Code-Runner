using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerSwingController : MonoBehaviour
{
    [Header("Grab")]
    [Tooltip("How far the player can be from the grip rigidbody to attach.")]
    public float maxAttachDistance = 1.25f;

    [Tooltip("If true, horizontal motor movement is blocked while swinging.")]
    public bool blockMotorWhileSwinging = true;

    [Header("Swing Drive")]
    [Tooltip("Horizontal force applied while swinging to let the player pump the swing.")]
    public float swingDriveForce = 35f;

    [Tooltip("Temporarily blocks the motor after detaching so momentum carries.")]
    public float releaseMotorLockSeconds = 0.18f;

    [Header("Release Motor Lock (velocity-based)")]
    [Tooltip("If enabled, extends the motor-lock duration based on horizontal release speed so you keep swing momentum longer.")]
    public bool useVelocityBasedReleaseMotorLock = true;

    [Tooltip("Horizontal speed at/under which we use the min lock time.")]
    public float releaseHorizSpeedMin = 3f;

    [Tooltip("Horizontal speed at/over which we use the max lock time.")]
    public float releaseHorizSpeedMax = 18f;

    [Tooltip("Motor lock time (seconds) when horizontal release speed is low.")]
    public float releaseMotorLockMin = 0.12f;

    [Tooltip("Motor lock time (seconds) when horizontal release speed is high.")]
    public float releaseMotorLockMax = 0.55f;

    [Tooltip("Gravity scale multiplier while attached (lower = higher arcs for same speed).")]
    [Range(0.1f, 1.5f)]
    public float swingGravityMultiplier = 0.7f;

    [Header("Gravity Profile (optional)")]
    [Tooltip("If enabled, uses different gravity multipliers when moving up vs down for better hang-time without impulses.")]
    public bool useUpDownGravityProfile = true;

    [Tooltip("Gravity multiplier while swinging/releasing and moving upward (y > 0).")]
    [Range(0.1f, 1.5f)]
    public float upGravityMultiplier = 0.55f;

    [Tooltip("Gravity multiplier while swinging/releasing and moving downward (y <= 0).")]
    [Range(0.1f, 1.5f)]
    public float downGravityMultiplier = 0.85f;

    [Header("Release Gravity (velocity-based)")]
    [Tooltip("On detach, set gravityScale to 0 and lerp back to the saved gravityScale based on upward release speed.")]
    public bool useVelocityBasedReleaseGravity = true;

    [Tooltip("Upward speed at/under which we use the fast recover time.")]
    public float releaseUpSpeedMin = 1.0f;

    [Tooltip("Upward speed at/over which we use the slow recover time.")]
    public float releaseUpSpeedMax = 16.0f;

    [Tooltip("Recover time (seconds) when upward release speed is low.")]
    public float releaseRecoverTimeFast = 0.10f;

    [Tooltip("Recover time (seconds) when upward release speed is high.")]
    public float releaseRecoverTimeSlow = 0.55f;

    [Header("Release Upward Assist (smooth)")]
    [Tooltip("Adds a small upward acceleration after release (applied as force over time, not an instant velocity jump).")]
    public bool enableReleaseUpwardAssist = true;

    [Tooltip("How long (seconds) to apply the upward assist after release.")]
    public float releaseUpwardAssistSeconds = 0.12f;

    [Tooltip("Upward force applied during assist. Increase slightly for more pop.")]
    public float releaseUpwardAssistForce = 35f;

    [Tooltip("Only apply upward assist if upward velocity on release exceeds this.")]
    public float releaseUpwardAssistMinUpSpeed = 1.0f;

    [Header("Release Horizontal Assist (smooth)")]
    [Tooltip("Adds a small horizontal acceleration after release (applied as force over time).")]
    public bool enableReleaseHorizontalAssist = true;

    [Tooltip("How long (seconds) to apply the horizontal assist after release.")]
    public float releaseHorizontalAssistSeconds = 0.14f;

    [Tooltip("Horizontal force applied during assist (in the direction of release velocity).")]
    public float releaseHorizontalAssistForce = 14f;

    [Tooltip("Only apply horizontal assist if horizontal speed on release exceeds this.")]
    public float releaseHorizontalAssistMinSpeed = 4.0f;

    [Header("Detach Velocity")]
    [Tooltip("On detach, overwrite the player's velocity with the swing/grip point velocity to preserve momentum.")]
    public bool inheritGripPointVelocityOnDetach = true;

    [Header("Optional Assist (default off)")]
    [Tooltip("If enabled, adds a small upward assist on detach. Leave off for purely smooth carry-through.")]
    public bool enableDetachUpwardAssist = false;

    [Tooltip("Multiplier for upward velocity at detach. Recommended range: 1.05 - 1.25.")]
    [Min(1f)]
    public float detachUpwardVelocityMultiplier = 1.12f;

    [Tooltip("Minimum upward speed required before applying detachUpwardVelocityMultiplier.")]
    public float detachUpwardMinSpeed = 0.5f;

    private Rigidbody2D rb;
    private HingeJoint2D hinge;
    private SwingGrip nearbyGrip;
    private bool isSwinging;
    private float desiredHorizontal;
    private float releaseMotorLockTimer;
    private float originalGravityScale;

    private bool releaseGravityRecovering;
    private float releaseGravityDefault;
    private float releaseGravityRecoverDuration;
    private float releaseGravityRecoverElapsed;

    private float releaseUpwardAssistTimer;
    private float releaseHorizontalAssistTimer;
    private float releaseHorizontalAssistSign;

    public bool IsSwinging => isSwinging;
    public bool ShouldBlockMotor => (blockMotorWhileSwinging && isSwinging) || releaseMotorLockTimer > 0f;
    public bool ShouldIgnoreJumpCut => isSwinging || releaseMotorLockTimer > 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null) originalGravityScale = rb.gravityScale;

        // Create a hinge joint on the player for grabbing grips.
        hinge = GetComponent<HingeJoint2D>();
        if (hinge == null) hinge = gameObject.AddComponent<HingeJoint2D>();
        hinge.enabled = false;
        hinge.autoConfigureConnectedAnchor = false;
        hinge.anchor = Vector2.zero;
        hinge.enableCollision = false;
    }

    public void Tick(PlayerInputIntent input)
    {
        if (rb == null) return;

        desiredHorizontal = input.Horizontal;
        if (releaseMotorLockTimer > 0f) releaseMotorLockTimer -= Time.deltaTime;

        if (!input.JumpHeld && isSwinging)
        {
            Detach();
            return;
        }

        if (input.JumpHeld && !isSwinging && nearbyGrip != null)
        {
            float dist = Vector2.Distance(rb.position, nearbyGrip.Body.position);
            if (dist <= maxAttachDistance)
            {
                Attach(nearbyGrip);
            }
        }
    }

    private void Attach(SwingGrip grip)
    {
        if (grip == null || grip.Body == null) return;

        hinge.connectedBody = grip.Body;
        hinge.connectedAnchor = Vector2.zero;
        hinge.anchor = Vector2.zero;
        hinge.enabled = true;
        isSwinging = true;

        // Reduce gravity while attached to support higher arcs.
        originalGravityScale = rb.gravityScale;
        float m = GetActiveGravityMultiplier(rb.velocity.y);
        rb.gravityScale = originalGravityScale * m;
        releaseGravityRecovering = false;

        // Track grab state for rope damping.
        grip.SetGrabbed(true);
    }

    private void Detach()
    {
        // Capture momentum from the connected body at the player's attach point.
        Vector2 playerVelocity = rb.velocity;
        Vector2 releaseVelocity = playerVelocity;
        Rigidbody2D connected = hinge != null ? hinge.connectedBody : null;
        if (inheritGripPointVelocityOnDetach && connected != null)
        {
            Vector2 gripVelocity = connected.GetPointVelocity(rb.worldCenterOfMass);

            // Never "steal" momentum by overwriting with a slower value.
            releaseVelocity = gripVelocity.sqrMagnitude > playerVelocity.sqrMagnitude ? gripVelocity : playerVelocity;
        }

        // Track grab state for rope damping.
        if (nearbyGrip != null) nearbyGrip.SetGrabbed(false);

        hinge.enabled = false;
        hinge.connectedBody = null;
        isSwinging = false;

        // Prevent the motor from overwriting momentum (optionally scaled by release speed).
        float lockSeconds = releaseMotorLockSeconds;
        if (useVelocityBasedReleaseMotorLock)
        {
            float hs = Mathf.Abs(releaseVelocity.x);
            float a = Mathf.InverseLerp(releaseHorizSpeedMin, releaseHorizSpeedMax, hs);
            lockSeconds = Mathf.Lerp(releaseMotorLockMin, releaseMotorLockMax, a);
        }
        releaseMotorLockTimer = Mathf.Max(releaseMotorLockTimer, lockSeconds);

        // Release gravity behavior:
        // - While swinging we use the profile (up/down multipliers)
        // - On detach we optionally set gravityScale=0 and recover over time based on upward speed
        if (useVelocityBasedReleaseGravity)
        {
            releaseGravityDefault = originalGravityScale;
            releaseGravityRecoverElapsed = 0f;

            float upSpeed = Mathf.Max(0f, releaseVelocity.y);
            float a = Mathf.InverseLerp(releaseUpSpeedMin, releaseUpSpeedMax, upSpeed);
            releaseGravityRecoverDuration = Mathf.Lerp(releaseRecoverTimeFast, releaseRecoverTimeSlow, a);
            releaseGravityRecoverDuration = Mathf.Max(0.0001f, releaseGravityRecoverDuration);

            rb.gravityScale = 0f;
            releaseGravityRecovering = true;
        }
        else
        {
            // Fallback: keep the configured profile immediately on release.
            rb.gravityScale = originalGravityScale * GetActiveGravityMultiplier(releaseVelocity.y);
            releaseGravityRecovering = false;
        }

        // Smooth upward assist (optional): gives a bit more "pop" without a velocity impulse.
        if (enableReleaseUpwardAssist && releaseVelocity.y > releaseUpwardAssistMinUpSpeed)
        {
            releaseUpwardAssistTimer = Mathf.Max(releaseUpwardAssistTimer, releaseUpwardAssistSeconds);
        }
        else
        {
            releaseUpwardAssistTimer = 0f;
        }

        // Smooth horizontal assist after release (in direction of release velocity).
        if (enableReleaseHorizontalAssist && Mathf.Abs(releaseVelocity.x) > releaseHorizontalAssistMinSpeed)
        {
            releaseHorizontalAssistTimer = Mathf.Max(releaseHorizontalAssistTimer, releaseHorizontalAssistSeconds);
            releaseHorizontalAssistSign = Mathf.Sign(releaseVelocity.x);
        }
        else
        {
            releaseHorizontalAssistTimer = 0f;
        }

        // Ensure we keep the swing's velocity at the moment of release.
        if (enableDetachUpwardAssist && releaseVelocity.y > detachUpwardMinSpeed)
            releaseVelocity = new Vector2(releaseVelocity.x, releaseVelocity.y * detachUpwardVelocityMultiplier);
        rb.velocity = releaseVelocity;
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        // Gravity behavior while attached (and only when not in a velocity-based recovery).
        if (isSwinging && !(useVelocityBasedReleaseGravity && releaseGravityRecovering))
        {
            float m = GetActiveGravityMultiplier(rb.velocity.y);
            rb.gravityScale = originalGravityScale * m;
        }

        // Gravity recovery after release (velocity-based).
        if (useVelocityBasedReleaseGravity && releaseGravityRecovering && !isSwinging)
        {
            releaseGravityRecoverElapsed += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(releaseGravityRecoverElapsed / releaseGravityRecoverDuration);
            rb.gravityScale = Mathf.Lerp(0f, releaseGravityDefault, t);
            if (t >= 1f)
            {
                rb.gravityScale = releaseGravityDefault;
                releaseGravityRecovering = false;
            }
        }

        // Smooth upward assist after release.
        if (!isSwinging && enableReleaseUpwardAssist && releaseUpwardAssistTimer > 0f)
        {
            rb.AddForce(Vector2.up * releaseUpwardAssistForce, ForceMode2D.Force);
            releaseUpwardAssistTimer -= Time.fixedDeltaTime;
            if (releaseUpwardAssistTimer <= 0f) releaseUpwardAssistTimer = 0f;
        }

        // Smooth horizontal assist after release.
        if (!isSwinging && enableReleaseHorizontalAssist && releaseHorizontalAssistTimer > 0f)
        {
            rb.AddForce(Vector2.right * (releaseHorizontalAssistSign * releaseHorizontalAssistForce), ForceMode2D.Force);
            releaseHorizontalAssistTimer -= Time.fixedDeltaTime;
            if (releaseHorizontalAssistTimer <= 0f) releaseHorizontalAssistTimer = 0f;
        }

        // Steering during the motor-lock window (so you can still adjust midair without killing momentum).
        if (!isSwinging && releaseMotorLockTimer > 0f && Mathf.Abs(desiredHorizontal) > 0.01f)
        {
            rb.AddForce(Vector2.right * (desiredHorizontal * 10f), ForceMode2D.Force);
        }

        if (!isSwinging) return;
        if (Mathf.Abs(desiredHorizontal) < 0.01f) return;

        // Apply force along the swing tangent so input "pumps" energy into the pendulum.
        Rigidbody2D connected = hinge != null ? hinge.connectedBody : null;
        if (connected == null)
        {
            rb.AddForce(Vector2.right * (desiredHorizontal * swingDriveForce), ForceMode2D.Force);
            return;
        }

        Vector2 r = rb.position - connected.position;
        if (r.sqrMagnitude < 0.0001f)
        {
            rb.AddForce(Vector2.right * (desiredHorizontal * swingDriveForce), ForceMode2D.Force);
            return;
        }

        Vector2 tangent = new Vector2(-r.y, r.x).normalized;
        if (tangent.x * desiredHorizontal < 0f) tangent = -tangent; // align with desired horizontal direction

        rb.AddForce(tangent * (Mathf.Abs(desiredHorizontal) * swingDriveForce), ForceMode2D.Force);
    }

    private float GetActiveGravityMultiplier(float velocityY)
    {
        if (!useUpDownGravityProfile) return swingGravityMultiplier;
        return velocityY > 0.01f ? upGravityMultiplier : downGravityMultiplier;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        SwingGrip grip = other.GetComponent<SwingGrip>();
        if (grip != null) nearbyGrip = grip;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        SwingGrip grip = other.GetComponent<SwingGrip>();
        if (grip != null && nearbyGrip == grip) nearbyGrip = null;
    }
}

