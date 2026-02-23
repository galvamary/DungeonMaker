using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CurrencyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private string goldFormat = "{0}G";

    [Header("플로팅 텍스트 설정")]
    [SerializeField] private TextMeshProUGUI floatingGoldText;
    [SerializeField] private float floatDuration = 0.8f;
    [SerializeField] private float floatDistance = 50f;

    private int previousGold;
    private bool isInitialized = false;
    private Vector2 floatingStartPos;
    private Coroutine floatingCoroutine;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            previousGold = GameManager.Instance.CurrentGold;
            isInitialized = true;
            UpdateGoldDisplay(GameManager.Instance.CurrentGold);
            GameManager.Instance.OnGoldChanged += UpdateGoldDisplay;
        }

        // 초기 위치 저장 및 숨김
        if (floatingGoldText != null)
        {
            floatingStartPos = floatingGoldText.rectTransform.anchoredPosition;
            floatingGoldText.gameObject.SetActive(false);
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
            // 골드 감소 감지 → 플로팅 텍스트 표시
            if (isInitialized)
            {
                int delta = gold - previousGold;
                if (delta < 0)
                {
                    ShowFloatingText(delta);
                }
            }
            previousGold = gold;

            goldText.text = string.Format(goldFormat, gold);

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

    private void ShowFloatingText(int delta)
    {
        if (floatingGoldText == null) return;

        // 이전 애니메이션 중단
        if (floatingCoroutine != null)
        {
            StopCoroutine(floatingCoroutine);
        }

        // 텍스트 설정 및 초기 상태 복원
        floatingGoldText.text = $"{delta}G";
        floatingGoldText.rectTransform.anchoredPosition = floatingStartPos;
        Color c = floatingGoldText.color;
        c.a = 1f;
        floatingGoldText.color = c;
        floatingGoldText.gameObject.SetActive(true);

        floatingCoroutine = StartCoroutine(FloatAndFadeCoroutine());
    }

    private IEnumerator FloatAndFadeCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < floatDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / floatDuration;

            // 위로 이동
            floatingGoldText.rectTransform.anchoredPosition = floatingStartPos + new Vector2(0f, floatDistance * t);

            // 후반부에 페이드 아웃 (40% 지점부터 시작)
            if (t > 0.4f)
            {
                Color c = floatingGoldText.color;
                c.a = Mathf.Lerp(1f, 0f, (t - 0.4f) / 0.6f);
                floatingGoldText.color = c;
            }

            yield return null;
        }

        floatingGoldText.gameObject.SetActive(false);
        floatingCoroutine = null;
    }
}
