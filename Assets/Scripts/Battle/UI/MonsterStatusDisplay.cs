using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.CompilerServices;

/// <summary>
/// Displays individual monster's status (name, HP, MP)
/// </summary>
public class MonsterStatusDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private Slider mpSlider;
    [SerializeField] private TextMeshProUGUI mpText;
    [SerializeField] private GameObject hpFill; // HP slider fill object
    [SerializeField] private GameObject mpFill; // MP slider fill object

    private BattleEntity monster;
    public BattleEntity Monster => monster;

    private void Awake()
    {
        // Set outline for text elements
        SetupTextOutline(hpText);
        SetupTextOutline(mpText);
    }

    /// <summary>
    /// Setup black outline for TextMeshPro text
    /// </summary>
    private void SetupTextOutline(TextMeshProUGUI text)
    {
        if (text == null) return;

        // Create material instance to avoid affecting other texts
        text.fontMaterial = new UnityEngine.Material(text.fontMaterial);

        text.outlineWidth = 0.2f;
        text.outlineColor = Color.black;
    }

    /// <summary>
    /// Initialize the status display with a monster
    /// </summary>
    public void Initialize(BattleEntity entity)
    {
        if (entity == null || !entity.IsMonster)
        {
            Debug.LogError("MonsterStatusDisplay: Invalid monster entity");
            return;
        }

        monster = entity;
        UpdateDisplay();
    }

    /// <summary>
    /// Update all status values
    /// </summary>
    public void UpdateDisplay()
    {
        if (monster == null) return;

        // Update name
        if (nameText != null)
        {
            nameText.text = monster.EntityName;
        }

        // Update HP
        UpdateHP();

        // Update MP
        UpdateMP();
    }

    /// <summary>
    /// Update HP slider and text
    /// </summary>
    public void UpdateHP()
    {
        if (monster == null) return;

        if (hpSlider != null)
        {
            hpSlider.maxValue = monster.MaxHealth;
            hpSlider.value = monster.CurrentHealth;
        }

        // Hide fill when HP is 0
        if (hpFill != null)
        {
            hpFill.SetActive(monster.CurrentHealth > 0);
        }

        if (hpText != null)
        {
            hpText.text = $"{monster.CurrentHealth} / {monster.MaxHealth}";
        }
    }

    /// <summary>
    /// Update MP slider and text
    /// </summary>
    public void UpdateMP()
    {
        if (monster == null) return;

        if (mpSlider != null)
        {
            mpSlider.maxValue = monster.MaxMP;
            mpSlider.value = monster.CurrentMP;
        }

        // Hide fill when MP is 0
        if (mpFill != null)
        {
            mpFill.SetActive(monster.CurrentMP > 0);
        }

        if (mpText != null)
        {
            mpText.text = $"{monster.CurrentMP} / {monster.MaxMP}";
        }
    }
}
