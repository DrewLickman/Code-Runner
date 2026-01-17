using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    // Access components for player object.
    private Animator anim;
    private Cheats cheats;
    private Rigidbody2D rb;
    private PlayerMovement pm;
    private PlayerStatesList pState;

    // Health and health bar variables.
    public Image healthBar;
    public float healthAmount = baseHealth;
    private bool displayedDead = false;
    private static float baseHealth = 100f;

    // "[SerializeFeild]" allows these variables to be edited in Unity.
    private Vector3 respawnCoords;
    [SerializeField] private float deathFloorHeight = -80;
    
    //To play sounds upon getting wounded or dying
    [SerializeField] private AudioSource deathSound;
    [SerializeField] private AudioSource hitSound;

    // Post processing variables.
    PostProcessVolume ppv;
    ColorGrading cg;

    // Start() is called before the first frame update.
    void Start()
    {
        // Get the default coordinates set in Unity as the respawn coordinates.
        respawnCoords = transform.position;

        // Access components once to save processing power.
        anim    = GetComponent<Animator>();
        rb      = GetComponent<Rigidbody2D>();
        pm      = GetComponent<PlayerMovement>();
        pState  = GetComponent<PlayerStatesList>();
        ppv     = GameObject.Find("Game Camera/Post Processing Volume").GetComponent<PostProcessVolume>();
    }

    // Update() is called once per frame.
    void Update()
    {
        // If the entity runs out of health.
        if (healthAmount <= 0 && !displayedDead)
        {
            // Only trigger Die() and Respawn() once!
            displayedDead = true;

            // Play death animations and sound.
            Die();

            // Delay playing death animation.
            Invoke(nameof(Respawn), 2.5f);
        }        

        // If player drops below death floor height they die.
        if (transform.position.y <= deathFloorHeight)
        {
            // Make sure to zero the player's velocity and movement to prevent clipping into terrain
            // Only set velocity if Rigidbody2D exists and is not static
            if (rb != null && rb.bodyType != RigidbodyType2D.Static)
            {
                rb.velocity = new Vector2(0, 0);
            }

            TakeDamage(healthAmount);
        }

        // Update health bar.
        if (healthBar != null)
        {
            UpdateHealthbar();
        }
    }

    // Handles player taking damage.
    public void TakeDamage(float _damage)
    {
        hitSound.Play();
        // Decrement player health.
        healthAmount -= Mathf.RoundToInt(_damage);

        // Stop taking damage (invincibility-frames).
        StartCoroutine(StopTakingDamage());

        UpdatePostProcessing();
    }

    // Updates player health bar to current health.
    public void UpdateHealthbar()
    {
        healthBar.fillAmount = healthAmount / 100f;
    }

    // Handles UI post processing.
    private void UpdatePostProcessing()
    {
        // Color Grading.
        // More damage makes the screen turn more red & darker.
        if (ppv.profile.TryGetSettings(out cg))
        {
            // Channel Mixer.
            float redValue = 200f - healthAmount;
            cg.mixerRedOutRedIn.value = redValue;

            // Trackballs Gain.
            float gainValue = -1 + (healthAmount / 100);
            cg.gain.value.w = gainValue; //value.w is how dark the highlights get

        }
    }

    // Handles player invincibility-frames and TakeDamage animation.
    IEnumerator StopTakingDamage()
    {
        // Enable player invincibility-frames.
        pState.invincible = true;

        // Delay disabling invincibility-frames.
        yield return new WaitForSeconds(1f);
        pState.invincible = false;
    }

    // Handles death animation and audio.
    public void Die()
    {
        // Activate death animation.
        deathSound.Play(); 
        anim.SetTrigger("death");
        pState.alive = false;
        if (gameObject.name == "Player")
        {
            // Disable player movement.
            pm.ignoreUserInput = true;
        }
    }

    // Handles player respawn.
    public void Respawn()
    {
        // Teleport back to initial spawn coordinates.
        transform.position = respawnCoords;
        healthAmount = baseHealth;
        pm.ignoreUserInput = false;
        pState.alive = true;
        Time.timeScale = 1;
    }

    // Restarts the current scene.
    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1;
    }
}