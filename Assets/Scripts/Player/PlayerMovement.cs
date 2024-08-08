using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    // Create an Instance of PlayerMovement to reference in other scripts.
    public static PlayerMovement Instance { get; private set; }

    // Class of bools that handle player states like jumping, dashing, and direction.
    [HideInInspector] public PlayerStatesList pState;

    // Enum of movement state animations for our player to cycle through.
    // Each variable equals      0     1             2            3        4        5        6            mathematically.
    private enum MovementState { idle, runningRight, runningLeft, jumping, falling, dashing, wallSliding }
    MovementState state;

    // Access components for player object.
    private Animator anim;
    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private SpriteRenderer sprite;

    // "[SerializeFeild]" allows these variables to be edited in Unity.
    // Shut off user input at death/goal.
    [Header("Player Settings:")]
    [SerializeField] public bool ignoreUserInput = false;

    // Ground check transform and layer for player jumping.
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    // Wall check transform and layer for player wall-jumping.
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    [Space(5)]

    // Movement and jump variables.
    private bool isFacingRight = true;
    private float vertical;
    private float horizontal;
    private bool canDoubleJump = false;

    [Header("Player Movement Settings:")]
    private static float defaultMoveSpeed = 10f;
    [SerializeField] private float moveSpeed = defaultMoveSpeed;
    [SerializeField] private float jumpingPower = 21f;
    [SerializeField] private float gravity;

    // Wall sliding variables.
    private bool isWallSliding = false;
    [SerializeField] private float wallSlidingSpeed = 3f;

    // Wall jumping variables.
    private bool isWallJumping = false;
    private float wallJumpingCounter;
    private float wallJumpingDirection;
    private Vector2 wallJumpingPower = new Vector2(10f, 20f);
    [SerializeField] private float wallJumpingTime = 0.2f;
    [SerializeField] private float wallJumpingDuration = 0.4f;

    // Dashing variables.
    private bool canDash = true;
    private bool isDashing;
    private float timeSinceDash = 1;
    [SerializeField] private float dashingPower = 24f;
    [SerializeField] private float dashingTime = 0.2f;
    [SerializeField] private float dashingCooldown = 0.75f;
    [SerializeField] private Image dashIcon;
    [Space(5)]

    // Audio Variables
    [Header("Audio Inputs:")]
    [SerializeField] private AudioSource jumpSound;
    [SerializeField] private AudioSource doubleJumpSound;
    [SerializeField] private AudioSource wallJumpSound;
    [SerializeField] private AudioSource dashSound;
    [SerializeField] private AudioSource attackSound;
    [SerializeField] private AudioSource stepSound;
    [Space(5)]

    // Attack variables.
    [Header("Player Attack Settings:")]
    private bool playerClickedAttack = false;    // Handles player attack permissions and holds attack key from Unity Input Manager.
    private bool restoreTime;
    private float timeSinceAttack;
    private float restoreTimeSpeed;
    [SerializeField] float damage = 1f;

    // Attack transform, attack area, attackable layers, and attack animations.
    [SerializeField] Transform SideAttackTransform, UpAttackTransform, DownAttackTransform;
    [SerializeField] Vector2 SideAttackArea, UpAttackArea, DownAttackArea;
    [SerializeField] LayerMask attackableLayer;
    [SerializeField] GameObject slashEffect;
    [Space(5)]

    // Player attack recoil variables.
    [Header("Attack Recoil Settings:")]
    private int stepsXRecoiled;
    private int stepsYRecoiled;
    [SerializeField] int recoilXSteps = 3;
    [SerializeField] int recoilYSteps = 3;
    [SerializeField] float recoilXSpeed = 25;
    [SerializeField] float recoilYSpeed = 25;
    [Space(5)]

    // Player health variables.
    [Header("Player Health Settings:")]
    [SerializeField] float hitFlashSpeed;
    [Space(5)]

    // Effects variables.
    [Header("Effects:")]
    private float slowRatio = 0.45f;

    // Awake() is called when the script instance is being loaded.
    // Awake() is used to initialize any variables or game states before the game starts.
    private void Awake()
    {
        // Error checking for the PlayerMovement instance.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy game object if instance is inaccessible.
        }
        // If no PlayerMovement instance is accessible then destroy game object.
        else { Instance = this; }
    }

    // Start() is called before the first frame update.
    private void Start()
    {
        // Access components once to save processing power.
        pState  = GetComponent<PlayerStatesList>();
        sprite  = GetComponent<SpriteRenderer>();
        coll    = GetComponent<BoxCollider2D>();
        rb      = GetComponent<Rigidbody2D>();
        anim    = GetComponent<Animator>();
        gravity = rb.gravityScale;
    }

    // Draw three red rectangles for player melee attack area.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);
    }

    // Update() is called once per frame.
    void Update()
    {
        // Cast enum state into int state.
        anim.SetInteger("state", (int)state);

        // Make player static when ignoreUserInput is true.
        if (ignoreUserInput)
        {
            rb.bodyType = RigidbodyType2D.Static;
            return;
        }

        // Prevent player from moving, jumping, and flipping while dashing.
        if (isDashing) { return; }

        // Function calls.
        GetUserInput();
        ResetDoubleJump();
        TrySlow();
        TryJump();
        TryDash();
        TryAttack();
        RestoreTimeScale();
        FlashWhileInvincible();
        WallSlide();
        WallJump();
        ReduceJumpHeightOnRelease();
        UpdateAnimationState();
        UpdateUI();

        // Flip player direction when not wall jumping.
        if (!isWallJumping) { Flip(); }
    }

    // FixedUpdate() can run once, zero, or several times per frame, depending on
    // how many physics frames per second are set in the time settings, and how
    // fast/slow the framerate is.
    private void FixedUpdate()
    {
        // Prevent player from moving, jumping, and flipping while dashing.
        if (isDashing) { return; }

        // Get horizontal movement when not wall jumping.
        if (!isWallJumping && rb.bodyType != RigidbodyType2D.Static)
        {
            rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);
        }

        // Apply recoil to attacks and damage.
        TryRecoil();
    }

    //Helper functions:
    void GetUserInput()
    {
        // Set attack, vertical, and horizontal input via Unity Input Manager.
        playerClickedAttack = Input.GetButtonDown("Attack");
        vertical = Input.GetAxisRaw("Vertical");
        horizontal = Input.GetAxisRaw("Horizontal");
    }

    // Check if player is touching jumpable ground.
    private bool IsGrounded()
    {
        // Create invisible circle at player's feet to check for overlap with jumpable ground.
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    // Check if player is touching a wall.
    private bool IsWalled()
    {
        // Create invisible circle at player side to check for overlap with walls.
        return Physics2D.OverlapCircle(wallCheck.position, 0.1f, wallLayer);
    }

    // Restores time scale if it has been modified.
    public void RestoreTimeScale()
    {
        // Guard clause to prevent nesting.
        // If restoreTime isn't required, skip this function.
        if (!restoreTime)
        { return; }

        // If time scale < 1 then summation time scale.
        if (Time.timeScale < 1)
        {
            Time.timeScale += Time.deltaTime * restoreTimeSpeed;
        }
        // Stop restoring time scale when time scale = 1.
        else
        {
            Time.timeScale = 1;
            restoreTime = false;
        }
    }

    // Cause player sprite to flash when player is invincible.
    void FlashWhileInvincible()
    {
        if (sprite == null)
        {
            Debug.LogError("SpriteRenderer not assigned in PlayerMovement.");
            return; // Exit if sprite is null to prevent further errors.
        }

        if (pState == null)
        {
            Debug.LogError("PlayerStatesList not assigned in PlayerMovement.");
            return; // Exit if pState is null to prevent further errors.
        }

        // Change player sprite color if player is invincible and don't change if not.
        sprite.material.color = pState.invincible ?
            Color.Lerp(Color.white, Color.black, Mathf.PingPong(Time.time * hitFlashSpeed, 1.0f)) : Color.white;
    }

    //Movement functions:

    void ResetDoubleJump()
    {
        // Reset jump counter
        if (IsGrounded()) { canDoubleJump = true; }
    }

    void TrySlow()
    {
        // Apply slowed effect to player.
        if (pState.slowed)
        {
            moveSpeed = defaultMoveSpeed * slowRatio;
            anim.speed = slowRatio;
        }
        else
        {
            moveSpeed = defaultMoveSpeed;
            anim.speed = 1.0f;
        }
    }

    void TryJump()
    {
        // Guard clause to prevent nesting.
        // If the player isn't pressing jump, or if they are unable to double jump, skip trying to jump
        if (!Input.GetButtonDown("Jump") || !canDoubleJump)
        { return; }

        // Jump if on jumpable ground or the single double jump.
        
        if (!IsGrounded())
        { canDoubleJump = false; }

        // Increase vertical velocity. 
        rb.velocity = new Vector2(rb.velocity.x, jumpingPower);

        //Play jump sound if on ground, play double jump sound if not on ground
        if (IsGrounded())
        { jumpSound.Play(); }
        else if (!IsGrounded() && !isWallJumping)
        { doubleJumpSound.Play(); }
        
    }

    void TryDash()
    {
        timeSinceDash += Time.deltaTime;
        // Dash by hitting leftShift if canDash is true.
        if (Input.GetButtonDown("Dash") && canDash && !isWallSliding)
        {
            StartCoroutine(Dash());
            dashSound.Play();
            timeSinceDash = 0;
        }
    }

    // Handles player dashing permissions and execution.
    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        // Preserve original gravity value and set gravity to zero while dashing.
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        // Dashing physics.
        rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);

        // Dash for the as long as dashingTime.
        yield return new WaitForSeconds(dashingTime);

        // Restore gravity after dashing.
        rb.gravityScale = originalGravity;
        isDashing = false;

        // Player can dash again after dashingCooldown.
        yield return new WaitForSeconds(dashingCooldown);

        if (timeSinceDash >= dashingCooldown) { canDash = true; }
    }

    // Check if player can wall slide and do it if so.
    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && horizontal != 0f)
        {
            isWallSliding = true;

            // Clamp player to wall and set wall slide speed.
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else { isWallSliding = false; }
    }

    // Check if player can wall jump and do it if so.
    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;

            // Wall jumping direction is the direction opposite of where the player is facing.
            wallJumpingDirection = -transform.localScale.x;

            // Set wall jumping counter.
            wallJumpingCounter = wallJumpingTime;

            // Stop invoking the method StopWallJumping().
            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            // Decrement wall jumping counter.
            wallJumpingCounter -= Time.deltaTime;
        }

        // If the player jumps while wall sliding they will wall jump.
        if (Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            isWallJumping = true;
            wallJumpSound.Play();

            // Apply wall jump physics.
            rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);

            // Reset wall jumping counter.
            wallJumpingCounter = 0f;

            // Invoke the method StopWallJumping().
            Invoke(nameof(StopWallJumping), wallJumpingDuration);

            // Check if player is facing the correct direction to wall jump off the next wall.
            // If not, change the direction the player is facing.
            if (transform.localScale.x != wallJumpingDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = rb.transform.localScale;
                localScale.x *= -1f;
                rb.transform.localScale = localScale;
            }
        }
    }

    // Stop allowing player to wall jump.
    private void StopWallJumping() { isWallJumping = false; }

    // Flip the player when they move in that direction.
    private void Flip()
    {
        // If facing right and moving left OR If facing left and moving right THEN flip.
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector2 localScale = rb.transform.localScale;
            localScale.x *= -1f;
            rb.transform.localScale = localScale;
        }
    }

    void ReduceJumpHeightOnRelease()
    {
        // Letting go of jump will reduce the jump height
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }
    }

    //Combat functions:

    // Player attack handler.
    void TryAttack()
    {
        // Set the time since the last attack.
        timeSinceAttack = Time.deltaTime;

        // Guard clause to prevent nesting.
        // If the player didn't try to attack, skip this function.
        if (!playerClickedAttack) { return; }

        // Reset time since last attack.
        timeSinceAttack = 0; 
            
        // If player is on the ground then side attack and display slash effect.
        if (vertical == 0)
        {
            // Side attack.
            ChooseAttackDirection("Side");
        }
        // If player's vertical input > 0 then up attack and display slash effect.
        else if (vertical > 0)
        {
            // Up attack.
            ChooseAttackDirection("Up");
        }
        // If player is in the air and player's vertical input < 0 then down attack and display slash effect.
        else if (!IsGrounded() && vertical < 0)
        {
            // Down attack.
            ChooseAttackDirection("Down");
        }
        
    }

    // Allow the programmer to write a readable direction for the attack, all that's modifyable is in here. 
    void ChooseAttackDirection(string attackDirection)
    {
        if (attackDirection == "Side")
        { AttackDirection(SideAttackTransform, SideAttackArea, ref pState.recoilingX, recoilXSpeed, slashEffect, 0, attackSound); }

        else if (attackDirection == "Up")
        { AttackDirection(UpAttackTransform, UpAttackArea, ref pState.recoilingY, recoilYSpeed, slashEffect, 80, attackSound); }

        else if (attackDirection == "Down")
        { AttackDirection(DownAttackTransform, DownAttackArea, ref pState.recoilingY, recoilYSpeed, slashEffect, -90, attackSound); }
    }

    // Take inputs predefined in ChooseAttackDirection to keep consistent attack method. 
    void AttackDirection(Transform positionArea, Vector2 distance, ref bool recoilDirection, float recoilSpeed, GameObject visualEffect, int effectAngle, AudioSource soundEffect)
    {
        soundEffect.Play();
        Hit(positionArea, distance, ref recoilDirection, recoilSpeed);
        SlashEffectAtAngle(visualEffect, effectAngle, positionArea);
    }


    // Handles player hit and recoil on enemies.
    private void Hit(Transform _attackTransform, Vector2 _attackArea, ref bool _recoilDir, float _recoilStrength)
    {
        // Holds an array of colliders for objects that can be hit.
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayer);

        // Guard clause for better readability.
        // If there are no objects to hit.
        if (objectsToHit.Length < 1)
        { return; }

        _recoilDir = true; //add a recoil effect on the player
        canDoubleJump = true; //if player hit enemy, allow another air jump and dash
        canDash = true;
        timeSinceDash += 1;
        
        // Loop through objectsToHit array and deal damage accordingly.
        for (int i = 0; i < objectsToHit.Length; i++)
        {
            // Apply hit damage to enemy, decrement enemy health, and recoil player.
            // The ? is the same as if (objectsToHit[i] != null) {}
            objectsToHit[i]?.GetComponent<Enemy>().EnemyHit(damage, (transform.position - objectsToHit[i].transform.position).normalized, _recoilStrength);
        }
    }

    // Handles slash animations according to direction.
    void SlashEffectAtAngle(GameObject _slashEffect, int _effectAngle, Transform _attackTransform)
    {
        // Create slash effect.
        _slashEffect = Instantiate(_slashEffect, _attackTransform);
        
        // Prevent 0 degree angle from causing bug where side attack only displays to one side.
        if (_effectAngle == 0) { return; }

        // Handle slash effect positioning.
        _slashEffect.transform.eulerAngles = new Vector3(0, 0, _effectAngle);

        // Handle slash effect scale.
        _slashEffect.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
    }

    // Handles player recoil.
    void TryRecoil()
    {
        // If player is recoiling on the x-axis.
        if (pState.recoilingX)
        {
            // If facing right.
            if (isFacingRight)
            {
                // Apply left recoil to player hit.
                rb.velocity = new Vector2(-recoilXSpeed, 0);
            }
            // If facing left.
            else
            {
                // Apply right recoil to player hit.
                rb.velocity = new Vector2(recoilXSpeed, 0);
            }
        }

        // If player is recoiling on the y-axis.
        if (pState.recoilingY)
        {
            // Disable player gravity.
            rb.gravityScale = 0;

            // If player vertical input < 0.
            if (vertical < 0)
            {
                // Apply upward recoil to player hit.
                rb.velocity = new Vector2(rb.velocity.x, recoilYSpeed);
            }
            // If player vertical input > 0.
            else
            {
                // Apply downward recoil to player hit.
                rb.velocity = new Vector2(rb.velocity.x, -recoilYSpeed);
            }
        }
        // If player is not recoiling.
        else { rb.gravityScale = gravity; }

        // If player is recoiling on the x-axis AND player still has steps left to recoil.
        if (pState.recoilingX && stepsXRecoiled < recoilXSteps)
        {
            // Increment steps recoiled.
            stepsXRecoiled++;
        }
        // If player is out of steps to recoil then stop recoiling.
        else { StopRecoilX(); }

        // If player is recoiling on the y-axis AND player still has steps left to recoil.
        if (pState.recoilingY && stepsYRecoiled < recoilYSteps)
        {
            // Increment steps recoiled.
            stepsYRecoiled++;
        }
        // If player is out of steps to recoil then stop recoiling.
        else { StopRecoilY(); }

        // If player is touching jumpable ground then stop recoiling.
        if (IsGrounded()) { StopRecoilY(); }
    }

    // Stop player from recoiling on the x-axis.
    void StopRecoilX()
    {
        stepsXRecoiled = 0;
        pState.recoilingX = false;
    }

    // Stop player from recoiling on the y-axis.
    void StopRecoilY()
    {
        stepsYRecoiled = 0;
        pState.recoilingY = false;
    }

    // Initiates change in time scale based on when player takes damage.
    public void HitStopTime(float _newTimeScale, int _restoreSpeed, float _delay)
    {
        restoreTimeSpeed = _restoreSpeed;
        Time.timeScale = _newTimeScale;

        // Check if time scale has a delay.
        if (_delay > 0)
        {
            StopCoroutine(StartTimeAgain(_delay));
            StartCoroutine(StartTimeAgain(_delay));
        }
        // If time scale has delay then restore time scale.
        else
        {
            restoreTime = true;
        }
    }

    // Handles delay times.
    IEnumerator StartTimeAgain(float _delay)
    {
        restoreTime = true;
        yield return new WaitForSeconds(_delay);
    }

    // Switch between player animations based on movement.
    private void UpdateAnimationState()
    {
        // Guard clause to prevent nesting.
        // If wall jumping, don't change animation state.
        if (isWallJumping)
        { return; }

        // If not moving set state to idle animation.
        if (horizontal == 0f) { state = MovementState.idle; }

        // If moving right (positive x-axis) set state to runningRight animation.
        // *It just works with != instead of > so DO NOT change this*
        else if (horizontal != 0f) { state = MovementState.runningRight; }

        // If moving left (negative x-axis) set state to runningLeft animation.
        else if (horizontal < 0f) { state = MovementState.runningLeft; }

        // We use +/-0.1f because our y-axis velocity is rarely perfectly zero.
        // If moving up (positive y-axis) set state to jumping animation.
        if (rb.velocity.y > 0.1f) { state = MovementState.jumping; }

        // If moving down (negative y-axis) set state to falling animation.
        else if (rb.velocity.y < -0.1f) { state = MovementState.falling; }

        // If wall sliding set state to wallSliding animation.
        if (isWallSliding) { state = MovementState.wallSliding; }

        // If dashing set state to dashing animation.
        if (isDashing) { state = MovementState.dashing; }
        
    }
    private void UpdateUI()
    {
        // Refill dash icon fill amount. Make sure it is a value between 0 to 1, and not NaN
        dashIcon.fillAmount = timeSinceDash != 0 ? Math.Clamp(timeSinceDash/dashingCooldown, 0, 1) : 0;
    }
}