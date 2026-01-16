using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ToolbarSlotUI : MonoBehaviour
{
    [Header("Wired by ToolbarUI (or manually)")]
    public int index;
    public Image background;
    public Image icon;
    public Outline outline;
    public TextMeshProUGUI label;

    private Color unselectedBg;
    private Color selectedBg;
    private Color unselectedOutline;
    private Color selectedOutline;

    public void ConfigureColors(Color unselectedBackground, Color selectedBackground, Color unselectedOutlineColor, Color selectedOutlineColor)
    {
        unselectedBg = unselectedBackground;
        selectedBg = selectedBackground;
        unselectedOutline = unselectedOutlineColor;
        selectedOutline = selectedOutlineColor;
    }

    public void SetItem(ToolbarItem item, bool showLabel)
    {
        if (icon != null)
        {
            icon.sprite = item.icon;
            icon.enabled = item.icon != null;
        }

        if (label != null)
        {
            label.text = item.displayName ?? string.Empty;
            label.enabled = showLabel && !string.IsNullOrWhiteSpace(label.text);
        }
    }

    public void Clear(bool showLabel)
    {
        SetItem(default, showLabel);
    }

    public void SetSelected(bool selected)
    {
        if (background != null)
            background.color = selected ? selectedBg : unselectedBg;

        if (outline != null)
        {
            outline.effectColor = selected ? selectedOutline : unselectedOutline;
            outline.enabled = true;
        }
    }
}

