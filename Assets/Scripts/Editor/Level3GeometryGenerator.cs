using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Editor script to generate new level geometry for Level 3.
/// Designed around swinging traversal with hanging plugs - vertical spaces,
/// platforms at different heights, and gaps that require swinging.
/// </summary>
public class Level3GeometryGenerator
{
    [MenuItem("Tools/Level 3/Generate Level 3 Geometry", false, 1)]
    public static void GenerateLevel3Geometry()
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

        // Find existing geometry objects to replace
        GameObject existingTerrain = GameObject.Find("Terrain");
        GameObject existingWalls = GameObject.Find("Walls");
        GameObject existingSpikes = GameObject.Find("Spikes");

        // Create root for new geometry
        GameObject geometryRoot = new GameObject("Level3_NewGeometry");
        Undo.RegisterCreatedObjectUndo(geometryRoot, "Create Level 3 Geometry Root");
        
        // Generate new terrain with platforms at different heights
        GameObject terrain = CreateTerrain(geometryRoot.transform);
        
        // Generate new walls with vertical spaces for swinging
        GameObject walls = CreateWalls(geometryRoot.transform);
        
        // Create platforms for swinging traversal
        CreateSwingingPlatforms(geometryRoot.transform);
        
        // Mark scene as dirty
        EditorSceneManager.MarkSceneDirty(level3Scene);

        Debug.Log("New Level 3 geometry generated! Disable or delete old Terrain/Walls/Spikes objects.");

        // Optionally disable old geometry
        if (existingTerrain != null)
        {
            existingTerrain.SetActive(false);
            Debug.Log("Disabled old Terrain object.");
        }
        if (existingWalls != null)
        {
            existingWalls.SetActive(false);
            Debug.Log("Disabled old Walls object.");
        }
        if (existingSpikes != null)
        {
            existingSpikes.SetActive(false);
            Debug.Log("Disabled old Spikes object.");
        }
    }

    private static GameObject CreateTerrain(Transform parent)
    {
        GameObject terrain = new GameObject("Terrain");
        terrain.transform.SetParent(parent);
        terrain.layer = LayerMask.NameToLayer("Ground");

        // Create a Grid for tilemap
        Grid grid = terrain.AddComponent<Grid>();
        grid.cellSize = new Vector3(1, 1, 0);

        // Create tilemap for ground platforms
        GameObject tilemapObj = new GameObject("GroundTilemap");
        tilemapObj.transform.SetParent(terrain.transform);
        tilemapObj.layer = LayerMask.NameToLayer("Ground");
        
        Tilemap tilemap = tilemapObj.AddComponent<Tilemap>();
        TilemapRenderer renderer = tilemapObj.AddComponent<TilemapRenderer>();
        renderer.sortingLayerName = "Terrain";
        
        TilemapCollider2D collider = tilemapObj.AddComponent<TilemapCollider2D>();
        CompositeCollider2D composite = tilemapObj.AddComponent<CompositeCollider2D>();
        // CompositeCollider2D automatically adds a Rigidbody2D, so get it instead of adding
        Rigidbody2D rb = tilemapObj.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Failed to get Rigidbody2D component! Using collider-based approach instead.");
            CreateTerrainWithColliders(terrain.transform);
            return terrain;
        }
        rb.bodyType = RigidbodyType2D.Static;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        // Don't add PlatformEffector2D to main ground - it interferes with ground detection
        // PlatformEffector2D should only be on one-way platforms

        // Create a simple white tile for now (you can replace with actual terrain tiles)
        TileBase groundTile = CreateBasicTile(Color.gray);
        
        if (groundTile == null)
        {
            Debug.LogError("Failed to create ground tile! Using collider-based approach instead.");
            // Fallback: create platforms using colliders instead
            CreateTerrainWithColliders(terrain.transform);
            return terrain;
        }

        // Design: Multiple platform levels for swinging traversal
        // Ground floor - starting area
        for (int x = 0; x < 20; x++)
        {
            tilemap.SetTile(new Vector3Int(x, 0, 0), groundTile);
        }

        // Mid-level platforms (left side)
        for (int x = 5; x < 12; x++)
        {
            tilemap.SetTile(new Vector3Int(x, 8, 0), groundTile);
        }

        // High platform (right side) - requires swinging to reach
        for (int x = 25; x < 35; x++)
        {
            tilemap.SetTile(new Vector3Int(x, 15, 0), groundTile);
        }

        // Mid-level platform (center-right)
        for (int x = 40; x < 50; x++)
        {
            tilemap.SetTile(new Vector3Int(x, 10, 0), groundTile);
        }

        // Upper platform (far right) - final area
        for (int x = 60; x < 80; x++)
        {
            tilemap.SetTile(new Vector3Int(x, 20, 0), groundTile);
        }

        // Small platforms for intermediate swinging points
        for (int x = 15; x < 18; x++)
        {
            tilemap.SetTile(new Vector3Int(x, 5, 0), groundTile);
        }

        for (int x = 30; x < 33; x++)
        {
            tilemap.SetTile(new Vector3Int(x, 12, 0), groundTile);
        }

        for (int x = 55; x < 58; x++)
        {
            tilemap.SetTile(new Vector3Int(x, 18, 0), groundTile);
        }

        // Rebuild composite collider
        composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
        composite.generationType = CompositeCollider2D.GenerationType.Synchronous;
        collider.usedByComposite = true;
        
        Undo.RegisterCreatedObjectUndo(terrain, "Create Level 3 Terrain");
        EditorUtility.SetDirty(tilemapObj);

        return terrain;
    }

    private static GameObject CreateWalls(Transform parent)
    {
        GameObject walls = new GameObject("Walls");
        walls.transform.SetParent(parent);
        walls.layer = LayerMask.NameToLayer("Wall");

        // Create Grid
        Grid grid = walls.AddComponent<Grid>();
        grid.cellSize = new Vector3(1, 1, 0);

        // Create tilemap for walls
        GameObject tilemapObj = new GameObject("WallTilemap");
        tilemapObj.transform.SetParent(walls.transform);
        
        Tilemap tilemap = tilemapObj.AddComponent<Tilemap>();
        TilemapRenderer renderer = tilemapObj.AddComponent<TilemapRenderer>();
        renderer.sortingLayerName = "Terrain";
        
        TilemapCollider2D collider = tilemapObj.AddComponent<TilemapCollider2D>();
        CompositeCollider2D composite = tilemapObj.AddComponent<CompositeCollider2D>();
        // CompositeCollider2D automatically adds a Rigidbody2D, so get it instead of adding
        Rigidbody2D rb = tilemapObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }

        TileBase wallTile = CreateBasicTile(Color.gray);
        
        if (wallTile == null)
        {
            Debug.LogError("Failed to create wall tile! Walls will not be created.");
            return walls;
        }

        // Left wall - with gaps for vertical traversal
        for (int y = 0; y < 25; y++)
        {
            if (y < 3 || y > 22) // Leave gaps for entry/exit
            {
                tilemap.SetTile(new Vector3Int(-1, y, 0), wallTile);
            }
        }

        // Right wall - solid
        for (int y = 0; y < 30; y++)
        {
            tilemap.SetTile(new Vector3Int(85, y, 0), wallTile);
        }

        // Ceiling - with openings for swinging
        for (int x = 0; x < 85; x++)
        {
            if (x < 20 || (x > 25 && x < 40) || x > 60) // Leave gaps for vertical swinging
            {
                tilemap.SetTile(new Vector3Int(x, 25, 0), wallTile);
            }
        }

        // Rebuild composite collider
        composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
        composite.generationType = CompositeCollider2D.GenerationType.Synchronous;
        collider.usedByComposite = true;
        
        Undo.RegisterCreatedObjectUndo(walls, "Create Level 3 Walls");
        EditorUtility.SetDirty(tilemapObj);

        return walls;
    }

    private static void CreateSwingingPlatforms(Transform parent)
    {
        GameObject platforms = new GameObject("SwingingPlatforms");
        platforms.transform.SetParent(parent);

        // Create small floating platforms that can be used as intermediate landing points
        // These are positioned to create interesting swinging routes

        // Platform 1: Between starting area and mid-level
        CreatePlatform(platforms.transform, new Vector3(18, 6, 0), new Vector2(3, 1));

        // Platform 2: Between mid-level and high platform
        CreatePlatform(platforms.transform, new Vector3(22, 13, 0), new Vector2(2, 1));

        // Platform 3: Between high platform and center-right
        CreatePlatform(platforms.transform, new Vector3(38, 12, 0), new Vector2(2, 1));

        // Platform 4: Between center-right and upper platform
        CreatePlatform(platforms.transform, new Vector3(52, 19, 0), new Vector2(3, 1));
    }

    private static void CreatePlatform(Transform parent, Vector3 position, Vector2 size)
    {
        GameObject platform = new GameObject($"Platform_{position.x}_{position.y}");
        platform.transform.SetParent(parent);
        platform.transform.position = position;
        platform.layer = LayerMask.NameToLayer("Ground");

        // Create sprite renderer
        SpriteRenderer sr = platform.AddComponent<SpriteRenderer>();
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, new Color(0.6f, 0.6f, 0.6f, 1f));
        tex.Apply();
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
        sr.sprite = sprite;
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = size;
        sr.sortingLayerName = "Terrain";

        // Create collider
        BoxCollider2D collider = platform.AddComponent<BoxCollider2D>();
        collider.size = size;

        // Don't add PlatformEffector2D - these are solid landing platforms, not one-way platforms
        // PlatformEffector2D interferes with ground detection using OverlapCircle
        
        Undo.RegisterCreatedObjectUndo(platform, "Create Platform");
    }

    private static TileBase CreateBasicTile(Color color)
    {
        try
        {
            // Create a simple tile asset
            Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[16 * 16];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
            
            if (sprite == null)
            {
                Debug.LogError("Failed to create sprite for tile!");
                return null;
            }
            
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            if (tile == null)
            {
                Debug.LogError("Failed to create Tile instance!");
                return null;
            }
            
            tile.sprite = sprite;
            tile.name = "GeneratedTile_" + color.ToString();
            
            return tile;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating tile: {e.Message}");
            return null;
        }
    }
    
    private static void CreateTerrainWithColliders(Transform parent)
    {
        // Fallback method: create platforms using BoxCollider2D instead of tilemaps
        Debug.Log("Creating terrain using BoxCollider2D fallback method...");
        
        // Ground floor - starting area
        CreatePlatformCollider(parent, new Vector3(10, 0.5f, 0), new Vector2(20, 1), "GroundFloor");
        
        // Mid-level platforms (left side)
        CreatePlatformCollider(parent, new Vector3(8.5f, 8.5f, 0), new Vector2(7, 1), "MidLevelLeft");
        
        // High platform (right side)
        CreatePlatformCollider(parent, new Vector3(30, 15.5f, 0), new Vector2(10, 1), "HighPlatform");
        
        // Mid-level platform (center-right)
        CreatePlatformCollider(parent, new Vector3(45, 10.5f, 0), new Vector2(10, 1), "MidLevelRight");
        
        // Upper platform (far right)
        CreatePlatformCollider(parent, new Vector3(70, 20.5f, 0), new Vector2(20, 1), "UpperPlatform");
        
        // Small platforms for intermediate swinging points
        CreatePlatformCollider(parent, new Vector3(16.5f, 5.5f, 0), new Vector2(3, 1), "SmallPlatform1");
        CreatePlatformCollider(parent, new Vector3(31.5f, 12.5f, 0), new Vector2(3, 1), "SmallPlatform2");
        CreatePlatformCollider(parent, new Vector3(56.5f, 18.5f, 0), new Vector2(3, 1), "SmallPlatform3");
    }
    
    private static void CreatePlatformCollider(Transform parent, Vector3 position, Vector2 size, string name)
    {
        GameObject platform = new GameObject(name);
        platform.transform.SetParent(parent);
        platform.transform.position = position;
        platform.layer = LayerMask.NameToLayer("Ground");
        
        // Create sprite renderer
        SpriteRenderer sr = platform.AddComponent<SpriteRenderer>();
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.gray);
        tex.Apply();
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
        sr.sprite = sprite;
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = size;
        sr.sortingLayerName = "Terrain";
        
        // Create collider
        BoxCollider2D collider = platform.AddComponent<BoxCollider2D>();
        collider.size = size;
        
        // Don't add PlatformEffector2D - these are solid landing platforms
        // PlatformEffector2D interferes with ground detection using OverlapCircle
        
        Undo.RegisterCreatedObjectUndo(platform, "Create Platform Collider");
    }
}
