using System.Text;
using UnityEngine;

public class DebugOverlay : MonoBehaviour
{
    [Header("Toggle")]
    public KeyCode toggleKey = KeyCode.F1;
    public bool visible;

    private PlayerProgress progress;
    private RoomLoader loader;
    private AbilitySystem abilities;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        progress = GameRoot.Instance != null ? GameRoot.Instance.GetComponent<PlayerProgress>() : FindFirstObjectByType<PlayerProgress>();
        loader = GameRoot.Instance != null ? GameRoot.Instance.GetComponent<RoomLoader>() : FindFirstObjectByType<RoomLoader>();
        abilities = GameRoot.Instance != null ? GameRoot.Instance.GetComponent<AbilitySystem>() : FindFirstObjectByType<AbilitySystem>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            visible = !visible;
    }

    private void OnGUI()
    {
        if (!visible) return;

        StringBuilder sb = new StringBuilder(256);
        sb.AppendLine("CodeRunner Debug");
        sb.AppendLine($"fps: {(1f / Mathf.Max(0.0001f, Time.unscaledDeltaTime)):0}");
        if (progress != null)
        {
            sb.AppendLine($"room: {progress.Data.currentRoomId}");
            sb.AppendLine($"unlockedAbilities: {progress.Data.unlockedAbilities.Count}");
        }
        if (loader != null)
        {
            sb.AppendLine($"roomLoaded: {loader.IsRoomLoaded}");
        }
        if (abilities != null)
        {
            sb.AppendLine($"abilityDefs: {abilities.allAbilities.Count}");
        }

        GUI.Box(new Rect(10, 10, 360, 120), sb.ToString());
    }
}

