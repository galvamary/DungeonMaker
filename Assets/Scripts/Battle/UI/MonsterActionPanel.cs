using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// UI panel for player to control monster actions during battle
/// Shows buttons for Basic Attack, Skills, and Defend
/// </summary>
public class MonsterActionPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button basicAttackButton;
    [SerializeField] private Button skillButton;
    [SerializeField] private Button defendButton;

    // Events for action selection
    public event Action OnBasicAttackClicked;
    public event Action OnSkillClicked;
    public event Action OnDefendClicked;

    private BattleEntity currentMonster;

    private void Awake()
    {
        // Setup button listeners
        if (basicAttackButton != null)
        {
            basicAttackButton.onClick.AddListener(HandleBasicAttackClick);
        }

        if (skillButton != null)
        {
            skillButton.onClick.AddListener(HandleSkillClick);
        }

        if (defendButton != null)
        {
            defendButton.onClick.AddListener(HandleDefendClick);
        }

        // Hide panel initially
        Hide();
    }

    /// <summary>
    /// Shows the panel for a specific monster
    /// </summary>
    public void Show(BattleEntity monster)
    {
        currentMonster = monster;

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        UpdateButtonStates();
    }

    /// <summary>
    /// Hides the action panel
    /// </summary>
    public void Hide()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        currentMonster = null;
    }

    /// <summary>
    /// Updates button interactable states based on monster's available actions
    /// </summary>
    private void UpdateButtonStates()
    {
        if (currentMonster == null) return;

        // Basic attack is always available
        if (basicAttackButton != null)
        {
            basicAttackButton.interactable = true;
        }

        // Skill button - check if monster has usable skills
        if (skillButton != null)
        {
            bool hasUsableSkill = HasUsableSkill();
            skillButton.interactable = hasUsableSkill;
        }

        // Defend is always available
        if (defendButton != null)
        {
            defendButton.interactable = true;
        }
    }

    /// <summary>
    /// Checks if monster has any usable skills
    /// </summary>
    private bool HasUsableSkill()
    {
        if (currentMonster.AvailableSkills == null || currentMonster.AvailableSkills.Count == 0)
        {
            return false;
        }

        foreach (var skill in currentMonster.AvailableSkills)
        {
            if (currentMonster.CanUseMP(skill.mpCost))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Handles basic attack button click
    /// </summary>
    private void HandleBasicAttackClick()
    {
        Debug.Log($"{currentMonster?.EntityName} - Basic Attack selected");
        OnBasicAttackClicked?.Invoke();
        Hide();
    }

    /// <summary>
    /// Handles skill button click
    /// </summary>
    private void HandleSkillClick()
    {
        Debug.Log($"{currentMonster?.EntityName} - Skill selected");
        OnSkillClicked?.Invoke();
        Hide();
    }

    /// <summary>
    /// Handles defend button click
    /// </summary>
    private void HandleDefendClick()
    {
        Debug.Log($"{currentMonster?.EntityName} - Defend selected");
        OnDefendClicked?.Invoke();
        Hide();
    }

    private void OnDestroy()
    {
        // Clean up button listeners
        if (basicAttackButton != null)
        {
            basicAttackButton.onClick.RemoveListener(HandleBasicAttackClick);
        }

        if (skillButton != null)
        {
            skillButton.onClick.RemoveListener(HandleSkillClick);
        }

        if (defendButton != null)
        {
            defendButton.onClick.RemoveListener(HandleDefendClick);
        }
    }
}
