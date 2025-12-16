using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles visual representation and effects for battle entities
/// Manages UI sprites, defense shield indicator, and visual animations
/// </summary>
public class BattleEntityVisual : MonoBehaviour
{
    [Header("Visual Components")]
    private RectTransform rectTransform;
    private Image entityImage;
    private GameObject defenseIndicator;

    [Header("Defense Visual")]
    [SerializeField] private Sprite defenseShieldSprite;

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
    }

    public RectTransform RectTransform => rectTransform;
}
