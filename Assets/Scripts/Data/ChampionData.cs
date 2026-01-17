using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewChampion", menuName = "Dungeon Maker/Champion Data")]
public class ChampionData : ScriptableObject
{
    [Header("Champion Info")]
    public string championName;

    [Header("Fatigue Sprites")]
    [Tooltip("Sprite when fatigue <= 50%")]
    public Sprite normalSprite;
    [Tooltip("Sprite when 50% < fatigue <= 90%")]
    public Sprite tiredSprite;
    [Tooltip("Sprite when fatigue > 90%")]
    public Sprite exhaustedSprite;

    [Header("Stats")]
    public int maxHealth = 100;
    public int maxMP = 50;
    public int attack = 10;
    public int defense = 5;
    public int speed = 5;

    [Header("Skills")]
    public SkillData basicAttack;  // Basic attack (0 MP cost)
    public List<SkillData> skills = new List<SkillData>();  // Special skills

    [Header("Rewards")]
    public int goldReward = 50;
}
