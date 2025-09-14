using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }
    
    [Header("Shop Settings")]
    [SerializeField] private List<MonsterData> availableMonsters = new List<MonsterData>();
    
    // 구매한 몬스터 개수 추적
    private Dictionary<MonsterData, int> ownedMonsters = new Dictionary<MonsterData, int>();
    
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
            
            // 몬스터 개수 업데이트
            if (ownedMonsters.ContainsKey(monster))
            {
                ownedMonsters[monster]++;
            }
            else
            {
                ownedMonsters[monster] = 1;
            }
            
            // 인벤토리 UI 업데이트
            if (MonsterInventoryUI.Instance != null)
            {
                MonsterInventoryUI.Instance.UpdateMonsterInventory(monster, ownedMonsters[monster]);
            }
            
            Debug.Log($"Purchased {monster.monsterName} for {monster.cost} gold. Total owned: {ownedMonsters[monster]}");
            return true;
        }
        else
        {
            Debug.Log($"Not enough gold to purchase {monster.monsterName}. Cost: {monster.cost}, Current: {GameManager.Instance.CurrentGold}");
            return false;
        }
    }
    
    public int GetMonsterCount(MonsterData monster)
    {
        return ownedMonsters.ContainsKey(monster) ? ownedMonsters[monster] : 0;
    }
    
    public List<MonsterData> GetAvailableMonsters()
    {
        return availableMonsters;
    }
}