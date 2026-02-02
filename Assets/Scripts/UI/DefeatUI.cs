using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class DefeatUI : MonoBehaviour
{
    public static DefeatUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private Image championImage;

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
        // Hide defeat panel at start
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(false);
        }
    }

    public void ShowDefeat(Champion champion)
    {
        championImage.sprite = champion.CurrentSprite;

        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
        }
    }

    public void HideDefeat()
    {
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(false);
        }
    }

    public void OnReturnButtonClicked()
    {
        HideDefeat();

        // Reduce reputation to 1/3 when defeated
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReduceReputationToOneThird();
        }

        // Return to preparation phase
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.ReturnToPreparation();
        }
    }
}
