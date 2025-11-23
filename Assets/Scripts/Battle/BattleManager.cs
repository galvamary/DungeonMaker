using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Battle State")]
    private bool isBattleActive = false;
    private Champion currentChampion;
    private List<MonsterData> currentMonsters;
    private Room currentRoom;

    [Header("Battle Entities")]
    private BattleEntity championEntity;
    private List<BattleEntity> monsterEntities = new List<BattleEntity>();

    public bool IsBattleActive => isBattleActive;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartBattle(Champion champion, Room room)
    {
        if (isBattleActive)
        {
            Debug.LogWarning("Battle already in progress!");
            return;
        }

        if (room == null || !room.HasMonster)
        {
            Debug.LogWarning("Cannot start battle: No monsters in room!");
            return;
        }

        isBattleActive = true;
        currentChampion = champion;
        currentRoom = room;
        currentMonsters = new List<MonsterData>(room.PlacedMonsters);

        Debug.Log($"Battle started! {champion.Data.championName} vs {currentMonsters.Count} monsters in room {room.GridPosition}");

        // Show battle UI
        if (BattleUI.Instance != null)
        {
            BattleUI.Instance.ShowBattleBackground();
        }

        // Spawn battle entities
        StartCoroutine(SpawnBattleEntities());
    }

    private IEnumerator SpawnBattleEntities()
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

        // Spawn champion entity
        RectTransform championContainer = BattleUI.Instance.ChampionPositionContainer;
        if (championContainer != null)
        {
            GameObject championObj = new GameObject("ChampionEntity", typeof(RectTransform));
            championEntity = championObj.AddComponent<BattleEntity>();
            championEntity.InitializeFromChampion(currentChampion);

            // Set as child of position container
            RectTransform rectTransform = championObj.GetComponent<RectTransform>();
            rectTransform.SetParent(championContainer);
            rectTransform.localPosition = Vector3.zero;
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(100, 100);
            rectTransform.eulerAngles = new Vector3(0f, 180f, 0f);

            Debug.Log($"Champion spawned at {championContainer.position}");
        }
        else
        {
            Debug.LogError("Champion position container is not assigned in BattleUI!");
        }

        // Spawn monster entities
        RectTransform[] monsterContainers = BattleUI.Instance.MonsterPositionContainers;
        for (int i = 0; i < currentMonsters.Count; i++)
        {
            RectTransform containerTransform = GetMonsterPositionContainer(i, currentMonsters.Count, monsterContainers);

            if (containerTransform != null)
            {
                GameObject monsterObj = new GameObject($"MonsterEntity_{i}", typeof(RectTransform));
                BattleEntity monsterEntity = monsterObj.AddComponent<BattleEntity>();
                monsterEntity.InitializeFromMonster(currentMonsters[i]);

                // Set as child of position container
                RectTransform rectTransform = monsterObj.GetComponent<RectTransform>();
                rectTransform.SetParent(containerTransform);
                rectTransform.localPosition = Vector3.zero;
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.sizeDelta = new Vector2(100, 100);

                monsterEntities.Add(monsterEntity);
                Debug.Log($"Monster {currentMonsters[i].monsterName} spawned at {containerTransform.position}");
            }
            else
            {
                Debug.LogError($"Monster position container {i} is not assigned in BattleUI!");
            }
        }

        Debug.Log($"Battle entities spawned: 1 champion vs {monsterEntities.Count} monsters");
    }

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

    private void ClearBattleEntities()
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

    public void EndBattle(bool championWon)
    {
        if (!isBattleActive)
        {
            Debug.LogWarning("No battle to end!");
            return;
        }

        Debug.Log($"Battle ended! Champion won: {championWon}");

        // Clear battle entities
        ClearBattleEntities();

        // Hide battle UI
        if (BattleUI.Instance != null)
        {
            BattleUI.Instance.HideBattleBackground();
        }

        // Clear monsters from room if champion won
        if (championWon && currentRoom != null)
        {
            currentRoom.RemoveAllMonsters();
        }

        isBattleActive = false;
        currentChampion = null;
        currentMonsters = null;
        currentRoom = null;

        // TODO: Resume exploration
    }
}
