using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeEffect : MonoBehaviour
{
    public static FadeEffect Instance { get; private set; }

    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.5f;

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
        // Make sure fade image starts transparent and disabled
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
            fadeImage.gameObject.SetActive(false);
        }
    }

    public void EnableFadeImage()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
        }
    }

    public void DisableFadeImage()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(false);
        }
    }

    public IEnumerator FadeOutAndIn()
    {
        // Fade out (darken)
        yield return StartCoroutine(FadeOut());

        // Fade in (brighten)
        yield return StartCoroutine(FadeIn());
    }

    public IEnumerator FadeOut()
    {
        if (fadeImage == null) yield break;

        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            // Use SmoothStep for smoother transition
            color.a = Mathf.Lerp(0f, 1f, t);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;
    }

    public IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;

        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            // Use SmoothStep for smoother transition
            color.a = Mathf.Lerp(1f, 0f, t);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;
    }
}
