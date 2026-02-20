using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Displays warning messages that fade out after a few seconds
/// </summary>
public class WarningMessageUI : MonoBehaviour
{
    public static WarningMessageUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private TextMeshProUGUI warningText;

    [Header("Settings")]
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeOutDuration = 1f;

    private Coroutine currentWarningCoroutine;

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

    /// <summary>
    /// Shows a warning message that fades out after a delay
    /// </summary>
    public void ShowWarning(string message)
    {
        if (warningPanel == null || warningText == null)
        {
            Debug.LogWarning("Warning UI not properly set up!");
            return;
        }

        // Stop any existing warning
        if (currentWarningCoroutine != null)
        {
            StopCoroutine(currentWarningCoroutine);
        }

        currentWarningCoroutine = StartCoroutine(ShowWarningCoroutine(message));
    }

    private IEnumerator ShowWarningCoroutine(string message)
    {
        // Set message
        warningText.text = message;

        // Show panel with full opacity
        warningPanel.SetActive(true);
        CanvasGroup canvasGroup = warningPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = warningPanel.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 1f;

        // Wait for display duration
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        // Hide panel
        canvasGroup.alpha = 0f;
        warningPanel.SetActive(false);
        currentWarningCoroutine = null;
    }
}
