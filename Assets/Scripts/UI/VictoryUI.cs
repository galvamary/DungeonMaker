using UnityEngine;

/// <summary>
/// Manages the victory UI panel shown when the player defeats all monsters
/// </summary>
public class VictoryUI : MonoBehaviour
{
    public static VictoryUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject victoryPanel;
    private ChampionData currentChampionData;

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
        // Hide victory panel at start
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Shows the victory panel
    /// </summary>
    public void ShowVictory(Champion champion)
    {
        currentChampionData = champion.Data;
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Hides the victory panel
    /// </summary>
    public void HideVictory()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Called when the return button is clicked
    /// </summary>
    public void OnReturnButtonClicked()
    {
        HideVictory();

        // Calculate gold reward based on treasure room count: 150 + 50n
        if (GameManager.Instance != null)
        {
            int treasureRoomCount = RoomManager.Instance != null ? RoomManager.Instance.GetTreasureRoomCount() : 1;
            int goldReward = 150 + (50 * treasureRoomCount);

            GameManager.Instance.AddGold(goldReward);
            Debug.Log($"Victory reward! Received {goldReward} gold (base 150 + {50 * treasureRoomCount} from {treasureRoomCount} treasure rooms)");
            GameManager.Instance.IncreaseReputation(1);
        }

        // Return to preparation phase
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.ReturnToPreparation();
        }
    }
}
