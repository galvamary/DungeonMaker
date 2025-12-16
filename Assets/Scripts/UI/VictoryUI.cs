using UnityEngine;

/// <summary>
/// Manages the victory UI panel shown when the player defeats all monsters
/// </summary>
public class VictoryUI : MonoBehaviour
{
    public static VictoryUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject victoryPanel;

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
    public void ShowVictory()
    {
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

        // Return to preparation phase
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.ReturnToPreparation();
        }
    }
}
