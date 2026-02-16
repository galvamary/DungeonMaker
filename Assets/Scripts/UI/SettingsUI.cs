using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    public static SettingsUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Volume Sliders")]
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Volume Labels")]
    [SerializeField] private TextMeshProUGUI bgmVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;

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

    private void Start()
    {
        // Hide settings panel at start
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // Load saved volume settings
        LoadVolumeSettings();

        // Setup slider listeners
        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    private void Update()
    {
        // Toggle settings panel with ESC key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettings();
        }
    }

    public void ToggleSettings()
    {
        if (settingsPanel != null)
        {
            bool isActive = settingsPanel.activeSelf;
            settingsPanel.SetActive(!isActive);

            // Pause/unpause game when settings open/close
            Time.timeScale = !isActive ? 0f : 1f;
        }
    }

    private void OnBGMVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(value);
        }

        // Update label
        if (bgmVolumeText != null)
        {
            bgmVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }

        // Save setting
        PlayerPrefs.SetFloat("BGMVolume", value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }

        // Update label
        if (sfxVolumeText != null)
        {
            sfxVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }

        // Save setting
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    private void LoadVolumeSettings()
    {
        // Load BGM volume
        float bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.value = bgmVolume;
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(bgmVolume);
        }
        if (bgmVolumeText != null)
        {
            bgmVolumeText.text = $"{Mathf.RoundToInt(bgmVolume * 100)}%";
        }

        // Load SFX volume
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfxVolume;
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(sfxVolume);
        }
        if (sfxVolumeText != null)
        {
            sfxVolumeText.text = $"{Mathf.RoundToInt(sfxVolume * 100)}%";
        }
    }

    public void OnResetButtonClicked()
    {
        // Close settings panel
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // Resume game time
        Time.timeScale = 1f;

        // Reset game to initial state
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    public void OnQuitButtonClicked()
    {
        // Save current game state
        SaveGameState();

        // Save PlayerPrefs
        PlayerPrefs.Save();

        // Quit application
        Debug.Log("Quitting game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void SaveGameState()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("Cannot save: GameManager not found");
            return;
        }

        // Save basic game state
        PlayerPrefs.SetInt("SavedGold", GameManager.Instance.CurrentGold);
        PlayerPrefs.SetInt("SavedReputation", GameManager.Instance.CurrentReputation);

        // Save room data
        if (RoomManager.Instance != null)
        {
            SaveRoomData();
        }

        // Save monster inventory
        if (ShopManager.Instance != null)
        {
            SaveInventoryData();
        }

        PlayerPrefs.SetInt("HasSaveData", 1);
        Debug.Log($"Game state saved to PlayerPrefs: Gold={GameManager.Instance.CurrentGold}, Reputation={GameManager.Instance.CurrentReputation}");
    }

    private void SaveRoomData()
    {
        if (RoomManager.Instance == null) return;

        // Create a temporary SaveState to use existing save methods
        SaveState tempState = new SaveState();
        RoomManager.Instance.SaveRoomState(tempState);

        // Convert room data to JSON
        RoomDataList roomDataList = new RoomDataList();
        foreach (var roomState in tempState.savedRooms)
        {
            RoomDataSerialized roomData = new RoomDataSerialized();
            roomData.posX = roomState.gridPosition.x;
            roomData.posY = roomState.gridPosition.y;
            roomData.roomType = (int)roomState.roomType;

            // Save monster names in the room
            roomData.monsterNames = new List<string>();
            if (roomState.placedMonsters != null)
            {
                foreach (var monster in roomState.placedMonsters)
                {
                    if (monster != null)
                    {
                        roomData.monsterNames.Add(monster.name);
                    }
                }
            }

            roomDataList.rooms.Add(roomData);
        }

        string roomJson = JsonUtility.ToJson(roomDataList);
        PlayerPrefs.SetString("SavedRooms", roomJson);
        Debug.Log($"Saved {roomDataList.rooms.Count} rooms to PlayerPrefs");
    }

    private void SaveInventoryData()
    {
        if (ShopManager.Instance == null) return;

        // Create a temporary SaveState to use existing save methods
        SaveState tempState = new SaveState();
        ShopManager.Instance.SaveInventoryState(tempState);

        // Convert inventory to JSON (list of monster names)
        MonsterInventoryList inventoryList = new MonsterInventoryList();
        inventoryList.monsterNames = new List<string>();

        foreach (var monster in tempState.savedInventory)
        {
            if (monster != null)
            {
                inventoryList.monsterNames.Add(monster.name);
            }
        }

        string inventoryJson = JsonUtility.ToJson(inventoryList);
        PlayerPrefs.SetString("SavedInventory", inventoryJson);
        Debug.Log($"Saved {inventoryList.monsterNames.Count} monsters to PlayerPrefs");
    }

    public static void LoadGameState()
    {
        if (PlayerPrefs.GetInt("HasSaveData", 0) == 0)
        {
            Debug.Log("No save data found");
            return;
        }

        Debug.Log("========== LOADING SAVED GAME ==========");

        // Load basic game state
        if (GameManager.Instance != null)
        {
            int savedGold = PlayerPrefs.GetInt("SavedGold", 200);
            int savedReputation = PlayerPrefs.GetInt("SavedReputation", 1);

            // Use reflection to set private fields
            var goldField = typeof(GameManager).GetField("currentGold", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var reputationField = typeof(GameManager).GetField("currentReputation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (goldField != null && reputationField != null)
            {
                goldField.SetValue(GameManager.Instance, savedGold);
                reputationField.SetValue(GameManager.Instance, savedReputation);

                // Trigger UI updates
                var onGoldChanged = typeof(GameManager).GetField("OnGoldChanged", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                var onReputationChanged = typeof(GameManager).GetField("OnReputationChanged", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                if (onGoldChanged != null)
                {
                    var goldDelegate = onGoldChanged.GetValue(GameManager.Instance) as GameManager.GoldChangedDelegate;
                    goldDelegate?.Invoke(savedGold);
                }

                if (onReputationChanged != null)
                {
                    var reputationDelegate = onReputationChanged.GetValue(GameManager.Instance) as GameManager.ReputationChangedDelegate;
                    reputationDelegate?.Invoke(savedReputation);
                }

                Debug.Log($"Loaded GameManager state: Gold={savedGold}, Reputation={savedReputation}");
            }
        }

        // Load room data
        LoadRoomData();

        // Load inventory data
        LoadInventoryData();

        Debug.Log("========== LOAD COMPLETE ==========");
    }

    private static void LoadRoomData()
    {
        string roomJson = PlayerPrefs.GetString("SavedRooms", "");
        if (string.IsNullOrEmpty(roomJson) || RoomManager.Instance == null)
        {
            Debug.Log("No room data to load");
            return;
        }

        try
        {
            RoomDataList roomDataList = JsonUtility.FromJson<RoomDataList>(roomJson);
            if (roomDataList == null || roomDataList.rooms == null)
            {
                Debug.LogWarning("Failed to parse room data");
                return;
            }

            // Clear existing rooms first (except entrance)
            RoomManager.Instance.ResetAllRooms();

            // Restore each room
            foreach (var roomData in roomDataList.rooms)
            {
                Vector2Int gridPos = new Vector2Int(roomData.posX, roomData.posY);
                RoomType roomType = (RoomType)roomData.roomType;

                // Place the room (without deducting cost)
                RoomManager.Instance.PlaceRoom(gridPos, roomType, false);

                // Get the placed room
                Room room = RoomManager.Instance.GetRoomAt(gridPos);
                if (room != null && roomData.monsterNames != null)
                {
                    // Restore monsters in the room
                    foreach (string monsterName in roomData.monsterNames)
                    {
                        MonsterData monsterData = Resources.Load<MonsterData>($"Monsters/{monsterName}");
                        if (monsterData != null && room != null)
                        {
                            room.AddMonster(monsterData);
                        }
                    }
                }
            }

            Debug.Log($"Loaded {roomDataList.rooms.Count} rooms from PlayerPrefs");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading room data: {e.Message}");
        }
    }

    private static void LoadInventoryData()
    {
        string inventoryJson = PlayerPrefs.GetString("SavedInventory", "");
        if (string.IsNullOrEmpty(inventoryJson) || ShopManager.Instance == null)
        {
            Debug.Log("No inventory data to load");
            return;
        }

        try
        {
            MonsterInventoryList inventoryList = JsonUtility.FromJson<MonsterInventoryList>(inventoryJson);
            if (inventoryList == null || inventoryList.monsterNames == null)
            {
                Debug.LogWarning("Failed to parse inventory data");
                return;
            }

            // Clear existing inventory
            ShopManager.Instance.ResetInventory();

            // Restore inventory
            foreach (string monsterName in inventoryList.monsterNames)
            {
                MonsterData monsterData = Resources.Load<MonsterData>($"Monsters/{monsterName}");
                if (monsterData != null)
                {
                    // Add to inventory (need to access private method or use public interface)
                    // We'll need to add a public method to ShopManager for this
                    ShopManager.Instance.AddMonsterToInventoryFromLoad(monsterData);
                }
            }

            Debug.Log($"Loaded {inventoryList.monsterNames.Count} monsters from PlayerPrefs");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading inventory data: {e.Message}");
        }
    }

    // Helper classes for JSON serialization
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
        public List<RoomDataSerialized> rooms = new List<RoomDataSerialized>();
    }

    [Serializable]
    private class MonsterInventoryList
    {
        public List<string> monsterNames;
    }

    private void OnDestroy()
    {
        // Cleanup slider listeners
        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        }

        // Ensure time scale is restored
        Time.timeScale = 1f;
    }
}
