using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[AddComponentMenu("Player/Player Movement Config")]
public class PlayerMovementConfig : MonoBehaviour
{
    [Header("Ground / Wall Checks")]
    [Tooltip("Transform used for ground overlap checks (feet position).")]
    public Transform groundCheck;
    [Tooltip("Layers considered solid ground for jumping and landing.")]
    public LayerMask groundLayer;
    [Tooltip("Transform used for wall overlap checks (side position).")]
    public Transform wallCheck;
    [Tooltip("Layers considered walls for wall sliding/jumping.")]
    public LayerMask wallLayer;

    [Header("Movement")]
    [Min(0f)]
    public float moveSpeed = 10f;
    [Tooltip("How quickly horizontal speed lerps to the target on ground (higher = snappier).")]
    [Min(0f)]
    public float groundHorizontalLerp = 22f;
    [Tooltip("How quickly horizontal speed lerps to the target in air (lower = preserves momentum longer).")]
    [Min(0f)]
    public float airHorizontalLerp = 0f;
    [Min(0f)]
    public float jumpingPower = 21f;
    [Min(0f)]
    public float wallSlidingSpeed = 3f;
    [Min(0f)]
    public float wallJumpingTime = 0.2f;
    [Min(0f)]
    public float wallJumpingDuration = 0.4f;
    public Vector2 wallJumpingPower = new Vector2(10f, 20f);

    [Header("Dash")]
    [Min(0f)]
    public float dashingPower = 24f;
    [Min(0f)]
    public float dashingTime = 0.2f;
    [Min(0f)]
    public float dashingCooldown = 0.75f;
    [Tooltip("Optional UI image to show dash cooldown as a filled radial/bar.")]
    public Image dashIcon;

    [Header("Audio Inputs")]
    public AudioSource jumpSound;
    public AudioSource doubleJumpSound;
    public AudioSource wallJumpSound;
    public AudioSource dashSound;
    public AudioSource attackSound;
    public AudioSource stepSound;

    [Header("Player Attack Settings")]
    [Min(0f)]
    public float damage = 1f;
    public Transform SideAttackTransform, UpAttackTransform, DownAttackTransform;
    public Vector2 SideAttackArea, UpAttackArea, DownAttackArea;
    public LayerMask attackableLayer;
    public GameObject slashEffect;

    [Header("Attack Recoil Settings")]
    [Min(0)]
    public int recoilXSteps = 3;
    [Min(0)]
    public int recoilYSteps = 3;
    [Min(0f)]
    public float recoilXSpeed = 25f;
    [Min(0f)]
    public float recoilYSpeed = 25f;

    [Header("Effects")]
    [Min(0f)]
    public float hitFlashSpeed = 1f;
    [Range(0f, 1f)]
    public float slowRatio = 0.45f;
}

