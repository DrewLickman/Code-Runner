using System;
using UnityEngine;

[Serializable]
public struct ToolbarItem
{
    [Tooltip("Display name for the toolbar slot (optional).")]
    public string displayName;

    [Tooltip("Icon sprite for the toolbar slot (optional).")]
    public Sprite icon;

    public ToolbarItem(string name, Sprite sprite)
    {
        displayName = name;
        icon = sprite;
    }

    public bool IsEmpty => string.IsNullOrWhiteSpace(displayName) && icon == null;
}

