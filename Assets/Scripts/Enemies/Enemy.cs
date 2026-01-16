using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Access relevant components.
    protected Animator anim;
    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    protected PlayerMovement player;
    protected HealthManager healthManager;

    // "[SerializeFeild]" allows these variables to be edited in Unity.
    // Enemy variables.
    protected float recoilTimer;
    [SerializeField] protected float health;
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected bool isRecoiling = false;
    [SerializeField] protected float recoilLength;
    [SerializeField] protected float recoilFactor;
    [SerializeField] protected float moveSpeed = 5f;

    [Header("Player Collision Knockback")]
    [SerializeField] protected bool applyCollisionKnockback = true;
    [SerializeField] protected float collisionKnockbackX = 18f;
    [SerializeField] protected float collisionKnockbackY = 10f;
    [SerializeField] protected float collisionKnockbackLockSeconds = 0.18f;

    private bool isDead;

    // Start() is called before the first frame update.
    protected virtual void Start()
    {
        // Leave this empty so enemy types can override it.
    }

    // Awake() is called when the script instance is being loaded.
    // Awake() is used to initialize any variables or game states before the game starts.
    protected virtual void Awake()
    {
        // Access components once to save processing power.
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        player = PlayerMovement.Instance;
    }

    // Update() is called once per frame.
    protected virtual void Update()
    {
        // If enemy health is zero then die (overrideable).
        if (!isDead && health <= 0)
        {
            isDead = true;
            Die();
            return;
        }

        // Handles enemy recoil.
        if (isRecoiling)
        {
            if (recoilTimer < recoilLength)
            {
                recoilTimer += Time.deltaTime;
            }
            else
            {
                isRecoiling = false;
                recoilTimer = 0;
            }
        }
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    // Handles enemy being attacked..
    public virtual void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        if (_damageDone <= 0) { return; }

        // Do damage.
        health -= _damageDone;

        // Add recoil.
        if (!isRecoiling)
        {
            rb.AddForce(-_hitForce * recoilFactor * _hitDirection);
        }
    }

    // Handles enemy and player collision.
    protected void OnCollisionEnter2D(Collision2D collision)
    {
        // If object is the player AND player is not invincible.
        if (collision.gameObject.CompareTag("Player") && !PlayerMovement.Instance.pState.invincible)
        {
            // Hit the player.
            EnemyAttack(collision.gameObject);
            PlayerMovement.Instance.HitStopTime(0.25f, 5, 0.5f);
        }
    }

    // Continuously push the player out while colliding (prevents staying inside enemy colliders).
    protected void OnCollisionStay2D(Collision2D collision)
    {
        if (!applyCollisionKnockback) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        ApplyCollisionKnockback(collision.gameObject);
    }

    // If an enemy uses trigger colliders (e.g., boss inner trigger), keep pushing player out.
    protected void OnTriggerStay2D(Collider2D other)
    {
        if (!applyCollisionKnockback) return;
        if (!other.CompareTag("Player")) return;

        ApplyCollisionKnockback(other.gameObject);
    }

    // Handles enemy attack.
    protected virtual void EnemyAttack(GameObject entity)
    {
        // If the collision is with a player that is not invincible and alive.
        if (entity.gameObject.CompareTag("Player")
            && !PlayerMovement.Instance.pState.invincible
            && PlayerMovement.Instance.pState.alive
            )
        {
            // Do damage.
            healthManager = entity.gameObject.GetComponent<HealthManager>();
            healthManager.TakeDamage(damage);

            // Bounce player away from the enemy on contact.
            ApplyCollisionKnockback(entity);
        }
    }

    protected virtual void ApplyCollisionKnockback(GameObject playerObject)
    {
        if (!applyCollisionKnockback) return;
        if (playerObject == null) return;

        Rigidbody2D playerRb = playerObject.GetComponent<Rigidbody2D>();
        if (playerRb == null) return;

        Vector2 away = (playerRb.position - rb.position);
        if (away.sqrMagnitude < 0.0001f)
        {
            // Fallback if positions are identical.
            float sign = playerRb.transform.position.x >= transform.position.x ? 1f : -1f;
            away = new Vector2(sign, 0f);
        }
        away.Normalize();

        Vector2 knockVel = new Vector2(away.x * collisionKnockbackX, collisionKnockbackY);

        // If the refactored motor exists, use it so knockback isn't overwritten by movement.
        PlayerMotor motor = playerObject.GetComponent<PlayerMotor>();
        if (motor != null)
        {
            motor.ApplyKnockback(knockVel, collisionKnockbackLockSeconds);
            return;
        }

        // Fallback: set velocity directly.
        playerRb.velocity = knockVel;
    }
}