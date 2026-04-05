using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Image fadeImage;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private TextMeshProUGUI reputationText;  // 최종 명성 표시

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 2f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip gameOverTextSFX;    // 게임 오버 텍스트 등장 시

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
        // Hide game over panel at start
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.SetActive(false);
        }

        // Subscribe to game over event
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver += ShowGameOver;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= ShowGameOver;
        }
    }

    /// <summary>
    /// Shows game over screen with fade in effect
    /// </summary>
    public void ShowGameOver()
    {
        StartCoroutine(ShowGameOverSequence());
    }

    private IEnumerator ShowGameOverSequence()
    {
        Debug.Log("Starting Game Over sequence...");

        // Fade in
        if (fadeImage != null)
        {
            yield return StartCoroutine(FadeIn());
        }

        // 최종 명성 텍스트 업데이트
        if (reputationText != null && GameManager.Instance != null)
        {
            reputationText.gameObject.SetActive(true);
            reputationText.text = $"Final Reputation: {GameManager.Instance.CurrentReputation}";
        }

        // 게임 오버 텍스트 패널 등장 + 사운드
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (AudioManager.Instance != null && gameOverTextSFX != null)
            {
                AudioManager.Instance.PlaySFX(gameOverTextSFX);
            }
        }

        if (restartButton != null)
        {
            restartButton.SetActive(true);
        }

        Debug.Log("Game Over screen displayed.");
    }

    private IEnumerator FadeIn()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
        }

        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        // Ensure fully opaque
        color.a = 1f;
        fadeImage.color = color;
    }

    /// <summary>
    /// Called when restart button is clicked
    /// </summary>
    public void OnRestartButtonClicked()
    {
        Debug.Log("Restart button clicked!");

        // Restart game without reloading scene
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }

        // Hide game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.SetActive(false);
        }

        if (reputationText != null)
        {
            reputationText.gameObject.SetActive(false);
        }
    }
}
