using UnityEngine;

[CreateAssetMenu(menuName = "CodeRunner/Core/WorldConfig", fileName = "WorldConfig")]
public class WorldConfig : ScriptableObject
{
    [Header("World")]
    public WorldGraph worldGraph;

    [Header("Start")]
    [Tooltip("Room id to load when starting a new game (requires WorldGraph).")]
    public string startingRoomId;

    [Tooltip("Fallback scene name to load additively if startingRoomId/worldGraph isn't set.")]
    public string startingSceneName;

    public const string ResourcesPath = "CodeRunner/WorldConfig";
}

