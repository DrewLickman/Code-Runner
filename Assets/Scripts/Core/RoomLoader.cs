using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomLoader : MonoBehaviour
{
    [Tooltip("Optional: if set, rooms are loaded/unloaded using this graph.")]
    public WorldGraph worldGraph;

    private PlayerProgress progress;
    private string loadedRoomSceneName;

    private void Awake()
    {
        progress = GetComponent<PlayerProgress>();
    }

    public bool IsRoomLoaded => !string.IsNullOrWhiteSpace(loadedRoomSceneName);

    public void LoadRoomById(string roomId)
    {
        if (worldGraph == null)
        {
            Debug.LogWarning("RoomLoader has no WorldGraph assigned.");
            return;
        }

        if (!worldGraph.TryGetRoom(roomId, out RoomNode node))
        {
            Debug.LogWarning($"Room id not found in graph: {roomId}");
            return;
        }

        StartCoroutine(LoadRoomScene(node.sceneName, roomId));
    }

    public void LoadRoomSceneName(string sceneName, string roomId = null)
    {
        StartCoroutine(LoadRoomScene(sceneName, roomId));
    }

    private IEnumerator LoadRoomScene(string sceneName, string roomId)
    {
        if (string.IsNullOrWhiteSpace(sceneName)) yield break;

        // Unload previous room (but keep base scene(s) intact).
        if (!string.IsNullOrWhiteSpace(loadedRoomSceneName))
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(loadedRoomSceneName);
            if (unloadOp != null)
                yield return unloadOp;
        }

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (loadOp != null)
            yield return loadOp;

        loadedRoomSceneName = sceneName;

        if (progress != null && !string.IsNullOrWhiteSpace(roomId))
        {
            progress.Data.currentRoomId = roomId;
            progress.DiscoverRoom(roomId);
        }
    }
}

