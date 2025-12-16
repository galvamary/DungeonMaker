using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BattleUI : MonoBehaviour
{
    public static BattleUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Image battleBackgroundImage;

    [Header("Entity Position Containers")]
    [SerializeField] private RectTransform championPositionContainer;
    [SerializeField] private RectTransform[] monsterPositionContainers = new RectTransform[3];

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.5f;

    public RectTransform ChampionPositionContainer => championPositionContainer;
    public RectTransform[] MonsterPositionContainers => monsterPositionContainers;

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
        // Hide battle background at start
        if (battleBackgroundImage != null)
        {
            battleBackgroundImage.gameObject.SetActive(false);
        }
    }

    public void ShowBattleBackground()
    {
        if (battleBackgroundImage != null)
        {
            battleBackgroundImage.gameObject.SetActive(true);
            StartCoroutine(FadeInBackground());
        }
    }

    public void HideBattleBackground()
    {
        // Check if this GameObject is active before starting coroutine
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(FadeOutAndHide());
        }
        else
        {
            // If not active, just hide immediately without animation
            if (battleBackgroundImage != null)
            {
                Color color = battleBackgroundImage.color;
                color.a = 0f;
                battleBackgroundImage.color = color;
                battleBackgroundImage.gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator FadeInBackground()
    {
        if (battleBackgroundImage == null) yield break;

        float elapsedTime = 0f;
        Color color = battleBackgroundImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            color.a = Mathf.SmoothStep(0f, 1f, t);
            battleBackgroundImage.color = color;
            yield return null;
        }

        color.a = 1f;
        battleBackgroundImage.color = color;
    }

    private IEnumerator FadeOutAndHide()
    {
        if (battleBackgroundImage == null) yield break;

        float elapsedTime = 0f;
        Color color = battleBackgroundImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            color.a = Mathf.SmoothStep(1f, 0f, t);
            battleBackgroundImage.color = color;
            yield return null;
        }

        color.a = 0f;
        battleBackgroundImage.color = color;

        if (battleBackgroundImage != null)
        {
            battleBackgroundImage.gameObject.SetActive(false);
        }
    }
}
