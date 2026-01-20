using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Ally target selection panel - shows list of ally monsters to target with support skills
/// Dynamically creates ally buttons based on available alive monsters
/// </summary>
public class AllyTargetPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform allyButtonContainer; // Parent for ally buttons
    [SerializeField] private GameObject allyButtonPrefab;   // Prefab for ally button
    [SerializeField] private Button backButton;

    // Event for ally selection
    public event Action<BattleEntity> OnAllySelected;
    public event Action OnBackClicked;

    private List<BattleEntity> availableAllies = new List<BattleEntity>();
    private List<GameObject> spawnedButtons = new List<GameObject>();

    private void Awake()
    {
        // Setup back button
        if (backButton != null)
        {
            backButton.onClick.AddListener(HandleBackClick);
        }

        // Hide panel initially
        Hide();
    }

    /// <summary>
    /// Shows ally target selection panel
    /// </summary>
    public void Show(List<BattleEntity> allies)
    {
        availableAllies = allies;

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        // Generate ally buttons
        GenerateAllyButtons();
    }

    /// <summary>
    /// Hides the ally target selection panel
    /// </summary>
    public void Hide()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        // Clear all spawned buttons
        ClearAllyButtons();
        availableAllies.Clear();
    }

    /// <summary>
    /// Generates buttons for each available ally
    /// </summary>
    private void GenerateAllyButtons()
    {
        // Clear existing buttons
        ClearAllyButtons();

        if (availableAllies == null || availableAllies.Count == 0)
        {
            Debug.LogWarning("No allies available to target!");
            return;
        }

        // Create button for each ally
        foreach (var ally in availableAllies)
        {
            CreateAllyButton(ally);
        }
    }

    /// <summary>
    /// Creates a button for an ally monster
    /// </summary>
    private void CreateAllyButton(BattleEntity ally)
    {
        if (allyButtonPrefab == null || allyButtonContainer == null)
        {
            Debug.LogError("AllyButtonPrefab or AllyButtonContainer not assigned!");
            return;
        }

        // Instantiate button
        GameObject buttonObj = Instantiate(allyButtonPrefab, allyButtonContainer);
        spawnedButtons.Add(buttonObj);

        // Get button component
        Button button = buttonObj.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("AllyButtonPrefab doesn't have a Button component!");
            return;
        }

        // Setup button onClick
        button.onClick.AddListener(() => HandleAllyClick(ally));

        // Update button text
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = $"{ally.EntityName}";
        }

        // // Update button icon (if available)
        // Image iconImage = buttonObj.transform.Find("Icon")?.GetComponent<Image>();
        // if (iconImage != null && ally != null)
        // {
        //     // Try to get sprite from the entity's Image component
        //     Image entityImage = ally.GetComponent<Image>();
        //     if (entityImage != null && entityImage.sprite != null)
        //     {
        //         iconImage.sprite = entityImage.sprite;
        //     }
        // }
    }

    /// <summary>
    /// Clears all spawned ally buttons
    /// </summary>
    private void ClearAllyButtons()
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

    /// <summary>
    /// Handles ally button click
    /// </summary>
    private void HandleAllyClick(BattleEntity ally)
    {
        Debug.Log($"Ally selected: {ally.EntityName}");
        OnAllySelected?.Invoke(ally);
        Hide();
    }

    /// <summary>
    /// Handles back button click
    /// </summary>
    private void HandleBackClick()
    {
        Debug.Log("Back from ally selection");
        OnBackClicked?.Invoke();
        Hide();
    }

    private void OnDestroy()
    {
        // Clean up back button
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(HandleBackClick);
        }
    }
}
