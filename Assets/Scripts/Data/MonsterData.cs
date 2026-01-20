using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Monster", menuName = "Dungeon Maker/Monster Data")]
public class MonsterData : ScriptableObject
{
    [Header("Basic Info")]
    public string monsterName = "New Monster";
    public Sprite icon;
    public int cost = 100;
    
    [Header("Stats")]
    public int maxHealth = 10;
    public int maxMP = 0;
    public int attack = 5;
    public int defense = 2;
    public int speed = 3;

    [Header("Skills")]
    public SkillData basicAttack;  // Basic attack (0 MP cost)
    public List<SkillData> skills = new List<SkillData>();  // Special skills

    [Header("Respawn Settings")]
    [Tooltip("If true, this monster will respawn in the room after being defeated, allowing champions to fight it again")]
    public bool canRespawn = false;

    [Header("Description")]
    [TextArea(3, 5)]
    public string description = "A basic monster for your dungeon.";
}