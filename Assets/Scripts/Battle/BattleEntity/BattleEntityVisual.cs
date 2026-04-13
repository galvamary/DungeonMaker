using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Handles visual representation and effects for battle entities
/// Manages UI sprites, defense shield indicator, name display, and visual animations
/// </summary>
public class BattleEntityVisual : MonoBehaviour
{
    [Header("Visual Components")]
    private RectTransform rectTransform;
    private Image entityImage;
    private GameObject defenseIndicator;
    private GameObject nameDisplay;
    private TextMeshProUGUI nameText;

    [Header("Defense Visual")]
    [SerializeField] private Sprite defenseShieldSprite;

    [Header("Name Display Settings")]
    [SerializeField] private Color nameColorYellow = new Color(0.8705882f, 0.8078431f, 0.345098f); // Yellow for HP <= 50%
    [SerializeField] private Color nameColorOrange = new Color(0.8117647f, 0.4196078f, 0.1686275f); // Orange for HP <= 10%
    [SerializeField] private float hpThresholdYellow = 0.5f; // 50%
    [SerializeField] private float hpThresholdOrange = 0.1f; // 10%

    private BattleEntity entity;
    private FatigueEffect fatigueEffect;

    private Coroutine floatingCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        entityImage = GetComponent<Image>();
        if (entityImage == null)
        {
            entityImage = gameObject.AddComponent<Image>();
        }
    }

    private void Update()
    {
        // Animate defense indicator with pulsing effect
        if (defenseIndicator != null && defenseIndicator.activeSelf)
        {
            float pulse = 1f + Mathf.Sin(Time.time * 3f) * 0.05f;
            defenseIndicator.transform.localScale = Vector3.one * pulse;
        }
    }

    public void Initialize(BattleEntity battleEntity)
    {
        entity = battleEntity;
    }

    /// <summary>
    /// Sets up the visual appearance of the entity
    /// </summary>
    public void SetupVisual(Sprite sprite, bool isChampion)
    {
        if (entityImage != null && sprite != null)
        {
            entityImage.sprite = sprite;
            entityImage.preserveAspect = true;
        }

        // Scale based on type
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one * (isChampion ? 2.5f : 2f);
        }

        // Create defense indicator (initially hidden)
        CreateDefenseIndicator();

        // Create name display for all entities (always visible)
        CreateNameDisplay(isChampion);
    }

    /// <summary>
    /// Creates the defense shield indicator UI
    /// </summary>
    private void CreateDefenseIndicator()
    {
        // Create a child GameObject for the defense shield icon
        defenseIndicator = new GameObject("DefenseIndicator", typeof(RectTransform));
        Image shieldImage = defenseIndicator.AddComponent<Image>();

        // Set as child of this entity
        RectTransform shieldRect = defenseIndicator.GetComponent<RectTransform>();
        shieldRect.SetParent(rectTransform, false);

        // Position in front of the entity
        shieldRect.anchorMin = new Vector2(0.5f, 0.5f);
        shieldRect.anchorMax = new Vector2(0.5f, 0.5f);
        shieldRect.anchoredPosition = new Vector2(0, 0);
        shieldRect.sizeDelta = new Vector2(80, 80);
        shieldRect.localPosition = new Vector3(70, 0, 0);

        // Use the assigned shield sprite
        if (defenseShieldSprite != null)
        {
            shieldImage.sprite = defenseShieldSprite;
            shieldImage.preserveAspect = true;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: No defense shield sprite assigned!");
        }

        // Initially hidden
        defenseIndicator.SetActive(false);
    }

    /// <summary>
    /// Sets the defense shield sprite
    /// </summary>
    public void SetDefenseShieldSprite(Sprite sprite)
    {
        defenseShieldSprite = sprite;

        // Update existing indicator if it exists
        if (defenseIndicator != null)
        {
            Image shieldImage = defenseIndicator.GetComponent<Image>();
            if (shieldImage != null)
            {
                shieldImage.sprite = sprite;
                shieldImage.preserveAspect = true;
            }
        }
    }


    /// <summary>
    /// Shows the defense indicator
    /// </summary>
    public void ShowDefenseIndicator()
    {
        if (defenseIndicator != null)
        {
            defenseIndicator.SetActive(true);
        }
    }

    /// <summary>
    /// Hides the defense indicator
    /// </summary>
    public void HideDefenseIndicator()
    {
        if (defenseIndicator != null)
        {
            defenseIndicator.SetActive(false);
        }
    }

    /// <summary>
    /// Updates the displayed sprite (e.g. after fatigue changes)
    /// </summary>
    public void UpdateSprite(Sprite sprite)
    {
        if (entityImage != null && sprite != null)
            entityImage.sprite = sprite;
    }

    /// <summary>
    /// Hides the entity sprite when dead
    /// </summary>
    public void HideSprite()
    {
        if (entityImage != null)
        {
            entityImage.gameObject.SetActive(false);
        }

        // Also hide name display when dead
        if (nameDisplay != null)
        {
            nameDisplay.SetActive(false);
        }
    }

    /// <summary>
    /// Creates the name display UI for all entities (positioned above the head)
    /// </summary>
    private void CreateNameDisplay(bool isChampion)
    {
        // Create a child GameObject for the name text
        nameDisplay = new GameObject("NameDisplay", typeof(RectTransform));
        nameText = nameDisplay.AddComponent<TextMeshProUGUI>();

        // Set as child of this entity
        RectTransform nameRect = nameDisplay.GetComponent<RectTransform>();
        nameRect.SetParent(rectTransform, false);

        nameRect.anchorMin = new Vector2(0.5f, 0.5f);
        nameRect.anchorMax = new Vector2(0.5f, 0.5f);
        nameRect.anchoredPosition = Vector2.zero;
        nameRect.sizeDelta = new Vector2(200, 50);
        nameRect.localPosition = new Vector3(0, 60, 0); // Above the head

        // Setup text properties
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.fontSize = 30;
        nameText.color = Color.white;

        // Set custom font from BattleManager if assigned
        if (BattleManager.Instance != null && BattleManager.Instance.ChampionNameFont != null)
        {
            nameText.font = BattleManager.Instance.ChampionNameFont;
        }

        // 이름 설정 및 즉시 표시
        nameText.text = entity.EntityName;
        nameDisplay.SetActive(true);
    }

    /// <summary>
    /// Updates the champion name display color based on HP percentage
    /// White when HP > 50%, yellow when HP <= 50%, orange when HP <= 10%
    /// </summary>
    public void UpdateChampionNameDisplay()
    {
        if (entity == null || !entity.IsChampion || nameText == null)
        {
            return;
        }

        float hpPercentage = (float)entity.CurrentHealth / entity.MaxHealth;

        if (hpPercentage > hpThresholdYellow)
        {
            nameText.color = Color.white;
        }
        else if (hpPercentage > hpThresholdOrange)
        {
            nameText.color = nameColorYellow;
        }
        else
        {
            nameText.color = nameColorOrange;
        }
    }

    /// <summary>
    /// 전투 UI에서 피로도 이펙트 생성 (챔피언 전용)
    /// 이미 Canvas 안에 있으므로 Image + Animator만 자식으로 추가
    /// </summary>
    public void SetupFatigueEffect(RuntimeAnimatorController controller, float fatiguePercent)
    {
        if (controller == null) return;

        // 이펙트 컨테이너 (부모 크기에 맞춤)
        GameObject effectObj = new GameObject("FatigueEffect", typeof(RectTransform));
        RectTransform effectRect = effectObj.GetComponent<RectTransform>();
        effectRect.SetParent(rectTransform, false);
        effectRect.anchorMin = Vector2.zero;
        effectRect.anchorMax = Vector2.one;
        effectRect.sizeDelta = Vector2.zero;
        effectRect.anchoredPosition = Vector2.zero;

        // Image + Animator
        Image effectImage = effectObj.AddComponent<Image>();
        effectImage.raycastTarget = false;

        Animator anim = effectObj.AddComponent<Animator>();
        anim.runtimeAnimatorController = controller;

        // FatigueEffect 컴포넌트로 제어
        fatigueEffect = effectObj.AddComponent<FatigueEffect>();
        fatigueEffect.SetAnimator(anim);

        // 현재 피로도에 따라 즉시 표시
        if (fatiguePercent >= 90f)
        {
            fatigueEffect.Show(2f);
        }
        else if (fatiguePercent >= 50f)
        {
            fatigueEffect.Show(1f);
        }
        else
        {
            fatigueEffect.Hide();
        }
    }

    public void UpdateFatigueEffect(float fatiguePercent)
    {
        if (fatigueEffect == null) return;

        if (fatiguePercent >= 90f)
            fatigueEffect.Show(2f);
        else if (fatiguePercent >= 50f)
            fatigueEffect.Show(1f);
        else
            fatigueEffect.Hide();
    }

    public RectTransform RectTransform => rectTransform;

    [Header("데미지 텍스트 설정")]
    [SerializeField] private float floatDuration = 0.8f;
    [SerializeField] private float floatDistance = 60f;

    public void ShowDamageText(int damage)
    {
        if (BattleManager.Instance == null || BattleManager.Instance.DamageText == null) return;

        // 이전 애니메이션 중단 (BattleManager에서 실행 중이므로 거기서 중단)
        if (floatingCoroutine != null)
        {
            BattleManager.Instance.StopCoroutine(floatingCoroutine);
        }

        TextMeshProUGUI tmp = BattleManager.Instance.DamageText;
        tmp.text = damage.ToString();

        // 데미지를 받은 엔티티 위치에 텍스트 배치
        RectTransform rt = tmp.rectTransform;
        rt.position = rectTransform.position;

        // 초기 상태 복원
        Color c = tmp.color;
        c.a = 1f;
        tmp.color = c;
        tmp.gameObject.SetActive(true);

        floatingCoroutine = BattleManager.Instance.StartCoroutine(FloatAndFadeCoroutine());
    }

    private IEnumerator FloatAndFadeCoroutine()
    {
        TextMeshProUGUI tmp = BattleManager.Instance.DamageText;
        RectTransform rt = tmp.rectTransform;
        Vector2 startPos = rt.anchoredPosition;

        float elapsed = 0f;
        while (elapsed < floatDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / floatDuration;

            // 위로 이동
            rt.anchoredPosition = startPos + new Vector2(0f, floatDistance * t);

            // 후반부에 페이드 아웃 (40% 지점부터 시작)
            if (t > 0.4f)
            {
                Color c = tmp.color;
                c.a = Mathf.Lerp(1f, 0f, (t - 0.4f) / 0.6f);
                tmp.color = c;
            }

            yield return null;
        }

        tmp.gameObject.SetActive(false);
        floatingCoroutine = null;
    }
}
