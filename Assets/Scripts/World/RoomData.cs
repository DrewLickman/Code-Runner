using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CodeRunner/World/RoomData", fileName = "Room_")]
public class RoomData : ScriptableObject
{
    [Header("Identity")]
    public string roomId;

    [Tooltip("Scene name for additive loading (must be in Build Settings when used).")]
    public string sceneName;

    [Header("Exits")]
    public List<RoomExitData> exits = new List<RoomExitData>();
}

[System.Serializable]
public class RoomExitData
{
    public string exitId;
    public string toRoomId;

    [Header("Gates")]
    public string requiredAbilityId;
    public string requiredKeyfileId;
}

