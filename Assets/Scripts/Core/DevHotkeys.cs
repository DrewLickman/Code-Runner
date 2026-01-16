using UnityEngine;

[DefaultExecutionOrder(-500)]
public class DevHotkeys : MonoBehaviour
{
    [Header("Hotkeys")]
    public KeyCode unlockAllAbilitiesKey = KeyCode.F2;
    public KeyCode loadStartRoomKey = KeyCode.F3;
    public KeyCode saveKey = KeyCode.F5;
    public KeyCode loadKey = KeyCode.F9;
    public KeyCode resetProgressKey = KeyCode.F10;

    private PlayerProgress progress;
    private AbilitySystem abilitySystem;
    private SaveSystem saveSystem;
    private RoomLoader roomLoader;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        progress = GameRoot.Instance != null ? GameRoot.Instance.GetComponent<PlayerProgress>() : FindFirstObjectByType<PlayerProgress>();
        abilitySystem = GameRoot.Instance != null ? GameRoot.Instance.GetComponent<AbilitySystem>() : FindFirstObjectByType<AbilitySystem>();
        saveSystem = GameRoot.Instance != null ? GameRoot.Instance.GetComponent<SaveSystem>() : FindFirstObjectByType<SaveSystem>();
        roomLoader = GameRoot.Instance != null ? GameRoot.Instance.GetComponent<RoomLoader>() : FindFirstObjectByType<RoomLoader>();
    }

    private void Update()
    {
        if (abilitySystem != null && Input.GetKeyDown(unlockAllAbilitiesKey))
        {
            for (int i = 0; i < abilitySystem.allAbilities.Count; i++)
            {
                AbilityDefinition def = abilitySystem.allAbilities[i];
                if (def != null) abilitySystem.Unlock(def);
            }
        }

        if (saveSystem != null && Input.GetKeyDown(saveKey))
        {
            saveSystem.Save();
        }

        if (saveSystem != null && Input.GetKeyDown(loadKey))
        {
            saveSystem.Load();
        }

        if (Input.GetKeyDown(loadStartRoomKey))
        {
            WorldConfig config = Resources.Load<WorldConfig>(WorldConfig.ResourcesPath);
            if (config != null && roomLoader != null)
            {
                if (roomLoader.worldGraph == null) roomLoader.worldGraph = config.worldGraph;

                if (roomLoader.worldGraph != null && !string.IsNullOrWhiteSpace(config.startingRoomId))
                    roomLoader.LoadRoomById(config.startingRoomId);
                else if (!string.IsNullOrWhiteSpace(config.startingSceneName))
                    roomLoader.LoadRoomSceneName(config.startingSceneName, config.startingRoomId);
            }
        }

        if (Input.GetKeyDown(resetProgressKey) && progress != null)
        {
            progress.LoadFrom(new PlayerProgressData());
            saveSystem?.Save();
        }
    }
}

