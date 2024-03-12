using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Get access to the components of the current object (player) so we can modify them in code
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;
    private BoxCollider2D coll;     
    private float dirX = 0f;        // Variable to hold direction on the x-axis (initialized to zero).

    // "[SerializeFeild]" allows these variables to be edited in Unity.
    [SerializeField] private float moveSpeed = 7f;          
    [SerializeField] private float jumpForce = 14f;         
    [SerializeField] private LayerMask jumpableGround;      // Variable to check against IsGrounded() method.

    // Enum of movement state animations for our player to cycle through.
    // Each variable equals      0     1        2        3        mathematically.
    private enum MovementState { idle, running, jumping, falling }

    // Start is called before the first frame update.
    private void Start()
    {
        // Store components once to save memory and CPU resources.
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame.
    private void Update()
    {
        // Get direction on x-axis from Input Manager in Unity and store in dirX.
        // "Raw" in "GetAxisRaw" makes the player stop instantly when letting go of a directional key.
        dirX = Input.GetAxisRaw("Horizontal");

        // Use dirX to create velocity on the x-axis (joy-stick compatible).
        rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);

        // If space is pressed and IsGround() method returns true, then player jumps.
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            // Play the jump sound effect.
            //jumpSoundEffect.Play();

            // Velocity for each axis: x = velocity from previous movement and y = 14.
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        // Call UpdateAnimationState() method.
        UpdateAnimationState();
    }

    // Method to update animation states.
    private void UpdateAnimationState()
    {
        // Variable to hold the movement state.
        MovementState state;

        // If moving right (positive x-axis) set running animation to true.
        if (dirX > 0f)
        {
            state = MovementState.running;  // running animation = true
            sprite.flipX = false;   // flip animation to face right
        }
        // If moving left (negative x-axis) set running animation to true and flip animation on the x-axis.
        else if (dirX < 0f)
        {
            state = MovementState.running;  // running animation = true
            sprite.flipX = true;    // flip animatino to face left
        }
        // If not moving set running animation to false.
        else
        {
            state = MovementState.idle; // running animation = false
        }

        // We use +/-0.1f because our y-axis velocity is never perfectly zero.
        // If moving up (positive y-axis) set jumping animation to true.
        if (rb.velocity.y > 0.1f)
        {
            state = MovementState.jumping;  // jumping animation = true.
        }
        // If moving down (negative y-axis) set falling animation to true.
        else if (rb.velocity.y < -0.1f)
        {
            state = MovementState.falling;  // falling animation = true.
        }

        // Cast enum state into int state
        anim.SetInteger("state", (int)state);
    }

    private bool IsGrounded()
    {
        // Create a box around the player model that is slightly lower than player's hitbox.
        // If box overlaps jumpableGround return true, then player can jump again.
        // If box does not overlap jumpableGround return false, then player cannot jump again.
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, 0.1f, jumpableGround);
    }
}
