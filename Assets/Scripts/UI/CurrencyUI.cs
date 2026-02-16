using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CurrencyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private string goldFormat = "Gold: {0}";
    
    private void Start()
    {
        if (GameManager.Instance != null)
        {
            UpdateGoldDisplay(GameManager.Instance.CurrentGold);
            GameManager.Instance.OnGoldChanged += UpdateGoldDisplay;
        }
    }
    
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoldChanged -= UpdateGoldDisplay;
        }
    }
    
    private void UpdateGoldDisplay(int gold)
    {
        if (goldText != null)
        {
            goldText.text = string.Format(goldFormat, gold);

            // Change color to red (#D60000) if gold is negative, otherwise white
            if (gold < 0)
            {
                ColorUtility.TryParseHtmlString("#d60000ff", out Color redColor);
                goldText.color = redColor;
            }
            else
            {
                goldText.color = Color.white;
            }
        }
    }
}