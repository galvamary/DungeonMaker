using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Main battle coordinator - manages battle flow and delegates to specialized systems
/// Responsibilities: Battle state, component coordination, and public API
/// </summary>
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Battle State")]
    private bool isBattleActive = false;
    private Champion currentChampion;
    private List<MonsterData> currentMonsters;
    private Room currentRoom;

    [Header("UI Sprites")]
    [SerializeField] private Sprite defenseShieldSprite;

    [Header("UI Panels")]
    [SerializeField] private MonsterActionPanel monsterActionPanel;
    [SerializeField] private MonsterStatusPanel monsterStatusPanel;

    [Header("Battle Systems")]
    private BattleSetup battleSetup;
    private BattleTurnSystem turnSystem;
    private ChampionAI championAI;
    private MonsterController monsterController;

    // Public properties
    public bool IsBattleActive => isBattleActive;
    public BattleEntity CurrentTurnEntity => turnSystem?.CurrentTurnEntity;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize battle systems
        InitializeSystems();
    }

    /// <summary>
    /// Initializes all battle subsystems
    /// </summary>
    private void InitializeSystems()
    {
        // Get or add battle setup component
        battleSetup = GetComponent<BattleSetup>();
        if (battleSetup == null)
        {
            battleSetup = gameObject.AddComponent<BattleSetup>();
        }

        // Set defense shield sprite to battle setup
        if (defenseShieldSprite != null)
        {
            battleSetup.SetDefenseShieldSprite(defenseShieldSprite);
        }

        // Get or add turn system component
        turnSystem = GetComponent<BattleTurnSystem>();
        if (turnSystem == null)
        {
            turnSystem = gameObject.AddComponent<BattleTurnSystem>();
        }
        turnSystem.Initialize(battleSetup);

        // Get or add champion AI component
        championAI = GetComponent<ChampionAI>();
        if (championAI == null)
        {
            championAI = gameObject.AddComponent<ChampionAI>();
        }
        championAI.Initialize(battleSetup);

        // Get or add monster controller component
        monsterController = GetComponent<MonsterController>();
        if (monsterController == null)
        {
            monsterController = gameObject.AddComponent<MonsterController>();
        }
        monsterController.Initialize(battleSetup, monsterActionPanel);

        // Subscribe to turn system events
        turnSystem.OnTurnStart += HandleTurnStart;
        turnSystem.OnBattleEnd += EndBattle;
    }

    /// <summary>
    /// Sets the defense shield sprite for all entities
    /// </summary>
    public void SetDefenseShieldSprite(Sprite sprite)
    {
        if (battleSetup != null)
        {
            battleSetup.SetDefenseShieldSprite(sprite);
        }
    }

    /// <summary>
    /// Starts a new battle
    /// </summary>
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

        // Set battle state
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

        // Start battle setup sequence
        StartCoroutine(StartBattleSequence());
    }

    /// <summary>
    /// Battle start sequence
    /// </summary>
    private IEnumerator StartBattleSequence()
    {
        // Spawn battle entities
        yield return battleSetup.StartCoroutine(battleSetup.SpawnBattleEntities(currentChampion, currentMonsters));

        // Initialize and show monster status panel
        if (monsterStatusPanel != null)
        {
            monsterStatusPanel.Initialize(battleSetup.MonsterEntities);
        }

        // Initialize turn order
        turnSystem.InitializeTurnOrder();

        // Start first turn
        turnSystem.StartTurn();
    }

    /// <summary>
    /// Handles turn start event from turn system
    /// </summary>
    private void HandleTurnStart(BattleEntity entity)
    {
        if (entity == null || !entity.IsAlive)
        {
            turnSystem.NextTurn();
            return;
        }

        // Delegate to appropriate controller based on entity type
        if (entity.IsChampion)
        {
            StartCoroutine(ExecuteChampionTurn(entity));
        }
        else
        {
            StartCoroutine(ExecuteMonsterTurn(entity));
        }
    }

    /// <summary>
    /// Executes champion turn using AI
    /// </summary>
    private IEnumerator ExecuteChampionTurn(BattleEntity champion)
    {
        yield return championAI.StartCoroutine(championAI.ExecuteTurn(champion));
        turnSystem.NextTurn();
    }

    /// <summary>
    /// Executes monster turn (player controlled or AI)
    /// </summary>
    private IEnumerator ExecuteMonsterTurn(BattleEntity monster)
    {
        yield return monsterController.StartCoroutine(monsterController.ExecuteTurn(monster));
        turnSystem.NextTurn();
    }

    /// <summary>
    /// Updates monster status display for HP changes
    /// </summary>
    public void UpdateMonsterHP(BattleEntity monster)
    {
        if (monsterStatusPanel != null && monster != null && monster.IsMonster)
        {
            monsterStatusPanel.UpdateMonsterHP(monster);
        }
    }

    /// <summary>
    /// Updates monster status display for MP changes
    /// </summary>
    public void UpdateMonsterMP(BattleEntity monster)
    {
        if (monsterStatusPanel != null && monster != null && monster.IsMonster)
        {
            monsterStatusPanel.UpdateMonsterMP(monster);
        }
    }

    /// <summary>
    /// Ends the battle
    /// </summary>
    public void EndBattle(bool championWon)
    {
        if (!isBattleActive)
        {
            Debug.LogWarning("No battle to end!");
            return;
        }

        Debug.Log($"Battle ended! Champion won: {championWon}");

        // Sync champion stats back to original champion object
        if (currentChampion != null && battleSetup.ChampionEntity != null)
        {
            currentChampion.UpdateStatsFromBattle(
                battleSetup.ChampionEntity.CurrentHealth,
                battleSetup.ChampionEntity.CurrentMP
            );
        }

        // Clear and hide monster status panel
        if (monsterStatusPanel != null)
        {
            monsterStatusPanel.ClearStatusDisplays();
        }

        // Handle monsters in room based on battle result (BEFORE clearing entities!)
        if (currentRoom != null && battleSetup.MonsterEntities != null)
        {
            if (championWon)
            {
                // Champion won - remove all monsters
                currentRoom.RemoveAllMonsters();
            }
            else
            {
                // Champion lost - remove only dead monsters, keep alive ones
                RemoveDeadMonstersFromRoom(currentRoom, battleSetup.MonsterEntities);
            }
        }

        // Hide battle UI
        if (BattleUI.Instance != null)
        {
            BattleUI.Instance.HideBattleBackground();
        }

        // Clear battle entities (AFTER using the monster data)
        battleSetup.ClearBattleEntities();

        // Reset battle state
        isBattleActive = false;
        currentChampion = null;
        currentMonsters = null;
        currentRoom = null;

        // Reset turn system
        turnSystem.Reset();

        // TODO: Resume exploration
    }

    /// <summary>
    /// Removes dead monsters from the room, keeping alive ones
    /// </summary>
    private void RemoveDeadMonstersFromRoom(Room room, List<BattleEntity> monsterEntities)
    {
        if (room == null || monsterEntities == null) return;

        Debug.Log($"=== Checking monsters for removal ===");
        Debug.Log($"Total monster entities: {monsterEntities.Count}");

        // Create a list to track which monsters died (by index)
        List<int> deadMonsterIndices = new List<int>();

        // Check each monster entity to see if it's dead
        for (int i = 0; i < monsterEntities.Count; i++)
        {
            if (monsterEntities[i] != null)
            {
                bool isAlive = monsterEntities[i].IsAlive;
                int currentHP = monsterEntities[i].CurrentHealth;
                string name = monsterEntities[i].EntityName;

                Debug.Log($"Monster {i}: {name} - HP: {currentHP}, IsAlive: {isAlive}");

                if (!isAlive)
                {
                    deadMonsterIndices.Add(i);
                    Debug.Log($"  → Marking for removal");
                }
            }
        }

        Debug.Log($"Found {deadMonsterIndices.Count} dead monsters to remove");

        // Remove dead monsters from room (in reverse order to maintain indices)
        for (int i = deadMonsterIndices.Count - 1; i >= 0; i--)
        {
            int index = deadMonsterIndices[i];
            room.RemoveMonster(index);
            Debug.Log($"Removed dead monster at index {index} from room");
        }

        Debug.Log($"Removal complete. {monsterEntities.Count - deadMonsterIndices.Count} monsters remain alive.");
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (turnSystem != null)
        {
            turnSystem.OnTurnStart -= HandleTurnStart;
            turnSystem.OnBattleEnd -= EndBattle;
        }
    }
}
