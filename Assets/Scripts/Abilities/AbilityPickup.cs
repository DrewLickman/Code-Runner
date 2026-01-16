using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AbilityPickup : MonoBehaviour
{
    [Tooltip("Ability to unlock when the player touches this pickup.")]
    public AbilityDefinition ability;

    [Tooltip("Optional: if set, used when ability is not assigned.")]
    public string abilityId;

    public bool destroyOnPickup = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        AbilitySystem system = GameRoot.Instance != null ? GameRoot.Instance.GetComponent<AbilitySystem>() : FindFirstObjectByType<AbilitySystem>();
        if (system == null) return;

        if (ability != null) system.Unlock(ability);
        else if (!string.IsNullOrWhiteSpace(abilityId)) system.Unlock(abilityId);

        if (destroyOnPickup) Destroy(gameObject);
    }
}

