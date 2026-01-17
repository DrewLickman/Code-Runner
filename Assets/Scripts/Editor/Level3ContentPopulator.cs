using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor script to populate Level 3 with enemies, collectibles, and other gameplay elements.
/// </summary>
public class Level3ContentPopulator
{
    [MenuItem("Tools/Level 3/Add Enemies to Level 3", false, 4)]
    public static void AddEnemies()
    {
        UnityEngine.SceneManagement.Scene level3Scene = GetLevel3Scene();
        if (!level3Scene.IsValid()) return;

        // Find or create root for enemies
        GameObject enemiesRoot = GameObject.Find("Level3_Enemies");
        if (enemiesRoot == null)
        {
            enemiesRoot = new GameObject("Level3_Enemies");
            Undo.RegisterCreatedObjectUndo(enemiesRoot, "Create Level 3 Enemies Root");
        }

        // Clear existing enemies
        while (enemiesRoot.transform.childCount > 0)
        {
            Undo.DestroyObjectImmediate(enemiesRoot.transform.GetChild(0).gameObject);
        }

        // Load enemy prefabs
        GameObject crawlerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Obstacles/Crawler.prefab");
        GameObject chargerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Obstacles/Charger.prefab");
        GameObject scannerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Obstacles/ScannerDrone.prefab");

        // Strategic enemy placement based on platform positions:
        // Ground floor (y=0, x=0-20) - starting area, light enemies
        if (crawlerPrefab != null)
        {
            CreateEnemy(enemiesRoot.transform, crawlerPrefab, new Vector3(5, 0.5f, 0), "Crawler_Start1");
            CreateEnemy(enemiesRoot.transform, crawlerPrefab, new Vector3(12, 0.5f, 0), "Crawler_Start2");
        }

        // Mid-level left (y=8, x=5-12) - medium challenge
        if (chargerPrefab != null)
        {
            CreateEnemy(enemiesRoot.transform, chargerPrefab, new Vector3(8.5f, 8.5f, 0), "Charger_MidLeft");
        }

        // High platform (y=15, x=25-35) - challenging area
        if (scannerPrefab != null)
        {
            CreateEnemy(enemiesRoot.transform, scannerPrefab, new Vector3(30, 15.5f, 0), "Scanner_High");
        }
        if (chargerPrefab != null)
        {
            CreateEnemy(enemiesRoot.transform, chargerPrefab, new Vector3(28, 15.5f, 0), "Charger_High");
        }

        // Mid-level right (y=10, x=40-50) - mixed enemies
        if (crawlerPrefab != null)
        {
            CreateEnemy(enemiesRoot.transform, crawlerPrefab, new Vector3(42, 10.5f, 0), "Crawler_MidRight");
        }
        if (scannerPrefab != null)
        {
            CreateEnemy(enemiesRoot.transform, scannerPrefab, new Vector3(47, 10.5f, 0), "Scanner_MidRight");
        }

        // Upper platform (y=20, x=60-80) - final challenge before exit
        if (scannerPrefab != null)
        {
            CreateEnemy(enemiesRoot.transform, scannerPrefab, new Vector3(65, 20.5f, 0), "Scanner_Upper1");
            CreateEnemy(enemiesRoot.transform, scannerPrefab, new Vector3(75, 20.5f, 0), "Scanner_Upper2");
        }
        if (chargerPrefab != null)
        {
            CreateEnemy(enemiesRoot.transform, chargerPrefab, new Vector3(70, 20.5f, 0), "Charger_Upper");
        }

        EditorSceneManager.MarkSceneDirty(level3Scene);
        Debug.Log($"Added enemies to Level 3!");
    }

    [MenuItem("Tools/Level 3/Add Collectibles to Level 3", false, 5)]
    public static void AddCollectibles()
    {
        UnityEngine.SceneManagement.Scene level3Scene = GetLevel3Scene();
        if (!level3Scene.IsValid()) return;

        // Find or create root for collectibles
        GameObject collectiblesRoot = GameObject.Find("Level3_Collectibles");
        if (collectiblesRoot == null)
        {
            collectiblesRoot = new GameObject("Level3_Collectibles");
            Undo.RegisterCreatedObjectUndo(collectiblesRoot, "Create Level 3 Collectibles Root");
        }

        // Clear existing collectibles
        while (collectiblesRoot.transform.childCount > 0)
        {
            Undo.DestroyObjectImmediate(collectiblesRoot.transform.GetChild(0).gameObject);
        }

        // Load ability definition for PacketHook (if it exists)
        AbilityDefinition packetHook = Resources.Load<AbilityDefinition>("CodeRunner/Abilities/Ability_PacketHook");

        // Place collectibles at strategic locations:
        // 1. On mid-level left platform - reward for first swing
        if (packetHook != null)
        {
            CreateAbilityPickup(collectiblesRoot.transform, packetHook, new Vector3(8.5f, 9.5f, 0), "PacketHook_MidLeft");
        }

        // 2. On high platform - reward for advanced swinging
        CreateGenericCollectible(collectiblesRoot.transform, new Vector3(30, 16.5f, 0), "Collectible_High");

        // 3. On upper platform - final reward
        CreateGenericCollectible(collectiblesRoot.transform, new Vector3(70, 21.5f, 0), "Collectible_Upper");

        EditorSceneManager.MarkSceneDirty(level3Scene);
        Debug.Log($"Added collectibles to Level 3!");
    }

    private static void CreateEnemy(Transform parent, GameObject prefab, Vector3 position, string name)
    {
        GameObject enemy = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (enemy == null)
        {
            Debug.LogWarning($"Failed to instantiate enemy prefab: {prefab.name}");
            return;
        }
        
        enemy.name = name;
        enemy.transform.SetParent(parent);
        enemy.transform.position = position;
        
        Undo.RegisterCreatedObjectUndo(enemy, $"Create Enemy: {name}");
    }

    private static void CreateAbilityPickup(Transform parent, AbilityDefinition ability, Vector3 position, string name)
    {
        GameObject pickup = new GameObject(name);
        pickup.transform.SetParent(parent);
        pickup.transform.position = position;

        CircleCollider2D col = pickup.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.6f;

        AbilityPickup abilityPickup = pickup.AddComponent<AbilityPickup>();
        abilityPickup.ability = ability;
        abilityPickup.destroyOnPickup = true;

        Undo.RegisterCreatedObjectUndo(pickup, $"Create Ability Pickup: {name}");
    }

    private static void CreateGenericCollectible(Transform parent, Vector3 position, string name)
    {
        GameObject collectible = new GameObject(name);
        collectible.transform.SetParent(parent);
        collectible.transform.position = position;

        CircleCollider2D col = collectible.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        // Add a simple visual indicator
        SpriteRenderer sr = collectible.AddComponent<SpriteRenderer>();
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.yellow);
        tex.Apply();
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
        sr.sprite = sprite;
        sr.sortingLayerName = "Default";

        Undo.RegisterCreatedObjectUndo(collectible, $"Create Collectible: {name}");
    }

    private static UnityEngine.SceneManagement.Scene GetLevel3Scene()
    {
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (scene.name == "Level 3")
            {
                return scene;
            }
        }
        
        Debug.LogError("Level 3 scene is not loaded. Please open the Level 3 scene first.");
        return default;
    }
}
