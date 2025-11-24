using UnityEngine;
using UnityEngine.UI;

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
    private int speed;
    private bool isChampion;

    [Header("Visual")]
    private RectTransform rectTransform;
    private Image uiImage;

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

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        uiImage = GetComponent<Image>();
        if (uiImage == null)
        {
            uiImage = gameObject.AddComponent<Image>();
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
        speed = champion.Data.speed;
        isChampion = true;

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
        }

        gameObject.name = $"BattleEntity_{entityName}";
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

    public void PerformBasicAttack(BattleEntity target)
    {
        if (target == null || !target.IsAlive)
        {
            Debug.LogWarning($"{entityName} tried to attack a dead or null target!");
            return;
        }

        Debug.Log($"{entityName} attacks {target.EntityName}!");

        // Calculate damage: attacker's attack - target's defense (minimum 1 damage)
        int damage = Mathf.Max(1, attack - target.Defense);
        target.TakeDamage(damage);
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
}
