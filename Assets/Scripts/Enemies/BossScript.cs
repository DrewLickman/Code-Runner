using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BossScript : Enemy
{
    [Header("Boss Root")]
    [Tooltip("RigidBody to drive (the bomb the boss rides). If empty, uses this GameObject Rigidbody2D.")]
    [SerializeField] private GameObject bomb;

    [Header("Movement Pattern")]
    [SerializeField] private float baseJumpForce = 30f;
    [SerializeField] private float baseJumpFrequency = 1.75f;
    [SerializeField] private float maxHorizontalSpeed = 18f;
    [SerializeField] private float horizontalInfluence = 1.0f;
    [SerializeField] private float turnDistance = 2f; // Prevent instantaneous turning.

    [Header("Phase Tuning")]
    [Range(0.1f, 0.9f)]
    [SerializeField] private float phase2HealthRatio = 0.5f;
    [SerializeField] private float phase2JumpForceMultiplier = 1.15f;
    [SerializeField] private float phase2JumpFrequencyMultiplier = 0.75f; // lower = more frequent

    [Header("Slam Attack")]
    [SerializeField] private float slamMinFallSpeed = -5f;
    [SerializeField] private float slamDamage = 15f;
    [SerializeField] private float slamRadius = 2.5f;
    [SerializeField] private LayerMask slamHitMask;

    [Header("Victory / Reward")]
    [SerializeField] private bool useMetroidvaniaReward = true;
    [SerializeField] private string rewardAbilityId = "privilege_escalation";
    [SerializeField] private float victoryDelaySeconds = 1.0f;
    [SerializeField] private string victorySceneName = "Level 1";
    [SerializeField] private bool alsoLoadLegacyVictoryScreen = false;

    // Access relevant components.
    private HealthManager hm;
    private GameObject playerObject;
    private SpriteRenderer spriteRenderer;
    private Color original;

    // Identified in Unity Inspector.
    [SerializeField] private ParticleSystem whiteLaunchParticles;
    [SerializeField] private ParticleSystem yellowLaunchParticles;
    [SerializeField] private ParticleSystem crashLaunchParticles;

    // Particles.
    ParticleSystem.EmissionModule emissions; // Used to toggle particle emitters.

    // Boss health bar variables.
    public Image healthBar;
    private float maxHealth;

    private bool facingLeft = true;
    private float timeSinceLastJump = 1f;
    private bool didSlamThisFall;
    private bool isDefeated;

    public event Action Defeated;

    protected override void Awake()
    {
        base.Awake();
    }

    // Start() is called before the first frame update.
    protected override void Start()
    {
        // Access components once to save processing power.
        if (bomb != null)
        {
            rb = bomb.GetComponent<Rigidbody2D>();
        }
        else
        {
            rb = GetComponent<Rigidbody2D>();
        }

        spriteRenderer = transform.parent != null ? transform.parent.GetComponent<SpriteRenderer>() : GetComponent<SpriteRenderer>();
        original = spriteRenderer.color;
        playerObject = GameObject.Find("Player");
        hm = playerObject.GetComponent<HealthManager>();
        maxHealth = health;
    }

    protected override void Update()
    {
        if (isDefeated) return;

        // Use Enemy base recoil handling (but not base death behavior, which is now overrideable via Die()).
        base.Update();

        gravityModifier(); // Launch at regular gravity, crash down after apex of launch.
        changeDirection(); // Whether player is on left or right of boss.
    }

    // FixedUpdate() can run once, zero, or several times per frame, depending on
    // how many physics frames per second are set in the time settings, and how
    // fast/slow the framerate is.
    private void FixedUpdate()
    {
        if (isDefeated) return;

        launchOffGround(); // Main movement function.
        timeSinceLastJump += Time.deltaTime;
        toggleGroundEmissions();
    }

    // Handles boss being attacked.
    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        if (_damageDone <= 0) { return; }

        health -= _damageDone;
        if (!isRecoiling)
        {
            rb.AddForce(-_hitForce * recoilFactor * _hitDirection);
        }

        timeSinceLastJump = 100; // Force the boss to jump.

        StartCoroutine(hitFlash(0.25f, original));

        //update boss health bar
        if (healthBar != null) healthBar.fillAmount = health / maxHealth;
    }

    // Handles boss movement.
    private void launchOffGround()
    {
        float jumpFrequency = GetJumpFrequency();
        float jumpForce = GetJumpForce();

        if (timeSinceLastJump > jumpFrequency)
        {
            launchParticles();

            Vector3 playerPosition = playerObject.transform.position;
            Vector3 myPosition = transform.position;
            Vector2 direction = (playerPosition - myPosition);
            
            float targetVx = Mathf.Clamp(direction.x * horizontalInfluence, -maxHorizontalSpeed, maxHorizontalSpeed);
            rb.velocity = new Vector2(targetVx, jumpForce);
            timeSinceLastJump = 0;
            didSlamThisFall = false;
        }
    }

    private float GetJumpForce()
    {
        if (maxHealth <= 0) return baseJumpForce;
        bool phase2 = health / maxHealth <= phase2HealthRatio;
        return phase2 ? baseJumpForce * phase2JumpForceMultiplier : baseJumpForce;
    }

    private float GetJumpFrequency()
    {
        if (maxHealth <= 0) return baseJumpFrequency;
        bool phase2 = health / maxHealth <= phase2HealthRatio;
        return phase2 ? baseJumpFrequency * phase2JumpFrequencyMultiplier : baseJumpFrequency;
    }

    private void launchParticles()
    {
        // Play explosive particles at launch.
        emissions = whiteLaunchParticles.emission;
        emissions.enabled = true;

        emissions = yellowLaunchParticles.emission;
        emissions.enabled = true;

        // Stop the crashing explosive particles before launching.
        emissions = crashLaunchParticles.emission;
        emissions.enabled = false;
    }

    // Handles boss particle emissions on the ground.
    private void toggleGroundEmissions()
    {
        // If boss is on the ground.
        if (rb.velocity.y < 0 && rb.position.y < 1.25)
        {
            emissions = whiteLaunchParticles.emission;
            emissions.enabled = false;

            emissions = yellowLaunchParticles.emission;
            emissions.enabled = false;

            emissions = crashLaunchParticles.emission;
            emissions.enabled = true;
            crashLaunchParticles.Play();
        }
    }

    // If the boss is at the apex of its jump (plus slight delay), increase gravity to simulate ground slam.
    private void gravityModifier()
    {
        if (rb.velocity.y < slamMinFallSpeed)
        {
            rb.gravityScale = 100;

            // Trigger slam damage once per fall when moving fast downward.
            if (!didSlamThisFall)
            {
                TrySlamDamage();
                didSlamThisFall = true;
            }
        }
        else
        {
            rb.gravityScale = 5;
        }
    }

    private void TrySlamDamage()
    {
        if (slamDamage <= 0) return;
        if (hm == null || playerObject == null) return;

        // If no mask is set, fall back to distance check to the player.
        if (slamHitMask.value == 0)
        {
            float dist = Vector2.Distance(transform.position, playerObject.transform.position);
            if (dist <= slamRadius) hm.TakeDamage(slamDamage);
            return;
        }

        Collider2D hit = Physics2D.OverlapCircle(transform.position, slamRadius, slamHitMask);
        if (hit != null && hit.CompareTag("Player")) hm.TakeDamage(slamDamage);
    }

    // Change the direction of the boss.
    private void changeDirection()
    {
        // If player is right of boss and boss is facing left, then look right OR
        // if player is left of boss and boss is facing right, then look left.
        if (((playerObject.transform.position.x > transform.position.x + turnDistance) && facingLeft) ||
            ((playerObject.transform.position.x < transform.position.x - turnDistance) && !facingLeft))
        {
            facingLeft = !facingLeft;
            Vector2 newDirection = rb.transform.localScale;
            newDirection.x *= -1;
            rb.transform.localScale = newDirection;

            timeSinceLastJump = 0; //reset jump timer 
        }
    }

    // When the boss is hit, flash the bomb red to show that it got hit.
    IEnumerator hitFlash(float timeLength, Color originalColor)
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(timeLength);
        spriteRenderer.color = originalColor;
    }

    protected override void Die()
    {
        if (isDefeated) return;
        isDefeated = true;

        // Stop movement/interaction.
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        if (rb != null) rb.velocity = Vector2.zero;

        // Reward unlock for vertical slice.
        if (useMetroidvaniaReward)
        {
            AbilitySystem system = GameRoot.Instance != null ? GameRoot.Instance.GetComponent<AbilitySystem>() : FindFirstObjectByType<AbilitySystem>();
            if (system != null && !string.IsNullOrWhiteSpace(rewardAbilityId))
            {
                system.Unlock(rewardAbilityId);
            }
        }

        // End the timer if present (non-fatal if missing).
        if (Timer.instance != null) Timer.instance.EndTimer();

        Defeated?.Invoke();
        StartCoroutine(VictoryRoutine());
    }

    private IEnumerator VictoryRoutine()
    {
        yield return new WaitForSeconds(victoryDelaySeconds);

        if (alsoLoadLegacyVictoryScreen)
        {
            SceneManager.LoadScene("Victory Screen");
            yield break;
        }

        if (!string.IsNullOrWhiteSpace(victorySceneName))
        {
            SceneManager.LoadScene(victorySceneName);
        }
    }
}