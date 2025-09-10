using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }
    
    [Header("Shop Settings")]
    [SerializeField] private List<MonsterData> availableMonsters = new List<MonsterData>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public bool PurchaseMonster(MonsterData monster)
    {
        if (monster == null) return false;
        
        if (GameManager.Instance.CanAfford(monster.cost))
        {
            GameManager.Instance.SpendGold(monster.cost);
            Debug.Log($"Purchased {monster.monsterName} for {monster.cost} gold");
            return true;
        }
        else
        {
            Debug.Log($"Not enough gold to purchase {monster.monsterName}. Cost: {monster.cost}, Current: {GameManager.Instance.CurrentGold}");
            return false;
        }
    }
    
    public List<MonsterData> GetAvailableMonsters()
    {
        return availableMonsters;
    }
}