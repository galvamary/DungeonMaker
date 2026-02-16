using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DefeatUI : MonoBehaviour
{
    public static DefeatUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private Image championImage;

    [Header("Fade Settings")]
    [SerializeField] private Image whiteFadeImage;
    [SerializeField] private float fadeDuration = 1f;

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

        // Initialize white fade image (transparent and inactive)
        if (whiteFadeImage != null)
        {
            Color color = whiteFadeImage.color;
            color.a = 0f;
            whiteFadeImage.color = color;
            whiteFadeImage.gameObject.SetActive(false);
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
        StartCoroutine(ReturnToPreparationWithFade());
    }

    private IEnumerator ReturnToPreparationWithFade()
    {
        HideDefeat();

        // Check if reputation will drop below 1 (game over condition)
        // Do this BEFORE restoring state to check against current reputation
        bool willCauseGameOver = false;
        if (GameManager.Instance != null)
        {
            int reputationAfterPenalty = GameManager.Instance.CurrentReputation - 1;
            if (reputationAfterPenalty < 1)
            {
                willCauseGameOver = true;
                Debug.Log("Defeat will cause game over! Reputation would drop below 1.");
            }
        }

        // If game over, trigger game over without restoring state
        if (willCauseGameOver)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
            yield break; // Don't restore state or return to preparation
        }

        // Fade out to white
        yield return StartCoroutine(FadeOutToWhite());

        // Restore game state to the point before exploration started
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestoreGameState();
        }

        // Apply defeat penalties: -1 reputation, -150 gold (AFTER restoration)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.DecreaseReputation(1);
            GameManager.Instance.RemoveGold(150);
            Debug.Log("Defeat penalty applied: -1 Reputation, -150 Gold");
        }

        // Return to preparation phase (while screen is still white)
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.ReturnToPreparation();
        }

        // Fade in from white
        yield return StartCoroutine(FadeInFromWhite());
    }

    private IEnumerator FadeOutToWhite()
    {
        if (whiteFadeImage == null) yield break;

        whiteFadeImage.gameObject.SetActive(true);

        float elapsedTime = 0f;
        Color color = whiteFadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            color.a = Mathf.SmoothStep(0f, 1f, t);
            whiteFadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        whiteFadeImage.color = color;
    }

    private IEnumerator FadeInFromWhite()
    {
        if (whiteFadeImage == null) yield break;

        float elapsedTime = 0f;
        Color color = whiteFadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            color.a = Mathf.SmoothStep(1f, 0f, t);
            whiteFadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        whiteFadeImage.color = color;

        whiteFadeImage.gameObject.SetActive(false);
    }
}
