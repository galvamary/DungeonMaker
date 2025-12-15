using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all monster status displays in battle
/// Shows monsters in their deployment order
/// </summary>
public class MonsterStatusPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform statusContainer;
    [SerializeField] private GameObject statusDisplayPrefab;

    private List<MonsterStatusDisplay> statusDisplays = new List<MonsterStatusDisplay>();

    /// <summary>
    /// Initialize status displays for all monsters in deployment order
    /// </summary>
    public void Initialize(List<BattleEntity> monsters)
    {
        ClearStatusDisplays();

        if (monsters == null || monsters.Count == 0)
        {
            Debug.LogWarning("MonsterStatusPanel: No monsters to display");
            return;
        }

        // Create status display for each monster in order
        foreach (var monster in monsters)
        {
            if (monster != null && monster.IsMonster)
            {
                CreateStatusDisplay(monster);
            }
        }

        Debug.Log($"MonsterStatusPanel: Initialized {statusDisplays.Count} status displays");
    }

    /// <summary>
    /// Create a status display for a single monster
    /// </summary>
    private void CreateStatusDisplay(BattleEntity monster)
    {
        if (statusDisplayPrefab == null || statusContainer == null)
        {
            Debug.LogError("MonsterStatusPanel: Missing prefab or container reference");
            return;
        }

        GameObject displayObj = Instantiate(statusDisplayPrefab, statusContainer);
        MonsterStatusDisplay display = displayObj.GetComponent<MonsterStatusDisplay>();

        if (display != null)
        {
            display.Initialize(monster);
            statusDisplays.Add(display);
        }
        else
        {
            Debug.LogError("MonsterStatusPanel: Status display prefab missing MonsterStatusDisplay component");
            Destroy(displayObj);
        }
    }

    /// <summary>
    /// Update only HP for a specific monster
    /// </summary>
    public void UpdateMonsterHP(BattleEntity monster)
    {
        if (monster == null) return;

        MonsterStatusDisplay display = statusDisplays.Find(d => d.Monster == monster);
        if (display != null)
        {
            display.UpdateHP();
        }
    }

    /// <summary>
    /// Update only MP for a specific monster
    /// </summary>
    public void UpdateMonsterMP(BattleEntity monster)
    {
        if (monster == null) return;

        MonsterStatusDisplay display = statusDisplays.Find(d => d.Monster == monster);
        if (display != null)
        {
            display.UpdateMP();
        }
    }

    /// <summary>
    /// Clear all status displays
    /// </summary>
    public void ClearStatusDisplays()
    {
        foreach (var display in statusDisplays)
        {
            if (display != null)
            {
                Destroy(display.gameObject);
            }
        }
        statusDisplays.Clear();
    }

    private void OnDestroy()
    {
        ClearStatusDisplays();
    }
}
