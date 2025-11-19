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

    [Header("Battle Positions")]
    [SerializeField] private Vector3 championPosition = new Vector3(-3f, 0f, 0f);
    [SerializeField] private readonly Vector3[] monsterPositions =
    {
        new Vector3(3f, 1.5f, 0f),   // Top
        new Vector3(3f, 0f, 0f),      // Middle
        new Vector3(3f, -1.5f, 0f)    // Bottom
    };

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

        // Spawn champion entity
        GameObject championObj = new GameObject("ChampionEntity");
        championEntity = championObj.AddComponent<BattleEntity>();
        championEntity.InitializeFromChampion(currentChampion);
        championEntity.SetPosition(championPosition);

        Debug.Log($"Champion spawned at {championPosition}");

        // Spawn monster entities
        for (int i = 0; i < currentMonsters.Count; i++)
        {
            GameObject monsterObj = new GameObject($"MonsterEntity_{i}");
            BattleEntity monsterEntity = monsterObj.AddComponent<BattleEntity>();
            monsterEntity.InitializeFromMonster(currentMonsters[i]);

            // Position based on monster count
            Vector3 position = GetMonsterBattlePosition(i, currentMonsters.Count);
            monsterEntity.SetPosition(position);

            monsterEntities.Add(monsterEntity);
            Debug.Log($"Monster {currentMonsters[i].monsterName} spawned at {position}");
        }

        Debug.Log($"Battle entities spawned: 1 champion vs {monsterEntities.Count} monsters");
    }

    private Vector3 GetMonsterBattlePosition(int index, int totalCount)
    {
        switch (totalCount)
        {
            case 1:
                // Single monster in middle position
                return monsterPositions[1];
            case 2:
                // Two monsters: top and bottom
                return (index == 0) ? monsterPositions[0] : monsterPositions[2];
            case 3:
                // Three monsters: use all positions
                return monsterPositions[index];
            default:
                return monsterPositions[1]; // Fallback to middle
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
