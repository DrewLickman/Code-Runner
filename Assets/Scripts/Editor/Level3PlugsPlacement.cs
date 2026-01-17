using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor script to add hanging plugs to Level 3 at strategic positions
/// for swinging traversal between the new platform geometry.
/// </summary>
public class Level3PlugsPlacement
{
    [MenuItem("Tools/Level 3/Add Hanging Plugs to Level 3", false, 3)]
    public static void AddHangingPlugs()
    {
        // Find or create the Level 3 scene
        UnityEngine.SceneManagement.Scene level3Scene = default;
        bool sceneFound = false;
        
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (scene.name == "Level 3")
            {
                level3Scene = scene;
                sceneFound = true;
                break;
            }
        }

        if (!sceneFound)
        {
            Debug.LogError("Level 3 scene is not loaded. Please open the Level 3 scene first.");
            return;
        }

        // Find or create root for plugs
        GameObject plugsRoot = GameObject.Find("Level3_HangingPlugs");
        if (plugsRoot == null)
        {
            plugsRoot = new GameObject("Level3_HangingPlugs");
            Undo.RegisterCreatedObjectUndo(plugsRoot, "Create Level 3 Plugs Root");
        }

        // Clear existing plugs if any
        while (plugsRoot.transform.childCount > 0)
        {
            Undo.DestroyObjectImmediate(plugsRoot.transform.GetChild(0).gameObject);
        }

        // Strategic plug positions based on the new geometry:
        // - Ground floor is at y=0 (x=0-20)
        // - Mid-level left at y=8 (x=5-12)
        // - High platform at y=15 (x=25-35)
        // - Mid-level right at y=10 (x=40-50)
        // - Upper platform at y=20 (x=60-80)
        // - Small platforms at y=5, y=12, y=18

        // Plug 1: Above gap between starting area and first small platform
        // Positioned to swing from ground (y=0) to small platform (y=5)
        CreateHangingPlug(plugsRoot.transform, new Vector3(22, 8, 0), "Plug_StartToSmall1");

        // Plug 2: Above gap between small platform and mid-level left
        // Positioned to swing from small platform (y=5) to mid-level (y=8)
        CreateHangingPlug(plugsRoot.transform, new Vector3(10, 12, 0), "Plug_Small1ToMidLeft");

        // Plug 3: Above gap between mid-level left and high platform
        // Positioned to swing from mid-level (y=8) to high platform (y=15)
        CreateHangingPlug(plugsRoot.transform, new Vector3(20, 18, 0), "Plug_MidLeftToHigh");

        // Plug 4: Above gap between high platform and mid-level right
        // Positioned to swing from high (y=15) to mid-right (y=10) - downward swing
        CreateHangingPlug(plugsRoot.transform, new Vector3(37, 20, 0), "Plug_HighToMidRight");

        // Plug 5: Above gap between mid-level right and upper platform
        // Positioned to swing from mid-right (y=10) to upper (y=20)
        CreateHangingPlug(plugsRoot.transform, new Vector3(55, 25, 0), "Plug_MidRightToUpper");

        // Plug 6: Above the final area - for reaching the exit
        CreateHangingPlug(plugsRoot.transform, new Vector3(75, 28, 0), "Plug_UpperToExit");

        // Additional plugs for alternative routes:
        // Plug 7: Between starting area and mid-level (alternative route)
        CreateHangingPlug(plugsRoot.transform, new Vector3(12, 10, 0), "Plug_StartToMidAlt");

        // Plug 8: Above the center area for vertical traversal
        CreateHangingPlug(plugsRoot.transform, new Vector3(30, 22, 0), "Plug_CenterVertical");

        // Mark scene as dirty
        EditorSceneManager.MarkSceneDirty(level3Scene);
        
        Debug.Log($"Added {plugsRoot.transform.childCount} hanging plugs to Level 3!");
    }

    private static void CreateHangingPlug(Transform parent, Vector3 position, string name)
    {
        GameObject plug = new GameObject(name);
        plug.transform.SetParent(parent);
        plug.transform.position = position;
        
        HangingPlug hangingPlug = plug.AddComponent<HangingPlug>();
        
        // Configure for good swinging feel
        hangingPlug.segmentCount = 12;
        hangingPlug.segmentLength = 0.5f;
        hangingPlug.maxBendAngle = 20f;
        hangingPlug.gripRadius = 0.3f;
        
        Undo.RegisterCreatedObjectUndo(plug, $"Create Hanging Plug: {name}");
    }
}
