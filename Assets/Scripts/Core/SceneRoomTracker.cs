using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneRoomTracker : MonoBehaviour
{
    private PlayerProgress progress;
    private RoomLoader loader;

    private void Awake()
    {
        progress = GetComponent<PlayerProgress>();
        loader = GetComponent<RoomLoader>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (progress == null) return;
        if (loader == null || loader.worldGraph == null) return;

        // If this scene is a known room scene, update current room id + discovery.
        for (int i = 0; i < loader.worldGraph.rooms.Count; i++)
        {
            RoomNode node = loader.worldGraph.rooms[i];
            if (node.sceneName == scene.name)
            {
                progress.Data.currentRoomId = node.roomId;
                progress.DiscoverRoom(node.roomId);
                return;
            }
        }
    }
}

