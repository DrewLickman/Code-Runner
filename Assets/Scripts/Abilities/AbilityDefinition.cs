using UnityEngine;

[CreateAssetMenu(menuName = "CodeRunner/Abilities/Ability", fileName = "Ability_")]
public class AbilityDefinition : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Stable id used for save data and gating rules (e.g. packet_hook).")]
    public string id;

    public string displayName;

    [TextArea]
    public string description;

    [Header("UI")]
    public Sprite icon;
}

