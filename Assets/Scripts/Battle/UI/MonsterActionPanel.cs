using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

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
    [SerializeField] private SkillSelectionPanel skillSelectionPanel;
    [SerializeField] private AllyTargetPanel allyTargetPanel;

    // Events for action selection
    public event Action OnBasicAttackClicked;
    public event Action<SkillData> OnSkillSelected; // Pass selected skill
    public event Action<SkillData, BattleEntity> OnSkillWithTargetSelected; // Pass skill and target for ally skills
    public event Action OnDefendClicked;

    private BattleEntity currentMonster;
    private SkillData pendingSkill; // Skill waiting for target selection
    private BattleSetup battleSetup; // Reference to battle setup

    public void Initialize(BattleSetup setup)
    {
        battleSetup = setup;
    }

    private void Awake()
    {
        // Setup button listeners
        if (basicAttackButton != null)
        {
            basicAttackButton.onClick.AddListener(HandleBasicAttackClick);
        }

        if (skillButton != null)
        {
            skillButton.onClick.AddListener(HandleSkillButtonClick);
        }

        if (defendButton != null)
        {
            defendButton.onClick.AddListener(HandleDefendClick);
        }

        // Setup skill selection panel events
        if (skillSelectionPanel != null)
        {
            skillSelectionPanel.OnSkillSelected += HandleSkillSelected;
        }

        // Setup ally target panel events
        if (allyTargetPanel != null)
        {
            allyTargetPanel.OnAllySelected += HandleAllyTargetSelected;
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
    /// Handles skill button click - opens skill selection panel
    /// </summary>
    private void HandleSkillButtonClick()
    {
        Debug.Log($"{currentMonster?.EntityName} - Opening skill selection");

        // Show skill selection panel
        if (skillSelectionPanel != null)
        {
            skillSelectionPanel.Show(currentMonster);
        }
        else
        {
            Debug.LogError("SkillSelectionPanel not assigned!");
        }
    }

    /// <summary>
    /// Handles skill selection from skill panel
    /// </summary>
    private void HandleSkillSelected(SkillData skill)
    {
        Debug.Log($"{currentMonster?.EntityName} - Skill selected: {skill.skillName}");

        // Check if skill requires ally target selection
        if (skill.targetType == SkillTarget.SingleAlly || skill.targetType == SkillTarget.AllAllies)
        {
            // Show ally target selection panel
            pendingSkill = skill;
            ShowAllyTargetSelection();
        }
        else
        {
            // No ally target needed - execute skill immediately
            OnSkillSelected?.Invoke(skill);
            Hide();
        }
    }

    /// <summary>
    /// Shows ally target selection panel
    /// </summary>
    private void ShowAllyTargetSelection()
    {
        if (allyTargetPanel == null)
        {
            Debug.LogError("AllyTargetPanel not assigned!");
            return;
        }

        // Get all alive ally monsters from BattleManager
        List<BattleEntity> allyMonsters = GetAllAllyMonsters();

        if (allyMonsters.Count == 0)
        {
            Debug.LogWarning("No allies available to target!");
            // Fall back to skill without target
            OnSkillSelected?.Invoke(pendingSkill);
            Hide();
            return;
        }

        // Show ally selection panel
        allyTargetPanel.Show(allyMonsters);
    }

    /// <summary>
    /// Gets all alive ally monsters from BattleSetup
    /// </summary>
    private List<BattleEntity> GetAllAllyMonsters()
    {
        List<BattleEntity> allies = new List<BattleEntity>();

        if (battleSetup != null && battleSetup.MonsterEntities != null)
        {
            foreach (var monster in battleSetup.MonsterEntities)
            {
                if (monster != null && monster.IsAlive)
                {
                    allies.Add(monster);
                }
            }
        }

        return allies;
    }

    /// <summary>
    /// Handles ally target selection
    /// </summary>
    private void HandleAllyTargetSelected(BattleEntity allyTarget)
    {
        Debug.Log($"Ally target selected: {allyTarget.EntityName} for skill: {pendingSkill.skillName}");
        OnSkillWithTargetSelected?.Invoke(pendingSkill, allyTarget);
        pendingSkill = null;
        Hide();
    }

    /// <summary>
    /// Handles back button from ally target selection
    /// </summary>
    public void HandleAllyTargetBack()
    {
        Debug.Log("Back from ally target selection - returning to skill selection");
        pendingSkill = null;

        // Show skill selection panel again
        if (skillSelectionPanel != null && currentMonster != null)
        {
            skillSelectionPanel.Show(currentMonster);
        }
    }

    /// <summary>
    /// Handles back button from skill selection panel
    /// </summary>
    public void HandleSkillPanelBack()
    {
        // Show main panel again
        Debug.Log("Back to main action panel");
        if (panelRoot != null && currentMonster != null)
        {
            panelRoot.SetActive(true);
        }
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
            skillButton.onClick.RemoveListener(HandleSkillButtonClick);
        }

        if (defendButton != null)
        {
            defendButton.onClick.RemoveListener(HandleDefendClick);
        }

        // Clean up skill panel events
        if (skillSelectionPanel != null)
        {
            skillSelectionPanel.OnSkillSelected -= HandleSkillSelected;
        }

        // Clean up ally target panel events
        if (allyTargetPanel != null)
        {
            allyTargetPanel.OnAllySelected -= HandleAllyTargetSelected;
        }
    }
}
