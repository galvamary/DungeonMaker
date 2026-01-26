using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReputationUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI reputationText;
    [SerializeField] private string reputationFormat = "Reputation: {0}";

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            UpdateReputationDisplay(GameManager.Instance.CurrentReputation);
            GameManager.Instance.OnReputationChanged += UpdateReputationDisplay;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnReputationChanged -= UpdateReputationDisplay;
        }
    }

    private void UpdateReputationDisplay(int reputation)
    {
        if (reputationText != null)
        {
            reputationText.text = string.Format(reputationFormat, reputation);
        }
    }
}
