using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CodeRunner/World/WorldGraph", fileName = "WorldGraph")]
public class WorldGraph : ScriptableObject
{
    public List<RoomNode> rooms = new List<RoomNode>();

    public bool TryGetRoom(string roomId, out RoomNode node)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].roomId == roomId)
            {
                node = rooms[i];
                return true;
            }
        }

        node = default;
        return false;
    }
}

[Serializable]
public struct RoomNode
{
    public string roomId;
    public string sceneName;
    public List<RoomExit> exits;
}

[Serializable]
public struct RoomExit
{
    public string toRoomId;
    public string requiredAbilityId; // empty = no gate
    public string requiredKeyfileId; // empty = no gate
}

