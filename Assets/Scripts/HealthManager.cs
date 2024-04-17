using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    private GameObject player;
    private Animator anim;
    private Rigidbody2D rb;
    private PlayerMovement pm;
    private Cheats cheats;

    public Image healthBar;
    private static float baseHealth = 100f;
    public float healthAmount = baseHealth;

    private Vector3 respawnCoords;
    [SerializeField] private float deathFloorHeight;
    // Start is called before the first frame update
    void Start()
    {
        //get the default coordinates set in Unity as the respawn coordinates
        respawnCoords = transform.position;

        //can't access these due to HealthManager not being a child of the player
        player = GameObject.Find("Player");
        anim = player.GetComponent<Animator>();
        rb = player.GetComponent<Rigidbody2D>();
        pm = player.GetComponent<PlayerMovement>();
        cheats = player.GetComponent<Cheats>();
    }

    // Update is called once per frame
    void Update()
    {
        //if the player dies
        if (healthAmount <= 0)
        {
            Debug.Log("Player ran out of health!");
            Die();
            //delay to play death animation
            Invoke(nameof(Respawn), 1f);
            //Respawn();
            //RestartLevel();
        }

        if (cheats.debugMode == true)
        {
            //FOR TESTING PURPOSES
            if (Input.GetKeyDown(KeyCode.Return))
            { TakeDamage(20); }

            if (Input.GetKeyDown(KeyCode.Backspace))
            { Heal(10); }
            //////////////////////
        }


        if (transform.position.y <= deathFloorHeight)
        {
            Debug.Log($"{gameObject.name} fell out of the world!");

            // Make sure to zero the player's velocity and movement to prevent clipping into terrain
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.velocity = new Vector2(0, 0);

            Respawn();
        }

        UpdateHealthbar();
    }

    public void TakeDamage(float amount)
    {
        healthAmount -= amount;
    }

    public void Heal(float amount)
    {
        healthAmount += amount;
        healthAmount = Mathf.Clamp(healthAmount, 0, 100);
    }

    public void UpdateHealthbar()
    {
        healthBar.fillAmount = healthAmount / 100f;
    }

    public void Die()
    {
        // Activate death animation.
        anim.SetTrigger("death");

        // Disable player movement.
        //rb.bodyType = RigidbodyType2D.Static;
        pm.ignoreUserInput = true;
    }

    public void Respawn()
    {
        //instantly teleport back to initial coordinates
        transform.position = respawnCoords;
        healthAmount = baseHealth;
    }

    private void RestartLevel()
    {
        // Restart level.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
