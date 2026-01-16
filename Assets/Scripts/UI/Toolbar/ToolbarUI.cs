using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[AddComponentMenu("UI/Player Toolbar UI")]
public class ToolbarUI : MonoBehaviour
{
    [Header("Toolbar")]
    [Min(1)] public int slotCount = 5;
    [Min(0)] public int selectedIndex = 0;
    public bool showLabels;

    [Header("Layout")]
    public Vector2 slotSize = new Vector2(64, 64);
    [Min(0)] public float slotSpacing = 8f;
    public Vector2 padding = new Vector2(12, 12);
    [Min(0)] public float bottomOffset = 20f;

    [Header("Style")]
    public Color unselectedBackground = new Color(0f, 0f, 0f, 0.45f);
    public Color selectedBackground = new Color(1f, 1f, 1f, 0.15f);
    public Color unselectedOutline = new Color(0f, 0f, 0f, 0.65f);
    public Color selectedOutline = new Color(1f, 1f, 1f, 0.9f);
    [Min(0)] public float outlineDistance = 2f;
    [Min(0)] public float iconPadding = 8f;

    [Header("Defaults (for now)")]
    public string defaultEquippedName = "Sword Slash";
    public Sprite defaultEquippedIcon;

    private readonly List<ToolbarSlotUI> slots = new List<ToolbarSlotUI>(8);
    private readonly List<ToolbarItem> items = new List<ToolbarItem>(8);

    private void Awake()
    {
        EnsureBuilt();
    }

    private void Start()
    {
        // Initialize default toolbar contents if nothing has been set yet.
        if (items.Count == 0)
        {
            items.Clear();
            for (int i = 0; i < slotCount; i++)
                items.Add(default);

            items[0] = new ToolbarItem(defaultEquippedName, defaultEquippedIcon);
        }

        ApplyToView();
    }

    public void SetItems(IReadOnlyList<ToolbarItem> newItems)
    {
        EnsureBuilt();

        items.Clear();
        if (newItems != null)
        {
            for (int i = 0; i < newItems.Count && i < slotCount; i++)
                items.Add(newItems[i]);
        }

        while (items.Count < slotCount)
            items.Add(default);

        ApplyToView();
    }

    public void SetSelectedIndex(int index)
    {
        EnsureBuilt();
        selectedIndex = Mathf.Clamp(index, 0, slotCount - 1);
        ApplySelection();
    }

    public void SetSlot(int index, ToolbarItem item)
    {
        EnsureBuilt();

        if (index < 0 || index >= slotCount) return;
        while (items.Count < slotCount) items.Add(default);

        items[index] = item;
        ApplyToView();
    }

    private void EnsureBuilt()
    {
        slotCount = Mathf.Max(1, slotCount);
        selectedIndex = Mathf.Clamp(selectedIndex, 0, slotCount - 1);

        RectTransform root = GetComponent<RectTransform>();
        if (root == null) root = gameObject.AddComponent<RectTransform>();

        // Bottom-center by default.
        root.anchorMin = new Vector2(0.5f, 0f);
        root.anchorMax = new Vector2(0.5f, 0f);
        root.pivot = new Vector2(0.5f, 0f);
        root.anchoredPosition = new Vector2(0f, bottomOffset);

        Transform containerTr = transform.Find("SlotContainer");
        RectTransform container;
        if (containerTr == null)
        {
            GameObject go = new GameObject("SlotContainer", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            container = go.GetComponent<RectTransform>();
        }
        else
        {
            container = containerTr.GetComponent<RectTransform>();
            if (container == null) container = containerTr.gameObject.AddComponent<RectTransform>();
        }

        container.anchorMin = new Vector2(0.5f, 0.5f);
        container.anchorMax = new Vector2(0.5f, 0.5f);
        container.pivot = new Vector2(0.5f, 0.5f);
        container.anchoredPosition = Vector2.zero;

        HorizontalLayoutGroup hlg = container.GetComponent<HorizontalLayoutGroup>();
        if (hlg == null) hlg = container.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.spacing = slotSpacing;
        hlg.padding = new RectOffset(Mathf.RoundToInt(padding.x), Mathf.RoundToInt(padding.x), Mathf.RoundToInt(padding.y), Mathf.RoundToInt(padding.y));
        hlg.childControlHeight = true;
        hlg.childControlWidth = true;
        hlg.childForceExpandHeight = false;
        hlg.childForceExpandWidth = false;

        ContentSizeFitter fitter = container.GetComponent<ContentSizeFitter>();
        if (fitter == null) fitter = container.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        RebuildSlots(container);
    }

    private void RebuildSlots(RectTransform container)
    {
        slots.Clear();

        // Try to re-use existing children if present.
        List<ToolbarSlotUI> existing = new List<ToolbarSlotUI>(container.childCount);
        for (int i = 0; i < container.childCount; i++)
        {
            ToolbarSlotUI slot = container.GetChild(i).GetComponent<ToolbarSlotUI>();
            if (slot != null) existing.Add(slot);
        }

        // Create missing slots.
        for (int i = 0; i < slotCount; i++)
        {
            ToolbarSlotUI slot = i < existing.Count ? existing[i] : CreateSlot(container, i);
            slot.index = i;
            slot.ConfigureColors(unselectedBackground, selectedBackground, unselectedOutline, selectedOutline);
            slots.Add(slot);
        }

        // Remove extras (only in editor/runtime when this component runs).
        for (int i = container.childCount - 1; i >= slotCount; i--)
        {
            Destroy(container.GetChild(i).gameObject);
        }
    }

    private ToolbarSlotUI CreateSlot(RectTransform parent, int index)
    {
        GameObject slotGo = new GameObject($"Slot_{index + 1:00}", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(LayoutElement), typeof(ToolbarSlotUI));
        slotGo.transform.SetParent(parent, false);

        RectTransform rt = slotGo.GetComponent<RectTransform>();
        rt.sizeDelta = slotSize;

        LayoutElement le = slotGo.GetComponent<LayoutElement>();
        le.preferredWidth = slotSize.x;
        le.preferredHeight = slotSize.y;
        le.minWidth = slotSize.x;
        le.minHeight = slotSize.y;

        Image bg = slotGo.GetComponent<Image>();
        bg.raycastTarget = false;
        bg.color = unselectedBackground;

        Outline outline = slotGo.GetComponent<Outline>();
        outline.effectColor = unselectedOutline;
        outline.effectDistance = new Vector2(outlineDistance, outlineDistance);
        outline.useGraphicAlpha = true;

        // Icon
        GameObject iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(slotGo.transform, false);
        RectTransform iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.anchorMin = Vector2.zero;
        iconRt.anchorMax = Vector2.one;
        iconRt.offsetMin = new Vector2(iconPadding, iconPadding);
        iconRt.offsetMax = new Vector2(-iconPadding, -iconPadding);
        Image icon = iconGo.GetComponent<Image>();
        icon.raycastTarget = false;
        icon.preserveAspect = true;
        icon.enabled = false;

        // Label (optional, off by default)
        GameObject labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(slotGo.transform, false);
        RectTransform labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.anchorMin = new Vector2(0f, 0f);
        labelRt.anchorMax = new Vector2(1f, 0f);
        labelRt.pivot = new Vector2(0.5f, 0f);
        labelRt.anchoredPosition = new Vector2(0f, 2f);
        labelRt.sizeDelta = new Vector2(0f, 18f);
        TextMeshProUGUI tmp = labelGo.GetComponent<TextMeshProUGUI>();
        tmp.text = "";
        tmp.fontSize = 14;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        tmp.enabled = false;

        ToolbarSlotUI slot = slotGo.GetComponent<ToolbarSlotUI>();
        slot.background = bg;
        slot.icon = icon;
        slot.outline = outline;
        slot.label = tmp;
        slot.ConfigureColors(unselectedBackground, selectedBackground, unselectedOutline, selectedOutline);
        return slot;
    }

    private void ApplyToView()
    {
        EnsureBuilt();

        while (items.Count < slotCount)
            items.Add(default);

        for (int i = 0; i < slots.Count; i++)
        {
            ToolbarItem item = i < items.Count ? items[i] : default;
            if (item.IsEmpty) slots[i].Clear(showLabels);
            else slots[i].SetItem(item, showLabels);
        }

        ApplySelection();
    }

    private void ApplySelection()
    {
        selectedIndex = Mathf.Clamp(selectedIndex, 0, slotCount - 1);
        for (int i = 0; i < slots.Count; i++)
            slots[i].SetSelected(i == selectedIndex);
    }
}

