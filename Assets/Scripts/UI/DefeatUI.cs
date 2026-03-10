using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

        // 패배 시 항상 게임 오버
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }
    }
}
