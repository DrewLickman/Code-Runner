using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AbilityGate : MonoBehaviour
{
    [Header("Gate Requirements")]
    [Tooltip("Ability id required to pass (empty = no ability gate).")]
    public string requiredAbilityId;

    [Tooltip("Keyfile id required to pass (empty = no keyfile gate).")]
    public string requiredKeyfileId;

    [Header("Gate Behavior")]
    [Tooltip("Collider that blocks the player. If empty, uses the Collider2D on this GameObject.")]
    public Collider2D blockingCollider;

    [Tooltip("Optional: visuals to enable/disable when unlocked.")]
    public GameObject lockedVisual;
    public GameObject unlockedVisual;

    private PlayerProgress progress;

    private void Awake()
    {
        if (blockingCollider == null) blockingCollider = GetComponent<Collider2D>();
        progress = GameRoot.Instance != null ? GameRoot.Instance.GetComponent<PlayerProgress>() : FindFirstObjectByType<PlayerProgress>();
    }

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        bool unlocked = true;

        if (progress == null) unlocked = false;
        if (unlocked && !string.IsNullOrWhiteSpace(requiredAbilityId) && !progress.HasAbility(requiredAbilityId)) unlocked = false;
        if (unlocked && !string.IsNullOrWhiteSpace(requiredKeyfileId) && !progress.HasKeyfile(requiredKeyfileId)) unlocked = false;

        if (blockingCollider != null) blockingCollider.enabled = !unlocked;
        if (lockedVisual != null) lockedVisual.SetActive(!unlocked);
        if (unlockedVisual != null) unlockedVisual.SetActive(unlocked);
    }
}

