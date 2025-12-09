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
        turnSystem.OnBattleEnd += HandleBattleEnd;
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
    /// Handles battle end event from turn system
    /// </summary>
    private void HandleBattleEnd(bool championWon)
    {
        EndBattle(championWon);
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

        // Clear battle entities
        battleSetup.ClearBattleEntities();

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

        // Reset battle state
        isBattleActive = false;
        currentChampion = null;
        currentMonsters = null;
        currentRoom = null;

        // Reset turn system
        turnSystem.Reset();

        // TODO: Resume exploration
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (turnSystem != null)
        {
            turnSystem.OnTurnStart -= HandleTurnStart;
            turnSystem.OnBattleEnd -= HandleBattleEnd;
        }
    }
}
