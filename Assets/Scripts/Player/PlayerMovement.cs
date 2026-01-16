using UnityEngine;

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
    private SpriteRenderer sprite;

    // Shut off user input at death/goal.
    [Header("Player Settings:")]
    [SerializeField] public bool ignoreUserInput = false;

    // Extracted components (called in a deterministic order from this script).
    private PlayerMovementConfig config;
    private PlayerInputReader input;
    private PlayerMotor motor;
    private PlayerAbilities abilities;
    private PlayerCombat combat;
    private PlayerStateMachine stateMachine;
    private PlayerSwingController swing;

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
        pState = GetComponent<PlayerStatesList>();
        sprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Extracted components.
        config = GetComponent<PlayerMovementConfig>();
        input = GetComponent<PlayerInputReader>();
        motor = GetComponent<PlayerMotor>();
        abilities = GetComponent<PlayerAbilities>();
        combat = GetComponent<PlayerCombat>();
        stateMachine = GetComponent<PlayerStateMachine>();
        swing = GetComponent<PlayerSwingController>();
        if (swing == null) swing = gameObject.AddComponent<PlayerSwingController>();

        if (motor != null && rb != null) motor.Init(config, anim, rb);
        if (abilities != null && rb != null) abilities.Init(config, motor, stateMachine, rb);
        if (combat != null && rb != null)
        {
            combat.Init(config, pState, motor, rb, sprite);
            combat.SetAbilities(abilities);
        }
    }

    // Draw three red rectangles for player melee attack area.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        PlayerMovementConfig cfg = config != null ? config : GetComponent<PlayerMovementConfig>();
        if (cfg == null) return;

        if (cfg.SideAttackTransform != null) Gizmos.DrawWireCube(cfg.SideAttackTransform.position, cfg.SideAttackArea);
        if (cfg.UpAttackTransform != null) Gizmos.DrawWireCube(cfg.UpAttackTransform.position, cfg.UpAttackArea);
        if (cfg.DownAttackTransform != null) Gizmos.DrawWireCube(cfg.DownAttackTransform.position, cfg.DownAttackArea);
    }

    // Update() is called once per frame.
    void Update()
    {
        // Cast enum state into int state.
        if (anim != null) anim.SetInteger("state", (int)state);

        // Make player static when ignoreUserInput is true.
        if (ignoreUserInput)
        {
            if (rb != null) rb.bodyType = RigidbodyType2D.Static;
            return;
        }
        else if (rb != null && rb.bodyType == RigidbodyType2D.Static)
        {
            // Restore dynamic body after re-enabling input.
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        // Prevent player from moving, jumping, and flipping while dashing.
        if (abilities != null && abilities.IsDashing) { return; }

        // Input first (so everything reads a consistent intent snapshot).
        input?.Tick(ignoreUserInput);
        PlayerInputIntent intent = input != null ? input.Current : default;

        // Keep speed/anim speed synced with slowed state.
        motor?.TickSlowEffect(pState);

        // Swing controller (hold Jump to attach/release).
        swing?.Tick(intent);

        // Releasing Jump to detach from a swing should NOT trigger jump-cut (which halves upward velocity).
        // We "consume" JumpUp for abilities whenever the swing controller says to ignore jump-cut.
        if (swing != null && swing.ShouldIgnoreJumpCut)
        {
            intent.JumpUp = false;
        }

        // Abilities (jump/dash/wall logic) match the old Update order, unless swinging.
        if (swing == null || !swing.IsSwinging)
            abilities?.Tick(intent, pState);

        // Combat/effects.
        combat?.Tick(intent);

        // Animation and facing.
        UpdateAnimationState(intent.Horizontal);

        // Flip player direction when not wall jumping or swinging.
        if ((abilities == null || !abilities.IsWallJumping) && (swing == null || !swing.IsSwinging))
        {
            motor?.Flip(intent.Horizontal, blockFlip: false);
        }
    }

    // FixedUpdate() can run once, zero, or several times per frame, depending on
    // how many physics frames per second are set in the time settings, and how
    // fast/slow the framerate is.
    private void FixedUpdate()
    {
        // Prevent player from moving, jumping, and flipping while dashing.
        if (abilities != null && abilities.IsDashing) { return; }

        PlayerInputIntent intent = input != null ? input.Current : default;

        // Get horizontal movement when not wall jumping.
        bool blockMovement = (abilities != null && abilities.IsWallJumping) || (swing != null && swing.ShouldBlockMotor);
        motor?.FixedTickHorizontal(intent.Horizontal, blockMovement);

        // Apply recoil to attacks and damage.
        combat?.FixedTick(intent.Vertical);
    }

    // Switch between player animations based on movement.
    private void UpdateAnimationState(float horizontal)
    {
        // Guard clause to prevent nesting.
        // If wall jumping, don't change animation state.
        if (stateMachine != null && stateMachine.IsWallJumping)
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
        if (rb != null && rb.velocity.y > 0.1f) { state = MovementState.jumping; }

        // If moving down (negative y-axis) set state to falling animation.
        else if (rb != null && rb.velocity.y < -0.1f) { state = MovementState.falling; }

        // If wall sliding set state to wallSliding animation.
        if (stateMachine != null && stateMachine.IsWallSliding) { state = MovementState.wallSliding; }

        // If dashing set state to dashing animation.
        if (stateMachine != null && stateMachine.IsDashing) { state = MovementState.dashing; }
    }

    // Kept for external callers (enemies use this).
    public void HitStopTime(float newTimeScale, int restoreSpeed, float delay)
    {
        combat?.HitStopTime(newTimeScale, restoreSpeed, delay);
    }
}