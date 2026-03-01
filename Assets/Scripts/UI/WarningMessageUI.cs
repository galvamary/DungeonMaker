using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WarningMessageUI : MonoBehaviour
{
    public static WarningMessageUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private TextMeshProUGUI warningText;

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
        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
        }
    }

    public void ShowWarning(string message)
    {
        if (warningPanel == null || warningText == null)
        {
            Debug.LogWarning("Warning UI not properly set up!");
            return;
        }

        warningText.text = message;
        warningPanel.SetActive(true);
    }

    public void HideWarning()
    {
        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
        }
    }
}
