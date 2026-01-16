using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[AddComponentMenu("Player/Player Toolbar Installer")]
public class PlayerToolbarInstaller : MonoBehaviour
{
    [Header("Toolbar")]
    [Min(1)] public int slotCount = 5;
    [Min(0)] public int selectedIndex = 0;
    public bool showLabels;

    [Header("HUD Canvas")]
    public string hudRootName = "PlayerHUD";
    public bool dontDestroyOnLoad = true;

    private void Awake()
    {
        // If a toolbar already exists, don't create another.
        if (FindFirstObjectByType<ToolbarUI>() != null)
            return;

        Canvas canvas = GetOrCreateHudCanvas();
        EnsureEventSystem();

        GameObject toolbarGo = new GameObject("Toolbar", typeof(RectTransform), typeof(ToolbarUI));
        toolbarGo.transform.SetParent(canvas.transform, false);

        ToolbarUI toolbar = toolbarGo.GetComponent<ToolbarUI>();
        toolbar.slotCount = slotCount;
        toolbar.selectedIndex = Mathf.Clamp(selectedIndex, 0, slotCount - 1);
        toolbar.showLabels = showLabels;
        toolbar.defaultEquippedName = "Sword Slash";
        toolbar.defaultEquippedIcon = ResolveSwordSlashSprite();
    }

    private Canvas GetOrCreateHudCanvas()
    {
        GameObject existing = GameObject.Find(hudRootName);
        if (existing != null && existing.TryGetComponent(out Canvas existingCanvas))
            return existingCanvas;

        GameObject hudRoot = new GameObject(hudRootName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = hudRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500; // High enough to be above typical world-space UI.

        CanvasScaler scaler = hudRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        if (dontDestroyOnLoad)
            DontDestroyOnLoad(hudRoot);

        return canvas;
    }

    private void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
            return;

        GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        if (dontDestroyOnLoad)
            DontDestroyOnLoad(es);
    }

    private Sprite ResolveSwordSlashSprite()
    {
        // Prefer the player's configured slash effect sprite, so the HUD matches the actual VFX.
        PlayerMovementConfig cfg = GetComponent<PlayerMovementConfig>();
        if (cfg != null && cfg.slashEffect != null)
        {
            SpriteRenderer sr = cfg.slashEffect.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
                return sr.sprite;

            SpriteRenderer childSr = cfg.slashEffect.GetComponentInChildren<SpriteRenderer>();
            if (childSr != null && childSr.sprite != null)
                return childSr.sprite;
        }

        return null;
    }
}

