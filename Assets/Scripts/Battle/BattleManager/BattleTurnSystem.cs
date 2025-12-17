using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages turn order, round progression, and battle flow
/// Handles turn-based combat system including speed-based ordering and status updates
/// </summary>
public class BattleTurnSystem : MonoBehaviour
{
    [Header("Turn State")]
    private List<BattleEntity> turnOrder = new List<BattleEntity>();
    private int currentTurnIndex = 0;
    private int currentRound = 0;

    // Events for turn changes
    public delegate void TurnChangeHandler(BattleEntity entity);
    public event TurnChangeHandler OnTurnStart;
    public event TurnChangeHandler OnTurnEnd;

    public delegate void RoundChangeHandler(int roundNumber);
    public event RoundChangeHandler OnRoundStart;

    public delegate void BattleEndHandler(bool championWon);
    public event BattleEndHandler OnBattleEnd;

    // Public accessors
    public BattleEntity CurrentTurnEntity => turnOrder.Count > 0 && currentTurnIndex < turnOrder.Count ? turnOrder[currentTurnIndex] : null;
    public int CurrentRound => currentRound;
    public List<BattleEntity> TurnOrder => new List<BattleEntity>(turnOrder); // Return copy

    private BattleSetup battleSetup;

    public void Initialize(BattleSetup setup)
    {
        battleSetup = setup;
    }

    /// <summary>
    /// Initializes turn order based on entity speed
    /// </summary>
    public void InitializeTurnOrder()
    {
        turnOrder.Clear();
        currentTurnIndex = 0;
        currentRound = 1;

        // Get all alive entities from battle setup
        if (battleSetup == null)
        {
            Debug.LogError("BattleSetup not initialized!");
            return;
        }

        List<BattleEntity> allEntities = battleSetup.GetAllAliveEntities();
        turnOrder.AddRange(allEntities);

        // Sort by speed (descending - highest speed goes first)
        turnOrder.Sort((a, b) => b.Speed.CompareTo(a.Speed));

        Debug.Log("Turn order initialized:");
        for (int i = 0; i < turnOrder.Count; i++)
        {
            Debug.Log($"{i + 1}. {turnOrder[i].EntityName} (Speed: {turnOrder[i].Speed})");
        }
    }

    /// <summary>
    /// Starts the current turn
    /// </summary>
    public void StartTurn()
    {
        BattleEntity currentEntity = CurrentTurnEntity;
        if (currentEntity == null || !currentEntity.IsAlive)
        {
            NextTurn();
            return;
        }

        Debug.Log($"=== {currentEntity.EntityName}'s turn (Speed: {currentEntity.Speed}) ===");

        // Update status effects for current entity at turn start
        UpdateEntityStatusEffects(currentEntity);

        // Invoke turn start event
        OnTurnStart?.Invoke(currentEntity);
    }

    /// <summary>
    /// Advances to the next turn
    /// </summary>
    public void NextTurn()
    {
        // Invoke turn end event for current entity
        BattleEntity previousEntity = CurrentTurnEntity;
        if (previousEntity != null)
        {
            OnTurnEnd?.Invoke(previousEntity);
        }

        currentTurnIndex++;

        // If we've gone through all entities, start a new round
        if (currentTurnIndex >= turnOrder.Count)
        {
            StartNewRound();
            return;
        }

        // Check battle end conditions
        if (CheckBattleEnd())
        {
            return;
        }

        // Skip dead entities
        BattleEntity currentEntity = CurrentTurnEntity;
        if (currentEntity != null && !currentEntity.IsAlive)
        {
            NextTurn();
            return;
        }

        StartTurn();
    }

    /// <summary>
    /// Starts a new round
    /// </summary>
    private void StartNewRound()
    {
        currentTurnIndex = 0;
        currentRound++;

        Debug.Log($"--- Round {currentRound} ---");

        // Invoke round start event
        OnRoundStart?.Invoke(currentRound);

        // Check battle end conditions
        if (CheckBattleEnd())
        {
            return;
        }

        StartTurn();
    }

    /// <summary>
    /// Updates status effects for a single entity (called at their turn start)
    /// </summary>
    private void UpdateEntityStatusEffects(BattleEntity entity)
    {
        if (entity != null && entity.IsAlive)
        {
            entity.UpdateDefenseStatus();
            // Add other status effect updates here in the future
        }
    }

    /// <summary>
    /// Checks if the battle should end
    /// </summary>
    private bool CheckBattleEnd()
    {
        if (battleSetup == null)
        {
            return false;
        }

        bool championAlive = battleSetup.ChampionEntity != null && battleSetup.ChampionEntity.IsAlive;
        bool anyMonsterAlive = battleSetup.GetAliveMonsters().Count > 0;

        if (!championAlive)
        {
            Debug.Log("Champion defeated! Monsters win!");
            OnBattleEnd?.Invoke(false);
            return true;
        }

        if (!anyMonsterAlive)
        {
            Debug.Log("All monsters defeated! Champion wins!");
            OnBattleEnd?.Invoke(true);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Forces battle to end (for external calls)
    /// </summary>
    public void ForceEndBattle(bool championWon)
    {
        OnBattleEnd?.Invoke(championWon);
    }

    /// <summary>
    /// Resets the turn system
    /// </summary>
    public void Reset()
    {
        turnOrder.Clear();
        currentTurnIndex = 0;
        currentRound = 0;
        // Don't clear event subscriptions - they should persist between battles
        // OnTurnStart = null;
        // OnTurnEnd = null;
        // OnRoundStart = null;
        // OnBattleEnd = null;
    }
}
