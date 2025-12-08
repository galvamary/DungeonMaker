using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles battle entity spawning, positioning, and cleanup
/// Responsible for creating and placing champion and monster entities in the battle scene
/// </summary>
public class BattleSetup : MonoBehaviour
{
    [Header("UI Sprites")]
    [SerializeField] private Sprite defenseShieldSprite;

    private BattleEntity championEntity;
    private List<BattleEntity> monsterEntities = new List<BattleEntity>();

    // Public accessors
    public BattleEntity ChampionEntity => championEntity;
    public List<BattleEntity> MonsterEntities => monsterEntities;

    /// <summary>
    /// Sets the defense shield sprite for all entities
    /// </summary>
    public void SetDefenseShieldSprite(Sprite sprite)
    {
        defenseShieldSprite = sprite;
    }

    /// <summary>
    /// Spawns all battle entities (champion and monsters)
    /// </summary>
    public IEnumerator SpawnBattleEntities(Champion champion, List<MonsterData> monsters)
    {
        // Wait for fade/background animation
        yield return new WaitForSeconds(0.5f);

        // Clear any existing entities
        ClearBattleEntities();

        if (BattleUI.Instance == null)
        {
            Debug.LogError("BattleUI instance not found!");
            yield break;
        }

        // Spawn champion
        yield return SpawnChampion(champion);

        // Spawn monsters
        yield return SpawnMonsters(monsters);

        Debug.Log($"Battle entities spawned: 1 champion vs {monsterEntities.Count} monsters");
    }

    /// <summary>
    /// Spawns the champion entity
    /// </summary>
    private IEnumerator SpawnChampion(Champion champion)
    {
        RectTransform championContainer = BattleUI.Instance.ChampionPositionContainer;
        if (championContainer == null)
        {
            Debug.LogError("Champion position container is not assigned in BattleUI!");
            yield break;
        }

        // Create champion GameObject
        GameObject championObj = new GameObject("ChampionEntity", typeof(RectTransform));
        championEntity = championObj.AddComponent<BattleEntity>();

        // Set defense shield sprite
        if (defenseShieldSprite != null)
        {
            championEntity.SetDefenseShieldSprite(defenseShieldSprite);
        }

        // Initialize champion data
        championEntity.InitializeFromChampion(champion);

        // Position champion
        RectTransform rectTransform = championObj.GetComponent<RectTransform>();
        PositionEntity(rectTransform, championContainer);
        rectTransform.eulerAngles = new Vector3(0f, 180f, 0f); // Face right

        // Save original position
        championEntity.SaveOriginalPosition();

        Debug.Log($"Champion spawned at {championContainer.position}");
        yield return null;
    }

    /// <summary>
    /// Spawns all monster entities
    /// </summary>
    private IEnumerator SpawnMonsters(List<MonsterData> monsters)
    {
        RectTransform[] monsterContainers = BattleUI.Instance.MonsterPositionContainers;

        for (int i = 0; i < monsters.Count; i++)
        {
            RectTransform containerTransform = GetMonsterPositionContainer(i, monsters.Count, monsterContainers);

            if (containerTransform == null)
            {
                Debug.LogError($"Monster position container {i} is not assigned in BattleUI!");
                continue;
            }

            // Create monster GameObject
            GameObject monsterObj = new GameObject($"MonsterEntity_{i}", typeof(RectTransform));
            BattleEntity monsterEntity = monsterObj.AddComponent<BattleEntity>();

            // Set defense shield sprite
            if (defenseShieldSprite != null)
            {
                monsterEntity.SetDefenseShieldSprite(defenseShieldSprite);
            }

            // Initialize monster data
            monsterEntity.InitializeFromMonster(monsters[i]);

            // Position monster
            RectTransform rectTransform = monsterObj.GetComponent<RectTransform>();
            PositionEntity(rectTransform, containerTransform);

            // Save original position
            monsterEntity.SaveOriginalPosition();

            monsterEntities.Add(monsterEntity);
            Debug.Log($"Monster {monsters[i].monsterName} spawned at {containerTransform.position}");
        }

        yield return null;
    }

    /// <summary>
    /// Positions an entity in its container
    /// </summary>
    private void PositionEntity(RectTransform entityTransform, RectTransform container)
    {
        entityTransform.SetParent(container);
        entityTransform.localPosition = Vector3.zero;
        entityTransform.anchorMin = new Vector2(0.5f, 0.5f);
        entityTransform.anchorMax = new Vector2(0.5f, 0.5f);
        entityTransform.sizeDelta = new Vector2(100, 100);
    }

    /// <summary>
    /// Gets the appropriate position container for a monster based on total count
    /// </summary>
    private RectTransform GetMonsterPositionContainer(int index, int totalCount, RectTransform[] containers)
    {
        switch (totalCount)
        {
            case 1:
                // Single monster in middle position (index 1)
                return containers[1];
            case 2:
                // Two monsters: top (0) and bottom (2)
                return (index == 0) ? containers[0] : containers[2];
            case 3:
                // Three monsters: use all positions
                return containers[index];
            default:
                return containers[1]; // Fallback to middle
        }
    }

    /// <summary>
    /// Clears all battle entities
    /// </summary>
    public void ClearBattleEntities()
    {
        if (championEntity != null)
        {
            Destroy(championEntity.gameObject);
            championEntity = null;
        }

        foreach (var entity in monsterEntities)
        {
            if (entity != null)
            {
                Destroy(entity.gameObject);
            }
        }
        monsterEntities.Clear();
    }

    /// <summary>
    /// Gets all alive monsters
    /// </summary>
    public List<BattleEntity> GetAliveMonsters()
    {
        List<BattleEntity> aliveMonsters = new List<BattleEntity>();
        foreach (var monster in monsterEntities)
        {
            if (monster != null && monster.IsAlive)
            {
                aliveMonsters.Add(monster);
            }
        }
        return aliveMonsters;
    }

    /// <summary>
    /// Gets all alive entities (champion + monsters)
    /// </summary>
    public List<BattleEntity> GetAllAliveEntities()
    {
        List<BattleEntity> entities = new List<BattleEntity>();

        if (championEntity != null && championEntity.IsAlive)
        {
            entities.Add(championEntity);
        }

        foreach (var monster in monsterEntities)
        {
            if (monster != null && monster.IsAlive)
            {
                entities.Add(monster);
            }
        }

        return entities;
    }
}
