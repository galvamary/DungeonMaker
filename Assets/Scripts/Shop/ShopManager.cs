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

    public bool UseMonsterFromInventory(MonsterData monster)
    {
        if (monster == null) return false;

        if (ownedMonsters.ContainsKey(monster) && ownedMonsters[monster] > 0)
        {
            ownedMonsters[monster]--;

            // Update inventory UI
            if (MonsterInventoryUI.Instance != null)
            {
                MonsterInventoryUI.Instance.UpdateMonsterInventory(monster, ownedMonsters[monster]);
            }

            Debug.Log($"Used {monster.monsterName}. Remaining: {ownedMonsters[monster]}");
            return true;
        }

        Debug.Log($"No {monster.monsterName} available in inventory");
        return false;
    }

    public void ReturnMonsterToInventory(MonsterData monster)
    {
        if (monster == null) return;

        // Add monster back to inventory
        if (ownedMonsters.ContainsKey(monster))
        {
            ownedMonsters[monster]++;
        }
        else
        {
            ownedMonsters[monster] = 1;
        }

        // Update inventory UI
        if (MonsterInventoryUI.Instance != null)
        {
            MonsterInventoryUI.Instance.UpdateMonsterInventory(monster, ownedMonsters[monster]);
        }

        Debug.Log($"Returned {monster.monsterName} to inventory. Total: {ownedMonsters[monster]}");
    }

    /// <summary>
    /// Sells a monster from inventory back to the shop
    /// </summary>
    /// <param name="monster">Monster to sell</param>
    /// <returns>True if successfully sold</returns>
    public bool SellMonster(MonsterData monster)
    {
        if (monster == null)
        {
            Debug.LogWarning("Cannot sell null monster!");
            return false;
        }

        // Check if player owns this monster
        if (!ownedMonsters.ContainsKey(monster) || ownedMonsters[monster] <= 0)
        {
            Debug.LogWarning($"No {monster.monsterName} to sell!");
            return false;
        }

        // Remove monster from inventory
        ownedMonsters[monster]--;

        // Add gold to player
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(monster.cost);
        }

        // Update inventory UI
        if (MonsterInventoryUI.Instance != null)
        {
            MonsterInventoryUI.Instance.UpdateMonsterInventory(monster, ownedMonsters[monster]);
        }

        Debug.Log($"Sold {monster.monsterName} for {monster.cost} gold. Remaining: {ownedMonsters[monster]}");
        return true;
    }

    /// <summary>
    /// Resets monster inventory - clears all owned monsters
    /// </summary>
    public void ResetInventory()
    {
        Debug.Log("Resetting monster inventory...");

        // Clear owned monsters dictionary
        ownedMonsters.Clear();

        // Update inventory UI to reflect empty state
        if (MonsterInventoryUI.Instance != null)
        {
            // Clear all monster displays
            foreach (var monster in availableMonsters)
            {
                MonsterInventoryUI.Instance.UpdateMonsterInventory(monster, 0);
            }
        }

        Debug.Log("Monster inventory reset complete.");
    }

    /// <summary>
    /// Saves the current inventory state to the save state object
    /// </summary>
    public void SaveInventoryState(SaveState saveState)
    {
        saveState.savedInventory.Clear();

        // Save all owned monsters by expanding the dictionary into a list
        foreach (var kvp in ownedMonsters)
        {
            MonsterData monster = kvp.Key;
            int count = kvp.Value;

            // Add this monster 'count' times to the list
            for (int i = 0; i < count; i++)
            {
                saveState.savedInventory.Add(monster);
            }
        }

        Debug.Log($"Saved {saveState.savedInventory.Count} monsters to inventory save state");
    }

    /// <summary>
    /// Restores inventory state from the save state object
    /// </summary>
    public void RestoreInventoryState(SaveState saveState)
    {
        Debug.Log("Restoring inventory state...");

        // Clear current inventory
        ownedMonsters.Clear();

        // Restore monsters from saved list
        foreach (var monster in saveState.savedInventory)
        {
            if (ownedMonsters.ContainsKey(monster))
            {
                ownedMonsters[monster]++;
            }
            else
            {
                ownedMonsters[monster] = 1;
            }
        }

        // Update inventory UI
        if (MonsterInventoryUI.Instance != null)
        {
            // First clear all displays
            foreach (var monster in availableMonsters)
            {
                MonsterInventoryUI.Instance.UpdateMonsterInventory(monster, 0);
            }

            // Then update with restored counts
            foreach (var kvp in ownedMonsters)
            {
                MonsterInventoryUI.Instance.UpdateMonsterInventory(kvp.Key, kvp.Value);
            }
        }

        Debug.Log($"Restored {saveState.savedInventory.Count} monsters to inventory");
    }

    /// <summary>
    /// Adds a monster to inventory when loading from persistent save (PlayerPrefs)
    /// This is used by the load system and doesn't deduct gold
    /// </summary>
    public void AddMonsterToInventoryFromLoad(MonsterData monster)
    {
        if (monster == null) return;

        // Add monster to inventory
        if (ownedMonsters.ContainsKey(monster))
        {
            ownedMonsters[monster]++;
        }
        else
        {
            ownedMonsters[monster] = 1;
        }

        // Update inventory UI
        if (MonsterInventoryUI.Instance != null)
        {
            MonsterInventoryUI.Instance.UpdateMonsterInventory(monster, ownedMonsters[monster]);
        }
    }
}