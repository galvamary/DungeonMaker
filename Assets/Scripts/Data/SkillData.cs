using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Dungeon Maker/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("Skill Info")]
    public string skillName;
    public Sprite icon;

    [Header("Unlock Requirement")]
    [Tooltip("Minimum treasure rooms required to unlock this skill (0 = always available)")]
    public int requiredTreasureRooms = 0;

    [Header("Costs")]
    public int mpCost = 5;

    [Header("Effects")]
    public SkillType skillType;
    public int power = 20;  // Additional power (0 = use only attack stat for basic attacks)
    public SkillTarget targetType;
    public GameObject effectPrefab;  // Visual effect prefab

    [Header("Description")]
    [TextArea(2, 4)]
    public string description;
}

public enum SkillType
{
    Attack,      // Deals damage
    Heal,        // Heals HP
    Buff,        // Increases stats (future)
    Debuff       // Decreases enemy stats (future)
}

public enum SkillTarget
{
    SingleEnemy,   // One random enemy
    AllEnemies,    // All enemies
    Self,          // The caster
    SingleAlly,    // One ally (future for multi-champion)
    AllAllies      // All allies (future for multi-champion)
}
