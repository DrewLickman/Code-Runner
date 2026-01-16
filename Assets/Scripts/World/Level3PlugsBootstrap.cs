using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Temporary bootstrapper to populate Level 3 with swinging plugs + a PacketHook pickup
/// without requiring manual scene editing. Safe to delete later once Level 3 is authored.
/// </summary>
public class Level3PlugsBootstrap : MonoBehaviour
{
    private const string Level3SceneName = "Level 3";

    [Header("Spawn Positions")]
    public Vector3 plug1Pos = new Vector3(125f, 75f, 0f);
    public Vector3 plug2Pos = new Vector3(132f, 75f, 0f);
    public Vector3 packetHookPickupPos = new Vector3(129f, 68f, 0f);
    // IMPORTANT: keep this far from the player's likely spawn area to avoid accidental triggers.
    public Vector3 backDoorPos = new Vector3(120f, 60f, 0f);

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != Level3SceneName) return;

        // Avoid duplicating if we re-enter the scene.
        if (GameObject.Find("Level3_PlugsBootstrap_Root") != null) return;

        GameObject root = new GameObject("Level3_PlugsBootstrap_Root");

        CreatePlug(root.transform, "HangingPlug (1)", plug1Pos);
        CreatePlug(root.transform, "HangingPlug (2)", plug2Pos);
        CreatePacketHookPickup(root.transform, packetHookPickupPos);
        CreateDoor(root.transform, "Level Door Back", backDoorPos, "Level 2");
    }

    private static void CreatePlug(Transform parent, string name, Vector3 position)
    {
        GameObject go = new GameObject(name);
        go.transform.position = position;
        go.transform.SetParent(parent, true);
        go.AddComponent<HangingPlug>();
    }

    private static void CreatePacketHookPickup(Transform parent, Vector3 position)
    {
        GameObject go = new GameObject("AbilityPickup_PacketHook (L3)");
        go.transform.position = position;
        go.transform.SetParent(parent, true);

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.6f;

        AbilityPickup pickup = go.AddComponent<AbilityPickup>();
        pickup.ability = Resources.Load<AbilityDefinition>("CodeRunner/Abilities/Ability_PacketHook");
        pickup.destroyOnPickup = true;
    }

    private static void CreateDoor(Transform parent, string name, Vector3 position, string nextScene)
    {
        GameObject go = new GameObject(name);
        go.transform.position = position;
        go.transform.SetParent(parent, true);

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1.5f, 1.5f);

        FinishLevel door = go.AddComponent<FinishLevel>();

        // FinishLevel.nextScene is private; set via serialized field name.
        // This is a runtime bootstrap, so we use Unity's SerializedObject pattern.
        // For runtime, reflection is simplest and safe here.
        var f = typeof(FinishLevel).GetField("nextScene", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (f != null) f.SetValue(door, nextScene);
    }
}

