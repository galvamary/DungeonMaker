using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Dungeon Maker/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("Skill Info")]
    public string skillName;
    public Sprite icon;

    [Header("Costs")]
    public int mpCost = 5;

    [Header("Effects")]
    public SkillType skillType;
    public int power = 20;  // Additional power (0 = use only attack stat for basic attacks)
    public SkillTarget targetType;

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
