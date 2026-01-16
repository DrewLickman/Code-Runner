using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("Player/Player Combat")]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerMovementConfig))]
public class PlayerCombat : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private PlayerMovementConfig config;
    private PlayerStatesList pState;
    private PlayerMotor motor;
    private PlayerAbilities abilities;

    private bool playerClickedAttack;
    private float timeSinceAttack;

    private int stepsXRecoiled;
    private int stepsYRecoiled;

    private bool restoreTime;
    private float restoreTimeSpeed;
    private float gravity;
    private bool wasRecoilingY;

    public void Init(PlayerMovementConfig cfg, PlayerStatesList states, PlayerMotor m, Rigidbody2D body, SpriteRenderer sr)
    {
        config = cfg;
        pState = states;
        motor = m;
        rb = body;
        sprite = sr;

        if (rb != null) gravity = rb.gravityScale;
    }

    public void SetAbilities(PlayerAbilities a)
    {
        abilities = a;
    }

    public void Tick(PlayerInputIntent input)
    {
        if (config == null || rb == null || pState == null) return;

        playerClickedAttack = input.AttackDown;

        TryAttack(input.Vertical);
        RestoreTimeScale();
        FlashWhileInvincible();
    }

    public void FixedTick(float vertical)
    {
        if (config == null || rb == null || pState == null) return;
        TryRecoil(vertical);
    }

    // Initiates change in time scale based on when player takes damage.
    public void HitStopTime(float newTimeScale, int restoreSpeed, float delay)
    {
        restoreTimeSpeed = restoreSpeed;
        Time.timeScale = newTimeScale;

        if (delay > 0)
        {
            StopCoroutine(StartTimeAgain(delay));
            StartCoroutine(StartTimeAgain(delay));
        }
        else
        {
            restoreTime = true;
        }
    }

    private IEnumerator StartTimeAgain(float delay)
    {
        restoreTime = true;
        yield return new WaitForSeconds(delay);
    }

    private void RestoreTimeScale()
    {
        if (!restoreTime) return;

        if (Time.timeScale < 1)
        {
            Time.timeScale += Time.deltaTime * restoreTimeSpeed;
        }
        else
        {
            Time.timeScale = 1;
            restoreTime = false;
        }
    }

    private void FlashWhileInvincible()
    {
        if (sprite == null) return;
        if (pState == null) return;

        sprite.material.color = pState.invincible
            ? Color.Lerp(Color.white, Color.black, Mathf.PingPong(Time.time * config.hitFlashSpeed, 1.0f))
            : Color.white;
    }

    // Player attack handler.
    private void TryAttack(float vertical)
    {
        timeSinceAttack = Time.deltaTime;
        if (!playerClickedAttack) return;

        timeSinceAttack = 0;

        // If player is on the ground then side attack and display slash effect.
        if (vertical == 0)
        {
            ChooseAttackDirection("Side");
        }
        else if (vertical > 0)
        {
            ChooseAttackDirection("Up");
        }
        else if (motor != null && !motor.IsGrounded() && vertical < 0)
        {
            ChooseAttackDirection("Down");
        }
    }

    // Allow the programmer to write a readable direction for the attack.
    private void ChooseAttackDirection(string attackDirection)
    {
        if (attackDirection == "Side")
        {
            AttackDirection(config.SideAttackTransform, config.SideAttackArea, ref pState.recoilingX, config.recoilXSpeed, config.slashEffect, 0, config.attackSound);
        }
        else if (attackDirection == "Up")
        {
            AttackDirection(config.UpAttackTransform, config.UpAttackArea, ref pState.recoilingY, config.recoilYSpeed, config.slashEffect, 80, config.attackSound);
        }
        else if (attackDirection == "Down")
        {
            AttackDirection(config.DownAttackTransform, config.DownAttackArea, ref pState.recoilingY, config.recoilYSpeed, config.slashEffect, -90, config.attackSound);
        }
    }

    private void AttackDirection(Transform positionArea, Vector2 distance, ref bool recoilDirection, float recoilSpeed, GameObject visualEffect, int effectAngle, AudioSource soundEffect)
    {
        if (soundEffect != null) soundEffect.Play();
        Hit(positionArea, distance, ref recoilDirection, recoilSpeed);
        SlashEffectAtAngle(visualEffect, effectAngle, positionArea);
    }

    private void Hit(Transform attackTransform, Vector2 attackArea, ref bool recoilDir, float recoilStrength)
    {
        if (attackTransform == null) return;

        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(attackTransform.position, attackArea, 0, config.attackableLayer);

        if (objectsToHit.Length < 1) return;

        recoilDir = true;

        // If player hit enemy, allow another air jump and dash (matches old behavior).
        abilities?.OnEnemyHit();

        // If player hit enemy, allow another air jump and dash (matches old behavior).
        // Note: canDoubleJump / canDash are owned by PlayerAbilities now; those resets happen via player state elsewhere.

        for (int i = 0; i < objectsToHit.Length; i++)
        {
            objectsToHit[i]?.GetComponent<Enemy>()?.EnemyHit(config.damage, (transform.position - objectsToHit[i].transform.position).normalized, recoilStrength);
        }
    }

    private void SlashEffectAtAngle(GameObject slashEffectPrefab, int effectAngle, Transform attackTransform)
    {
        if (slashEffectPrefab == null || attackTransform == null) return;

        GameObject slash = Instantiate(slashEffectPrefab, attackTransform);

        if (effectAngle == 0) return;

        slash.transform.eulerAngles = new Vector3(0, 0, effectAngle);
        slash.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
    }

    private void TryRecoil(float vertical)
    {
        if (pState.recoilingX)
        {
            // Using transform scale sign to match the old left/right recoil behavior.
            bool facingRight = transform.localScale.x > 0;
            rb.velocity = facingRight ? new Vector2(-config.recoilXSpeed, 0) : new Vector2(config.recoilXSpeed, 0);
        }

        if (pState.recoilingY)
        {
            rb.gravityScale = 0;
            rb.velocity = vertical < 0 ? new Vector2(rb.velocity.x, config.recoilYSpeed) : new Vector2(rb.velocity.x, -config.recoilYSpeed);
            wasRecoilingY = true;
        }
        else if (wasRecoilingY)
        {
            // Only restore gravity if we were the ones who zeroed it (prevents overriding swing/dash gravity changes).
            rb.gravityScale = gravity;
            wasRecoilingY = false;
        }

        if (pState.recoilingX && stepsXRecoiled < config.recoilXSteps) stepsXRecoiled++;
        else StopRecoilX();

        if (pState.recoilingY && stepsYRecoiled < config.recoilYSteps) stepsYRecoiled++;
        else StopRecoilY();

        if (motor != null && motor.IsGrounded()) StopRecoilY();
    }

    private void StopRecoilX()
    {
        stepsXRecoiled = 0;
        pState.recoilingX = false;
    }

    private void StopRecoilY()
    {
        stepsYRecoiled = 0;
        pState.recoilingY = false;
    }
}

