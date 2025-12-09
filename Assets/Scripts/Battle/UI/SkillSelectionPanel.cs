using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Skill selection panel - shows list of available skills for monster to use
/// Dynamically creates skill buttons based on monster's available skills
/// </summary>
public class SkillSelectionPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform skillButtonContainer; // Parent for skill buttons
    [SerializeField] private GameObject skillButtonPrefab;   // Prefab for skill button
    [SerializeField] private Button backButton;

    // Event for skill selection
    public event Action<SkillData> OnSkillSelected;

    private BattleEntity currentMonster;
    private List<GameObject> spawnedButtons = new List<GameObject>();

    private void Awake()
    {
        // Hide panel initially
        Hide();
    }

    /// <summary>
    /// Shows skill selection panel for a monster
    /// </summary>
    public void Show(BattleEntity monster)
    {
        currentMonster = monster;

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        // Generate skill buttons
        GenerateSkillButtons();
    }

    /// <summary>
    /// Hides the skill selection panel
    /// </summary>
    public void Hide()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        // Clear all spawned buttons
        ClearSkillButtons();
        currentMonster = null;
    }

    /// <summary>
    /// Generates buttons for each available skill
    /// </summary>
    private void GenerateSkillButtons()
    {
        // Clear existing buttons
        ClearSkillButtons();

        if (currentMonster == null || currentMonster.AvailableSkills == null)
        {
            Debug.LogWarning("No skills available for this monster!");
            return;
        }

        // Create button for each skill
        foreach (var skill in currentMonster.AvailableSkills)
        {
            CreateSkillButton(skill);
        }
    }

    /// <summary>
    /// Creates a single skill button
    /// </summary>
    private void CreateSkillButton(SkillData skill)
    {
        if (skillButtonPrefab == null || skillButtonContainer == null)
        {
            Debug.LogError("Skill button prefab or container not assigned!");
            return;
        }

        // Instantiate button
        GameObject buttonObj = Instantiate(skillButtonPrefab, skillButtonContainer);
        spawnedButtons.Add(buttonObj);

        // Get button component
        Button button = buttonObj.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("Skill button prefab missing Button component!");
            return;
        }

        // Get text components (assuming structure: Button > Text)
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        
        if (buttonText != null)
        {
            // Display skill name and MP cost
            buttonText.text = $"{skill.skillName} (MP: {skill.mpCost})";
        }

        // Check if skill is usable
        bool canUse = currentMonster.CanUseMP(skill.mpCost);
        button.interactable = canUse;

        // Add click listener
        button.onClick.AddListener(() => HandleSkillClick(skill));

        // Optional: Add visual feedback for unusable skills
        if (!canUse && buttonText != null)
        {
            buttonText.color = Color.gray;
        }
    }

    /// <summary>
    /// Handles skill button click
    /// </summary>
    private void HandleSkillClick(SkillData skill)
    {
        Debug.Log($"Skill selected: {skill.skillName}");
        OnSkillSelected?.Invoke(skill);
        Hide();
    }

    /// <summary>
    /// Clears all spawned skill buttons
    /// </summary>
    private void ClearSkillButtons()
    {
        foreach (var button in spawnedButtons)
        {
            if (button != null)
            {
                Destroy(button);
            }
        }
        spawnedButtons.Clear();
    }
}
