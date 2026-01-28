using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Image fadeImage;
    [SerializeField] private GameObject restartButton;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 2f;

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

        // Activate panel first (but transparent)
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
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
    }
}
