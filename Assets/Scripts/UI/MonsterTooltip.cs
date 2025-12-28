using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterTooltip : MonoBehaviour
{
    public static MonsterTooltip Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI monsterNameText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI mpText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    private RectTransform tooltipRect;
    private Canvas canvas;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        // 툴팁이 마우스 이벤트를 차단하지 않도록 설정
        CanvasGroup canvasGroup = tooltipPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = tooltipPanel.AddComponent<CanvasGroup>();
        }
        canvasGroup.blocksRaycasts = false;  // 레이캐스트 차단 비활성화

        HideTooltip();
    }

    public void ShowTooltip(MonsterData monster, Vector2 mousePosition)
    {
        if (monster == null || tooltipPanel == null) return;

        // 몬스터 정보 표시
        if (monsterNameText != null)
            monsterNameText.text = monster.monsterName;

        if (healthText != null)
            healthText.text = $"HP: {monster.maxHealth}";

        if (mpText != null)
            mpText.text = $"MP: {monster.maxMP}";

        if (attackText != null)
            attackText.text = $"attack: {monster.attack}";

        if (defenseText != null)
            defenseText.text = $"defense: {monster.defense}";

        if (speedText != null)
            speedText.text = $"speed: {monster.speed}";

        if (descriptionText != null)
            descriptionText.text = monster.description;

        // 툴팁 활성화
        tooltipPanel.SetActive(true);

        // 다음 프레임에서 위치 조정 (레이아웃이 업데이트된 후)
        Canvas.ForceUpdateCanvases();
        UpdateTooltipPosition(mousePosition);
    }

    public void UpdateTooltipPosition(Vector2 mousePosition)
    {
        if (tooltipPanel == null || !tooltipPanel.activeSelf) return;

        // 마우스 커서를 툴팁의 오른쪽 아래 꼭짓점에 위치시킴
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            mousePosition,
            canvas.worldCamera,
            out localPoint
        );

        // 툴팁의 오른쪽 아래 꼭짓점이 마우스 위치에 오도록 조정
        Vector2 tooltipPosition = localPoint;
        tooltipPosition.x += tooltipRect.rect.width / 2 + 10;
        tooltipPosition.y -= tooltipRect.rect.height / 2;

        // 화면 밖으로 나가지 않도록 조정
        RectTransform canvasRect = canvas.transform as RectTransform;

        // 왼쪽 경계 체크 (툴팁의 왼쪽 끝이 화면을 벗어나지 않도록)
        float tooltipLeftEdge = tooltipPosition.x + tooltipRect.rect.width / 2;
        float rightBound = canvasRect.rect.width / 2;
        if (tooltipLeftEdge > rightBound)
        {
            tooltipPosition.x = rightBound - tooltipRect.rect.width / 2;
        }

        // 위쪽 경계 체크 (툴팁의 위쪽 끝이 화면을 벗어나지 않도록)
        float tooltipTopEdge = tooltipPosition.y - tooltipRect.rect.height / 2;
        float topBound = -canvasRect.rect.height / 2;
        if (tooltipTopEdge < topBound)
        {
            tooltipPosition.y = topBound + tooltipRect.rect.height / 2;
        }

        tooltipRect.anchoredPosition = tooltipPosition;
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
}
