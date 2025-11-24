using UnityEngine;

[CreateAssetMenu(fileName = "New Monster", menuName = "Dungeon Maker/Monster Data")]
public class MonsterData : ScriptableObject
{
    [Header("Basic Info")]
    public string monsterName = "New Monster";
    public Sprite icon;
    public int cost = 100;
    
    [Header("Stats")]
    public int health = 10;
    public int attack = 5;
    public int defense = 2;
    public int speed = 3;
    public float attackSpeed = 1.0f;
    
    [Header("Description")]
    [TextArea(3, 5)]
    public string description = "A basic monster for your dungeon.";
    
    [Header("Prefab")]
    public GameObject monsterPrefab;
}