using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class GameRoot : MonoBehaviour
{
    public static GameRoot Instance { get; private set; }

    public GameState State { get; private set; } = GameState.Boot;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;

        GameObject go = new GameObject("GameRoot");
        DontDestroyOnLoad(go);
        go.AddComponent<GameRoot>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load config from Resources if present.
        WorldConfig config = Resources.Load<WorldConfig>(WorldConfig.ResourcesPath);

        // Attach core managers if they aren't already present.
        GetOrAdd<PlayerProgress>();
        GetOrAdd<SaveSystem>();
        RoomLoader loader = GetOrAdd<RoomLoader>();
        GetOrAdd<AbilitySystem>();
        GetOrAdd<DevHotkeys>();
        GetOrAdd<DebugOverlay>();
        GetOrAdd<SceneRoomTracker>();
        GetOrAdd<Level3PlugsBootstrap>();

        // Auto-wire the RoomLoader.
        if (config != null && loader != null && loader.worldGraph == null)
        {
            loader.worldGraph = config.worldGraph;
        }

        State = GameState.Playing;
    }

    private T GetOrAdd<T>() where T : Component
    {
        T existing = GetComponent<T>();
        return existing != null ? existing : gameObject.AddComponent<T>();
    }
}

