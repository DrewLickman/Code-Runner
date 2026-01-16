using UnityEngine;

public static class GateRules
{
    public static bool CanTraverse(RoomExit exit, PlayerProgress progress)
    {
        if (progress == null) return false;

        if (!string.IsNullOrWhiteSpace(exit.requiredAbilityId) && !progress.HasAbility(exit.requiredAbilityId))
            return false;

        if (!string.IsNullOrWhiteSpace(exit.requiredKeyfileId) && !progress.HasKeyfile(exit.requiredKeyfileId))
            return false;

        return true;
    }
}

