using UnityEngine;
using System;
using System.Collections.Generic;

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
        LoadGameStateFromDisk();
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

    /// <summary>
    /// Saves the current game state to PlayerPrefs (called when quitting)
    /// </summary>
    public void SaveGameStateToDisk()
    {
        PlayerPrefs.SetInt("SavedGold", currentGold);
        PlayerPrefs.SetInt("SavedReputation", currentReputation);

        // Save room data
        if (RoomManager.Instance != null)
        {
            SaveState tempState = new SaveState();
            RoomManager.Instance.SaveRoomState(tempState);

            RoomDataList roomDataList = new RoomDataList();
            foreach (var roomState in tempState.savedRooms)
            {
                RoomDataSerialized roomData = new RoomDataSerialized();
                roomData.posX = roomState.gridPosition.x;
                roomData.posY = roomState.gridPosition.y;
                roomData.roomType = (int)roomState.roomType;
                roomData.monsterNames = new List<string>();
                if (roomState.placedMonsters != null)
                {
                    foreach (var monster in roomState.placedMonsters)
                    {
                        if (monster != null) roomData.monsterNames.Add(monster.name);
                    }
                }
                roomDataList.rooms.Add(roomData);
            }
            PlayerPrefs.SetString("SavedRooms", JsonUtility.ToJson(roomDataList));
        }

        // Save inventory data
        if (ShopManager.Instance != null)
        {
            SaveState tempState = new SaveState();
            ShopManager.Instance.SaveInventoryState(tempState);

            MonsterInventoryList inventoryList = new MonsterInventoryList();
            inventoryList.monsterNames = new List<string>();
            foreach (var monster in tempState.savedInventory)
            {
                if (monster != null) inventoryList.monsterNames.Add(monster.name);
            }
            PlayerPrefs.SetString("SavedInventory", JsonUtility.ToJson(inventoryList));
        }

        PlayerPrefs.SetInt("HasSaveData", 1);
        PlayerPrefs.Save();
        Debug.Log($"Game state saved to disk: Gold={currentGold}, Reputation={currentReputation}");
    }

    /// <summary>
    /// Loads the game state from PlayerPrefs (called on game start)
    /// </summary>
    public void LoadGameStateFromDisk()
    {
        if (PlayerPrefs.GetInt("HasSaveData", 0) == 0)
        {
            Debug.Log("No save data found");
            return;
        }

        Debug.Log("========== LOADING SAVED GAME ==========");

        // Load gold and reputation
        currentGold = PlayerPrefs.GetInt("SavedGold", 200);
        currentReputation = PlayerPrefs.GetInt("SavedReputation", 1);
        OnGoldChanged?.Invoke(currentGold);
        OnReputationChanged?.Invoke(currentReputation);

        // Load room data
        string roomJson = PlayerPrefs.GetString("SavedRooms", "");
        if (!string.IsNullOrEmpty(roomJson) && RoomManager.Instance != null)
        {
            try
            {
                RoomDataList roomDataList = JsonUtility.FromJson<RoomDataList>(roomJson);
                if (roomDataList?.rooms != null)
                {
                    RoomManager.Instance.ResetAllRooms();
                    foreach (var roomData in roomDataList.rooms)
                    {
                        Vector2Int gridPos = new Vector2Int(roomData.posX, roomData.posY);
                        RoomType roomType = (RoomType)roomData.roomType;
                        RoomManager.Instance.PlaceRoom(gridPos, roomType, false);

                        Room room = RoomManager.Instance.GetRoomAt(gridPos);
                        if (room != null && roomData.monsterNames != null)
                        {
                            foreach (string monsterName in roomData.monsterNames)
                            {
                                MonsterData monsterData = ShopManager.Instance.GetMonsterByName(monsterName);
                                if (monsterData != null) room.PlaceMonster(monsterData);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading room data: {e.Message}");
            }
        }

        // Load inventory data
        string inventoryJson = PlayerPrefs.GetString("SavedInventory", "");
        if (!string.IsNullOrEmpty(inventoryJson) && ShopManager.Instance != null)
        {
            try
            {
                MonsterInventoryList inventoryList = JsonUtility.FromJson<MonsterInventoryList>(inventoryJson);
                if (inventoryList?.monsterNames != null)
                {
                    ShopManager.Instance.ResetInventory();
                    foreach (string monsterName in inventoryList.monsterNames)
                    {
                        MonsterData monsterData = ShopManager.Instance.GetMonsterByName(monsterName);
                        if (monsterData != null)
                            ShopManager.Instance.AddMonsterToInventoryFromLoad(monsterData);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading inventory data: {e.Message}");
            }
        }

        Debug.Log($"Game state loaded from disk: Gold={currentGold}, Reputation={currentReputation}");
        Debug.Log("========== LOAD COMPLETE ==========");
    }

    // Helper classes for JSON serialization (disk save/load)
    [Serializable]
    private class RoomDataSerialized
    {
        public int posX;
        public int posY;
        public int roomType;
        public List<string> monsterNames;
    }

    [Serializable]
    private class RoomDataList
    {
        public List<RoomDataSerialized> rooms = new();
    }

    [Serializable]
    private class MonsterInventoryList
    {
        public List<string> monsterNames;
    }
}