using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerProgressData
{
    public string currentRoomId;
    public string lastSaveRoomId;
    public List<string> unlockedAbilities = new List<string>();
    public List<string> collectedKeyfiles = new List<string>();
    public List<string> discoveredRooms = new List<string>();
}

public class PlayerProgress : MonoBehaviour
{
    public PlayerProgressData Data { get; private set; } = new PlayerProgressData();

    private readonly HashSet<string> unlockedAbilitySet = new HashSet<string>();
    private readonly HashSet<string> discoveredRoomSet = new HashSet<string>();
    private readonly HashSet<string> keyfileSet = new HashSet<string>();

    public bool HasAbility(string abilityId) => !string.IsNullOrWhiteSpace(abilityId) && unlockedAbilitySet.Contains(abilityId);
    public bool HasKeyfile(string keyfileId) => !string.IsNullOrWhiteSpace(keyfileId) && keyfileSet.Contains(keyfileId);
    public bool HasDiscoveredRoom(string roomId) => !string.IsNullOrWhiteSpace(roomId) && discoveredRoomSet.Contains(roomId);

    public void LoadFrom(PlayerProgressData data)
    {
        Data = data ?? new PlayerProgressData();

        unlockedAbilitySet.Clear();
        discoveredRoomSet.Clear();
        keyfileSet.Clear();

        if (Data.unlockedAbilities != null)
        {
            foreach (string id in Data.unlockedAbilities)
                if (!string.IsNullOrWhiteSpace(id)) unlockedAbilitySet.Add(id);
        }

        if (Data.discoveredRooms != null)
        {
            foreach (string id in Data.discoveredRooms)
                if (!string.IsNullOrWhiteSpace(id)) discoveredRoomSet.Add(id);
        }

        if (Data.collectedKeyfiles != null)
        {
            foreach (string id in Data.collectedKeyfiles)
                if (!string.IsNullOrWhiteSpace(id)) keyfileSet.Add(id);
        }
    }

    public void UnlockAbility(string abilityId)
    {
        if (string.IsNullOrWhiteSpace(abilityId)) return;
        if (unlockedAbilitySet.Add(abilityId))
        {
            if (!Data.unlockedAbilities.Contains(abilityId))
                Data.unlockedAbilities.Add(abilityId);
        }
    }

    public void DiscoverRoom(string roomId)
    {
        if (string.IsNullOrWhiteSpace(roomId)) return;
        if (discoveredRoomSet.Add(roomId))
        {
            if (!Data.discoveredRooms.Contains(roomId))
                Data.discoveredRooms.Add(roomId);
        }
    }

    public void CollectKeyfile(string keyfileId)
    {
        if (string.IsNullOrWhiteSpace(keyfileId)) return;
        if (keyfileSet.Add(keyfileId))
        {
            if (!Data.collectedKeyfiles.Contains(keyfileId))
                Data.collectedKeyfiles.Add(keyfileId);
        }
    }
}

