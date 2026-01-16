using System.Collections.Generic;
using UnityEngine;

public class AbilitySystem : MonoBehaviour
{
    [Tooltip("Optional list of all abilities available in the project (for UI/debug).")]
    public List<AbilityDefinition> allAbilities = new List<AbilityDefinition>();

    private PlayerProgress progress;

    private void Awake()
    {
        progress = GetComponent<PlayerProgress>();

        // Auto-populate ability list from Resources if empty.
        if (allAbilities.Count == 0)
        {
            AbilityDefinition[] defs = Resources.LoadAll<AbilityDefinition>("CodeRunner/Abilities");
            if (defs != null && defs.Length > 0)
                allAbilities.AddRange(defs);
        }
    }

    public bool IsUnlocked(string abilityId)
    {
        return progress != null && progress.HasAbility(abilityId);
    }

    public void Unlock(string abilityId)
    {
        if (progress == null) return;
        progress.UnlockAbility(abilityId);
        GetComponent<SaveSystem>()?.Save();
    }

    public void Unlock(AbilityDefinition ability)
    {
        if (ability == null) return;
        Unlock(ability.id);
    }
}

