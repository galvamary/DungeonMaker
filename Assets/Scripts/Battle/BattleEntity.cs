using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class BattleEntity : MonoBehaviour
{
    [Header("Entity Data")]
    private string entityName;
    private Sprite entitySprite;
    private int currentHealth;
    private int maxHealth;
    private int currentMP;
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

    [Header("Visual")]
    private RectTransform rectTransform;
    private Image uiImage;
    private Vector3 originalPosition;
    private GameObject defenseIndicator; // Shield icon when defending

    [Header("Defense Visual")]
    [SerializeField] private Sprite defenseShieldSprite; // Assign in Inspector or via code

    public string EntityName => entityName;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int CurrentMP => currentMP;
    public int MaxMP => maxMP;
    public int Attack => attack;
    public int Defense => defense;
    public int Speed => speed;
    public bool IsChampion => isChampion;
    public bool IsAlive => currentHealth > 0;
    public SkillData BasicAttackSkill => basicAttackSkill;
    public List<SkillData> AvailableSkills => availableSkills;
    public bool IsDefending => isDefending;

    public void SaveOriginalPosition()
    {
        if (rectTransform != null)
        {
            originalPosition = rectTransform.position;
            Debug.Log($"[{entityName}] SaveOriginalPosition: {originalPosition}");
        }
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        uiImage = GetComponent<Image>();
        if (uiImage == null)
        {
            uiImage = gameObject.AddComponent<Image>();
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

    public void InitializeFromChampion(Champion champion)
    {
        entityName = champion.Data.championName;
        entitySprite = champion.Data.icon;
        currentHealth = champion.CurrentHealth;
        maxHealth = champion.Data.maxHealth;
        currentMP = champion.CurrentMP;
        maxMP = champion.Data.maxMP;
        attack = champion.Data.attack;
        defense = champion.Data.defense;
        baseDefense = champion.Data.defense;
        speed = champion.Data.speed;
        isChampion = true;

        // Copy basic attack and skills
        basicAttackSkill = champion.Data.basicAttack;
        availableSkills.Clear();
        if (champion.Data.skills != null)
        {
            availableSkills.AddRange(champion.Data.skills);
        }

        SetupVisual();
    }

    public void InitializeFromMonster(MonsterData monster)
    {
        entityName = monster.monsterName;
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

        SetupVisual();
    }

    private void SetupVisual()
    {
        if (uiImage != null && entitySprite != null)
        {
            uiImage.sprite = entitySprite;
            uiImage.preserveAspect = true;
        }

        // Scale based on type
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one * (isChampion ? 2.5f : 2f);
            // Original position will be saved after parent is set in BattleManager
        }

        gameObject.name = $"BattleEntity_{entityName}";

        // Create defense indicator (initially hidden)
        CreateDefenseIndicator();
    }

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
        shieldRect.anchoredPosition = new Vector2(0, 0); // Center on entity
        shieldRect.sizeDelta = new Vector2(80, 80); // Slightly larger than entity
        shieldRect.localPosition = new Vector3(70, 0, 0);

        // Use the assigned shield sprite
        if (defenseShieldSprite != null)
        {
            shieldImage.sprite = defenseShieldSprite;
            shieldImage.preserveAspect = true;
        }
        else
        {
            Debug.LogWarning($"{entityName}: No defense shield sprite assigned! Please assign one in the Inspector or via SetDefenseShieldSprite()");
        }

        // Initially hidden
        defenseIndicator.SetActive(false);
    }

    // Method to set defense shield sprite via code
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

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($"{entityName} took {damage} damage! Remaining health: {currentHealth}/{maxHealth}");

        if (!IsAlive)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{entityName} has been defeated!");
        // Visual effect can be added here
    }

    public bool CanUseMP(int mpCost)
    {
        return currentMP >= mpCost;
    }

    public void UseMP(int mpCost)
    {
        currentMP -= mpCost;
        currentMP = Mathf.Max(0, currentMP);
        Debug.Log($"{entityName} used {mpCost} MP. Remaining MP: {currentMP}/{maxMP}");
    }

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
    }

    public void RestoreMP(int amount)
    {
        currentMP += amount;
        currentMP = Mathf.Min(currentMP, maxMP);
        Debug.Log($"{entityName} restored {amount} MP. Current MP: {currentMP}/{maxMP}");
    }

    // Defense action - doubles defense for 1 turn
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
        if (defenseIndicator != null)
        {
            defenseIndicator.SetActive(true);
        }
    }

    // Called at the end of each turn to update defense status
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

    // Reset defense to normal
    private void EndDefending()
    {
        if (!isDefending) return;

        isDefending = false;
        defendingTurnsRemaining = 0;
        defense = baseDefense;
        Debug.Log($"{entityName}'s defensive stance ended. Defense: {defense}");

        // Hide defense indicator
        if (defenseIndicator != null)
        {
            defenseIndicator.SetActive(false);
        }
    }

    // Force end defense (e.g., when taking certain actions)
    public void CancelDefense()
    {
        if (isDefending)
        {
            EndDefending();
        }
    }

    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
    }

    public void SetMP(int mp)
    {
        currentMP = Mathf.Clamp(mp, 0, maxMP);
    }

    public IEnumerator PerformSkillWithAnimation(SkillData skill, BattleEntity target, List<BattleEntity> allTargets = null)
    {
        if (skill == null)
        {
            Debug.LogWarning($"{entityName} tried to use a null skill!");
            yield break;
        }

        if (!CanUseMP(skill.mpCost))
        {
            Debug.LogWarning($"{entityName} doesn't have enough MP to use {skill.skillName}! (Required: {skill.mpCost}, Current: {currentMP})");
            yield break;
        }

        UseMP(skill.mpCost);
        Debug.Log($"{entityName} uses {skill.skillName}!");

        // Attack animation - move towards target (only for single-target attack skills)
        bool movedForAttack = false;
        if (skill.skillType == SkillType.Attack && skill.targetType == SkillTarget.SingleEnemy && target != null && rectTransform != null)
        {
            yield return StartCoroutine(AttackAnimation(target));
            movedForAttack = true;
        }
        else
        {
            Debug.Log($"No animation: skillType={skill.skillType}, targetType={skill.targetType}, target={target}, rectTransform={rectTransform}");
        }

        // Show skill effect
        if (skill.effectPrefab != null)
        {
            ShowSkillEffect(skill, target, allTargets);
        }

        // Execute skill effect
        switch (skill.skillType)
        {
            case SkillType.Attack:
                ExecuteAttackSkill(skill, target, allTargets);
                break;
            case SkillType.Heal:
                ExecuteHealSkill(skill, target);
                break;
            default:
                Debug.LogWarning($"Skill type {skill.skillType} not implemented yet!");
                break;
        }

        // Wait a bit to see the impact
        yield return new WaitForSeconds(1.0f);

        // Return to original position (only if moved)
        if (movedForAttack && rectTransform != null)
        {
            yield return StartCoroutine(ReturnToPosition());
        }
    }

    private IEnumerator AttackAnimation(BattleEntity target)
    {
        if (target.rectTransform == null)
        {
            Debug.LogError($"Target {target.EntityName} has no RectTransform!");
            yield break;
        }

        Vector3 startPos = rectTransform.position;  // Canvas space position
        Vector3 targetPos = target.rectTransform.position;  // Canvas space position

        // Move to target's Y position with offset
        // Champion attacks from left (Y + offset), Monster attacks from right (Y - offset)
        float xOffset = isChampion ? 400f : -400f;
        Vector3 attackPos = new Vector3(targetPos.x + xOffset, targetPos.y, targetPos.z);

        Debug.Log($"Attack Animation - Start: {startPos}, Target: {targetPos}, AttackPos: {attackPos}");

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            Vector3 newPos = Vector3.Lerp(startPos, attackPos, t);
            rectTransform.position = newPos;
            yield return null;
        }

        rectTransform.position = attackPos;
    }

    private IEnumerator ReturnToPosition()
    {
        Vector3 startPos = rectTransform.position;  // Canvas space position
        Debug.Log($"[{entityName}] ReturnToPosition - From: {startPos}, To: {originalPosition}");

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rectTransform.position = Vector3.Lerp(startPos, originalPosition, t);
            yield return null;
        }

        rectTransform.position = originalPosition;
        Debug.Log($"[{entityName}] Return complete. Final position: {rectTransform.position}");
    }

    private void ShowSkillEffect(SkillData skill, BattleEntity target, List<BattleEntity> allTargets)
    {
        if (skill.effectPrefab == null)
            return;

        switch (skill.targetType)
        {
            case SkillTarget.SingleEnemy:
            case SkillTarget.SingleAlly:
                if (target != null && target.rectTransform != null)
                {
                    SpawnEffect(skill.effectPrefab, target.rectTransform.position);
                }
                break;

            case SkillTarget.AllEnemies:
                if (allTargets != null)
                {
                    foreach (var enemy in allTargets)
                    {
                        if (enemy != null && enemy.IsAlive && enemy.rectTransform != null)
                        {
                            SpawnEffect(skill.effectPrefab, enemy.rectTransform.position);
                        }
                    }
                }
                break;

            case SkillTarget.Self:
                if (rectTransform != null)
                {
                    SpawnEffect(skill.effectPrefab, rectTransform.position);
                }
                break;
        }
    }

    private void SpawnEffect(GameObject effectPrefab, Vector3 position)
    {
        GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity);

        // Set as child of same canvas for proper rendering
        if (rectTransform != null && rectTransform.parent != null)
        {
            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                effect.transform.SetParent(canvas.transform, true);
                effect.transform.position = position;
            }
        }

        // Get animation length and destroy after it completes
        Animator animator = effect.GetComponent<Animator>();
        float destroyTime = 1.0f; // Default
        float animationSpeed = 2.0f; // Animation playback speed (increase for faster effects)

        if (animator != null)
        {
            animator.speed = animationSpeed; // Speed up the animation

            if (animator.runtimeAnimatorController != null)
            {
                AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
                if (clips.Length > 0)
                {
                    destroyTime = clips[0].length / animationSpeed; // Adjust destroy time
                }
            }
        }

        Destroy(effect, destroyTime);
    }

    private void ExecuteAttackSkill(SkillData skill, BattleEntity target, List<BattleEntity> allTargets)
    {
        switch (skill.targetType)
        {
            case SkillTarget.SingleEnemy:
                if (target != null && target.IsAlive)
                {
                    // Damage = skill power + attacker's attack - target's defense (minimum 1)
                    int totalDamage = skill.power + attack;
                    int damage = Mathf.Max(1, totalDamage - target.Defense);
                    target.TakeDamage(damage);
                }
                break;

            case SkillTarget.AllEnemies:
                if (allTargets != null)
                {
                    foreach (var enemy in allTargets)
                    {
                        if (enemy != null && enemy.IsAlive)
                        {
                            int totalDamage = skill.power + attack;
                            int damage = Mathf.Max(1, totalDamage - enemy.Defense);
                            enemy.TakeDamage(damage);
                        }
                    }
                }
                break;

            default:
                Debug.LogWarning($"Target type {skill.targetType} not supported for attack skills!");
                break;
        }
    }

    private void ExecuteHealSkill(SkillData skill, BattleEntity target)
    {
        switch (skill.targetType)
        {
            case SkillTarget.Self:
                Heal(skill.power);
                break;

            case SkillTarget.SingleAlly:
                if (target != null && target.IsAlive)
                {
                    target.Heal(skill.power);
                }
                break;

            default:
                Debug.LogWarning($"Target type {skill.targetType} not supported for heal skills!");
                break;
        }
    }
}
