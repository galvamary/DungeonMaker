using UnityEngine;

[CreateAssetMenu(fileName = "NewChampion", menuName = "Dungeon Maker/Champion Data")]
public class ChampionData : ScriptableObject
{
    [Header("Champion Info")]
    public string championName;
    public Sprite icon;

    [Header("Stats")]
    public int maxHealth = 100;
    public int attack = 10;
    public int defense = 5;

    [Header("Rewards")]
    public int goldReward = 50;
}
