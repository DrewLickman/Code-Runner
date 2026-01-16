using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishLevel : MonoBehaviour
{
    // Level completion variables.
    private bool levelCompleted = false;
    private float sceneSwapDelayS = 1f;
    private GameObject player;

    // "[SerializeFeild]" allows these variables to be edited in Unity.
    [SerializeField] private string nextScene;
    
    [Header("Metroidvania (optional)")]
    [Tooltip("If true, and a WorldGraph is available, the door will enforce ability/keyfile gates from the graph before changing scenes.")]
    [SerializeField] private bool useWorldGraphGating = true;

    private void Start()
    {
        player = GameObject.Find("Player");
    }

    // Detect if player touches finish line.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If player collides with object AND level has not been completed, return true.
        if (collision.gameObject == player && !levelCompleted)
        {
            // Ignore user input so player stays at the goal.
            player.GetComponent<PlayerMovement>().ignoreUserInput = true;

            // Set levelCompleted to true after reaching finish line.
            levelCompleted = true;

            // Call CompleteLevel() method with delay.
            Invoke(nameof(CompleteLevel), sceneSwapDelayS);
        }
    }

    // Method for completing a level.
    private void CompleteLevel()
    {
        if (string.IsNullOrWhiteSpace(nextScene))
        {
            Debug.LogWarning($"{name}: nextScene is empty.");
            levelCompleted = false;
            if (player != null) player.GetComponent<PlayerMovement>().ignoreUserInput = false;
            return;
        }

        // If the metroidvania systems are present, use them to validate gates (but keep SceneManager load to avoid duplicating players for now).
        if (useWorldGraphGating && TryCanTraverseToScene(nextScene, out string destRoomId))
        {
            // Pre-mark discovery so UI/debug can reflect immediately after load.
            PlayerProgress progress = GameRoot.Instance != null ? GameRoot.Instance.GetComponent<PlayerProgress>() : null;
            if (progress != null && !string.IsNullOrWhiteSpace(destRoomId))
            {
                progress.Data.currentRoomId = destRoomId;
                progress.DiscoverRoom(destRoomId);
                GameRoot.Instance.GetComponent<SaveSystem>()?.Save();
            }
        }
        else if (useWorldGraphGating && GameRoot.Instance != null && GameRoot.Instance.GetComponent<RoomLoader>()?.worldGraph != null)
        {
            // We have a graph, but this transition is not allowed.
            levelCompleted = false;
            if (player != null) player.GetComponent<PlayerMovement>().ignoreUserInput = false;
            return;
        }

        // Default behavior: Load next level.
        SceneManager.LoadScene(nextScene);
    }

    private bool TryCanTraverseToScene(string sceneName, out string destRoomId)
    {
        destRoomId = null;

        if (GameRoot.Instance == null) return false;

        PlayerProgress progress = GameRoot.Instance.GetComponent<PlayerProgress>();
        RoomLoader loader = GameRoot.Instance.GetComponent<RoomLoader>();
        if (progress == null || loader == null || loader.worldGraph == null) return false;

        // Find destination room by scene name.
        WorldGraph graph = loader.worldGraph;
        for (int i = 0; i < graph.rooms.Count; i++)
        {
            if (graph.rooms[i].sceneName == sceneName)
            {
                destRoomId = graph.rooms[i].roomId;
                break;
            }
        }

        if (string.IsNullOrWhiteSpace(destRoomId)) return false;

        // If we don't know current room, allow.
        if (string.IsNullOrWhiteSpace(progress.Data.currentRoomId)) return true;

        // Find current room + matching exit gate.
        if (!graph.TryGetRoom(progress.Data.currentRoomId, out RoomNode current)) return true;
        if (current.exits == null) return true;

        for (int i = 0; i < current.exits.Count; i++)
        {
            RoomExit exit = current.exits[i];
            if (exit.toRoomId == destRoomId)
            {
                return GateRules.CanTraverse(exit, progress);
            }
        }

        // No explicit exit rule = allow for now.
        return true;
    }
}