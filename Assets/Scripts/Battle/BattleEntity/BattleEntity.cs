using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Core battle entity class - manages stats, health, MP, and status effects
/// This is the main data container for entities in battle
/// </summary>
public class BattleEntity : MonoBehaviour
{
    [Header("Entity Data")]
    private string entityName;
    private Sprite entitySprite;
    [SerializeField] private int currentHealth;
    private int maxHealth;
    [SerializeField] private int currentMP;
    private int maxMP;
    private int attack;
    private int defense;
    private int baseDefense; // Store original defense value
    private int speed;
    private bool isChampion;
    private SkillData basicAttackSkill;
    private List<SkillData> availableSkills = new List<SkillData>();

    [Header("Status Effects")]
    private bool isDefending = false;
    private int defendingTurnsRemaining = 0;

    [Header("Components")]
    private BattleEntityVisual visual;
    private BattleEntityAnimator animator;
    private BattleSkillExecutor skillExecutor;

    // Public properties
    public string EntityName => entityName;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int CurrentMP => currentMP;
    public int MaxMP => maxMP;
    public int Attack => attack;
    public int Defense => defense;
    public int Speed => speed;
    public bool IsChampion => isChampion;
    public bool IsMonster => !isChampion;
    public bool IsAlive => currentHealth > 0;
    public SkillData BasicAttackSkill => basicAttackSkill;
    public List<SkillData> AvailableSkills => availableSkills;
    public bool IsDefending => isDefending;

    private void Awake()
    {
        // Get or add required components
        visual = GetComponent<BattleEntityVisual>();
        if (visual == null)
        {
            visual = gameObject.AddComponent<BattleEntityVisual>();
        }

        animator = GetComponent<BattleEntityAnimator>();
        if (animator == null)
        {
            animator = gameObject.AddComponent<BattleEntityAnimator>();
        }

        skillExecutor = GetComponent<BattleSkillExecutor>();
        if (skillExecutor == null)
        {
            skillExecutor = gameObject.AddComponent<BattleSkillExecutor>();
        }
    }

    /// <summary>
    /// Initializes entity from a Champion
    /// </summary>
    public void InitializeFromChampion(Champion champion)
    {
        entityName = champion.Data.championName;
        entitySprite = champion.CurrentSprite; // Use current sprite based on fatigue
        currentHealth = champion.CurrentHealth;
        currentMP = champion.CurrentMP;

        // Calculate max HP/MP based on reputation
        int reputation = GameManager.Instance != null ? GameManager.Instance.CurrentReputation : 1;
        maxHealth = champion.Data.maxHealth * reputation;
        maxMP = champion.Data.maxMP * reputation;

        // 피로도로 감소된 스탯 사용
        attack = champion.EffectiveAttack;
        defense = champion.EffectiveDefense;
        baseDefense = champion.EffectiveDefense;
        speed = champion.EffectiveSpeed;
        isChampion = true;

        Debug.Log($"{entityName} battle stats (Fatigue: {champion.CurrentFatigue:F1}%) - " +
                  $"ATK: {attack}, DEF: {defense}, SPD: {speed}");

        // Copy basic attack and filtered skills
        basicAttackSkill = champion.Data.basicAttack;
        availableSkills.Clear();
        if (champion.AvailableSkills != null)
        {
            availableSkills.AddRange(champion.AvailableSkills);
            Debug.Log($"{entityName} has {availableSkills.Count} available skills in battle");
        }

        SetupComponents();
    }

    /// <summary>
    /// Initializes entity from a MonsterData
    /// </summary>
    /// <param name="monster">The monster data to initialize from</param>
    /// <param name="displayName">Optional custom display name (used for duplicate monsters with numbers)</param>
    public void InitializeFromMonster(MonsterData monster, string displayName = null)
    {
        // Use custom display name if provided, otherwise use monster's base name
        entityName = string.IsNullOrEmpty(displayName) ? monster.monsterName : displayName;
        entitySprite = monster.icon;
        currentHealth = monster.maxHealth;
        maxHealth = monster.maxHealth;
        currentMP = monster.maxMP;
        maxMP = monster.maxMP;
        attack = monster.attack;
        defense = monster.defense;
        baseDefense = monster.defense;
        speed = monster.speed;
        isChampion = false;

        // Copy basic attack and skills
        basicAttackSkill = monster.basicAttack;
        availableSkills.Clear();
        if (monster.skills != null)
        {
            availableSkills.AddRange(monster.skills);
        }

        SetupComponents();
    }

    /// <summary>
    /// Sets up all component references
    /// </summary>
    private void SetupComponents()
    {
        gameObject.name = $"BattleEntity_{entityName}";

        // Initialize visual component
        visual.Initialize(this);
        visual.SetupVisual(entitySprite, isChampion);

        // Initialize animator component
        animator.Initialize(isChampion);

        // Initialize skill executor
        skillExecutor.Initialize(this, animator, visual);
    }

    /// <summary>
    /// Sets the defense shield sprite for visual component
    /// </summary>
    public void SetDefenseShieldSprite(Sprite sprite)
    {
        if (visual != null)
        {
            visual.SetDefenseShieldSprite(sprite);
        }
    }

    /// <summary>
    /// Saves the original position (called after positioning in scene)
    /// </summary>
    public void SaveOriginalPosition()
    {
        if (animator != null)
        {
            animator.SaveOriginalPosition();
        }
    }

    #region Health and MP Management

    /// <summary>
    /// Applies damage to this entity
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($"{entityName} took {damage} damage! Remaining health: {currentHealth}/{maxHealth}");

        // Update status display
        if (BattleManager.Instance != null)
        {
            if (IsMonster)
            {
                BattleManager.Instance.UpdateMonsterHP(this);
            }
        }

        // Update champion name display (head-mounted)
        if (IsChampion && visual != null)
        {
            visual.UpdateChampionNameDisplay();
        }

        if (!IsAlive)
        {
            Die();
        }
    }

    /// <summary>
    /// Heals this entity
    /// </summary>
    public void Heal(int amount)
    {
        if (!IsAlive)
        {
            Debug.LogWarning($"{entityName} is dead and cannot be healed!");
            return;
        }

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        Debug.Log($"{entityName} healed {amount} HP. Current HP: {currentHealth}/{maxHealth}");

        // Update status display
        if (BattleManager.Instance != null)
        {
            if (IsMonster)
            {
                BattleManager.Instance.UpdateMonsterHP(this);
            }
        }

        // Update champion name display (head-mounted)
        if (IsChampion && visual != null)
        {
            visual.UpdateChampionNameDisplay();
        }
    }

    /// <summary>
    /// Checks if entity has enough MP for a cost
    /// </summary>
    public bool CanUseMP(int mpCost)
    {
        return currentMP >= mpCost;
    }

    /// <summary>
    /// Uses MP (deducts from current MP)
    /// </summary>
    public void UseMP(int mpCost)
    {
        currentMP -= mpCost;
        currentMP = Mathf.Max(0, currentMP);
        Debug.Log($"{entityName} used {mpCost} MP. Remaining MP: {currentMP}/{maxMP}");

        // Update status display for monsters
        if (IsMonster && BattleManager.Instance != null)
        {
            BattleManager.Instance.UpdateMonsterMP(this);
        }
    }

    /// <summary>
    /// Restores MP
    /// </summary>
    public void RestoreMP(int amount)
    {
        currentMP += amount;
        currentMP = Mathf.Min(currentMP, maxMP);
        Debug.Log($"{entityName} restored {amount} MP. Current MP: {currentMP}/{maxMP}");

        // Update status display for monsters
        if (IsMonster && BattleManager.Instance != null)
        {
            BattleManager.Instance.UpdateMonsterMP(this);
        }
    }

    /// <summary>
    /// Called when entity dies
    /// </summary>
    private void Die()
    {
        Debug.Log($"{entityName} has been defeated!");

        // Hide sprite when dead
        if (visual != null)
        {
            visual.HideSprite();
        }
    }

    #endregion

    #region Defense System

    /// <summary>
    /// Starts defending - doubles defense for 1 turn
    /// </summary>
    public void StartDefending()
    {
        if (isDefending)
        {
            Debug.LogWarning($"{entityName} is already defending!");
            return;
        }

        isDefending = true;
        defendingTurnsRemaining = 1;
        defense = baseDefense * 2;
        Debug.Log($"{entityName} takes a defensive stance! Defense: {baseDefense} → {defense}");

        // Show defense indicator
        if (visual != null)
        {
            visual.ShowDefenseIndicator();
        }
    }

    /// <summary>
    /// Updates defense status (called at end of each round)
    /// </summary>
    public void UpdateDefenseStatus()
    {
        if (isDefending)
        {
            defendingTurnsRemaining--;

            if (defendingTurnsRemaining <= 0)
            {
                EndDefending();
            }
        }
    }

    /// <summary>
    /// Ends defending status
    /// </summary>
    private void EndDefending()
    {
        if (!isDefending) return;

        isDefending = false;
        defendingTurnsRemaining = 0;
        defense = baseDefense;
        Debug.Log($"{entityName}'s defensive stance ended. Defense: {defense}");

        // Hide defense indicator
        if (visual != null)
        {
            visual.HideDefenseIndicator();
        }
    }

    #endregion

    #region Skill Execution

    /// <summary>
    /// Performs a skill with full animation and effects
    /// Delegates to BattleSkillExecutor component
    /// </summary>
    public IEnumerator PerformSkillWithAnimation(SkillData skill, BattleEntity target, List<BattleEntity> allTargets = null)
    {
        if (skillExecutor != null)
        {
            yield return skillExecutor.StartCoroutine(skillExecutor.PerformSkillWithAnimation(skill, target, allTargets));
        }
        else
        {
            Debug.LogError($"{entityName}: No BattleSkillExecutor component found!");
        }
    }

    #endregion
}
