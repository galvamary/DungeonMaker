using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

        // Create name display for champions (initially hidden)
        if (isChampion)
        {
            CreateNameDisplay();
        }
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
    /// Creates the name display UI for champions (positioned above the head)
    /// </summary>
    private void CreateNameDisplay()
    {
        // Create a child GameObject for the name text
        nameDisplay = new GameObject("NameDisplay", typeof(RectTransform));
        nameText = nameDisplay.AddComponent<TextMeshProUGUI>();

        // Set as child of this entity
        RectTransform nameRect = nameDisplay.GetComponent<RectTransform>();
        nameRect.SetParent(rectTransform, false);

        // Position above the entity's head
        nameRect.eulerAngles = new Vector3(0f, 180f, 0f);
        nameRect.anchorMin = new Vector2(0.5f, 0.5f);
        nameRect.anchorMax = new Vector2(0.5f, 0.5f);
        nameRect.anchoredPosition = Vector2.zero;
        nameRect.sizeDelta = new Vector2(200, 50);
        nameRect.localPosition = new Vector3(0, 60, 0); // Above the head

        // Setup text properties
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.fontSize = 30;
        nameText.fontStyle = FontStyles.Bold;

        // Set custom font from BattleManager if assigned
        if (BattleManager.Instance != null && BattleManager.Instance.ChampionNameFont != null)
        {
            nameText.font = BattleManager.Instance.ChampionNameFont;
        }

        // Initially hidden (will show when HP <= 50%)
        nameDisplay.SetActive(false);
    }

    /// <summary>
    /// Updates the name display based on champion's HP percentage
    /// Hidden when HP > 50%, yellow when HP <= 50%, orange when HP <= 10%
    /// </summary>
    public void UpdateChampionNameDisplay()
    {
        if (entity == null || !entity.IsChampion || nameDisplay == null || nameText == null)
        {
            return;
        }

        float hpPercentage = (float)entity.CurrentHealth / entity.MaxHealth;

        if (hpPercentage > hpThresholdYellow)
        {
            // HP > 50%: Hide name
            nameDisplay.SetActive(false);
        }
        else if (hpPercentage > hpThresholdOrange)
        {
            // 10% < HP <= 50%: Show name in yellow
            nameDisplay.SetActive(true);
            nameText.text = entity.EntityName;
            nameText.color = nameColorYellow;
        }
        else
        {
            // HP <= 10%: Show name in orange
            nameDisplay.SetActive(true);
            nameText.text = entity.EntityName;
            nameText.color = nameColorOrange;
        }
    }

    public RectTransform RectTransform => rectTransform;
}
