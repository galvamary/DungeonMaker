using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int currentGold = 1000;
    [SerializeField] private int currentReputation = 1;

    // Save state for exploration
    private SaveState currentSaveState;

    public int CurrentGold => currentGold;
    public int CurrentReputation => currentReputation;

    public delegate void GoldChangedDelegate(int newGold);
    public event GoldChangedDelegate OnGoldChanged;

    public delegate void ReputationChangedDelegate(int newReputation);
    public event ReputationChangedDelegate OnReputationChanged;

    public delegate void GameOverDelegate();
    public event GameOverDelegate OnGameOver;
    
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

    private void Start()
    {
        // Try to load saved game state from PlayerPrefs
        SettingsUI.LoadGameState();
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0 || currentGold < amount)
        {
            return false;
        }
        
        currentGold -= amount;
        OnGoldChanged?.Invoke(currentGold);
        return true;
    }
    
    public void AddGold(int amount)
    {
        if (amount > 0)
        {
            currentGold += amount;
            OnGoldChanged?.Invoke(currentGold);
        }
    }
    
    public bool CanAfford(int amount)
    {
        return currentGold >= amount;
    }

    /// <summary>
    /// Increases reputation when champion is defeated
    /// </summary>
    public void IncreaseReputation(int amount = 1)
    {
        if (amount > 0)
        {
            currentReputation += amount;
            OnReputationChanged?.Invoke(currentReputation);
            Debug.Log($"Reputation increased by {amount}. Current reputation: {currentReputation}");
        }
    }

    /// <summary>
    /// Decreases reputation when champion escapes successfully
    /// </summary>
    public void DecreaseReputation(int amount = 1)
    {
        if (amount > 0)
        {
            currentReputation -= amount;
            OnReputationChanged?.Invoke(currentReputation);
            Debug.Log($"Reputation decreased by {amount}. Current reputation: {currentReputation}");
        }
    }

    /// <summary>
    /// Removes gold without checking if player can afford it (can go negative)
    /// Used for defeat penalties
    /// </summary>
    public void RemoveGold(int amount)
    {
        if (amount > 0)
        {
            currentGold -= amount;
            OnGoldChanged?.Invoke(currentGold);
            Debug.Log($"Gold removed: -{amount}. Current gold: {currentGold}");
        }
    }

    /// <summary>
    /// Triggers the game over event
    /// </summary>
    public void TriggerGameOver()
    {
        Debug.Log("Game Over triggered!");
        OnGameOver?.Invoke();
    }

    /// <summary>
    /// Resets all game state to initial values
    /// </summary>
    public void ResetGame()
    {
        currentGold = 200;
        currentReputation = 1;

        OnGoldChanged?.Invoke(currentGold);
        OnReputationChanged?.Invoke(currentReputation);

        Debug.Log("Game reset! Gold: 200, Reputation: 1");
    }

    /// <summary>
    /// Restarts the game by resetting all game states without reloading the scene
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("========== RESTARTING GAME ==========");

        // 1. Reset GameManager state (gold, reputation)
        ResetGame();

        // 2. Reset all rooms (destroy all except entrance)
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.ResetAllRooms();
        }

        // 3. Reset monster inventory
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.ResetInventory();
        }

        // 4. Return to preparation phase
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.ReturnToPreparation();
        }

        // 5. Clear any remaining champions
        if (ChampionSpawner.Instance != null)
        {
            ChampionSpawner.Instance.ClearCurrentChampion();
        }

        Debug.Log("========== GAME RESTART COMPLETE ==========");
    }

    /// <summary>
    /// Saves the current game state (called at exploration start)
    /// </summary>
    public void SaveGameState()
    {
        if (currentSaveState == null)
        {
            currentSaveState = new SaveState();
        }
        else
        {
            currentSaveState.Clear();
        }

        // Save GameManager state
        currentSaveState.savedGold = currentGold;
        currentSaveState.savedReputation = currentReputation;

        // Save RoomManager state
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.SaveRoomState(currentSaveState);
        }

        // Save ShopManager state (inventory)
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.SaveInventoryState(currentSaveState);
        }

        Debug.Log($"Game state saved! Gold: {currentGold}, Reputation: {currentReputation}, Rooms: {currentSaveState.savedRooms.Count}");
    }

    /// <summary>
    /// Restores the saved game state (called on defeat)
    /// </summary>
    public void RestoreGameState()
    {
        if (currentSaveState == null)
        {
            Debug.LogWarning("No saved state to restore!");
            return;
        }

        Debug.Log("========== RESTORING GAME STATE ==========");

        // Restore GameManager state
        currentGold = currentSaveState.savedGold;
        currentReputation = currentSaveState.savedReputation;
        OnGoldChanged?.Invoke(currentGold);
        OnReputationChanged?.Invoke(currentReputation);

        // Restore RoomManager state
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RestoreRoomState(currentSaveState);
        }

        // Restore ShopManager state (inventory)
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.RestoreInventoryState(currentSaveState);
        }

        Debug.Log($"Game state restored! Gold: {currentGold}, Reputation: {currentReputation}");
        Debug.Log("========== RESTORE COMPLETE ==========");
    }
}