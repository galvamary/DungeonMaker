using UnityEngine;

public class BattleEntity : MonoBehaviour
{
    [Header("Entity Data")]
    private string entityName;
    private Sprite entitySprite;
    private int currentHealth;
    private int maxHealth;
    private int attack;
    private int defense;
    private bool isChampion;

    [Header("Visual")]
    private SpriteRenderer spriteRenderer;

    public string EntityName => entityName;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int Attack => attack;
    public int Defense => defense;
    public bool IsChampion => isChampion;
    public bool IsAlive => currentHealth > 0;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
    }

    public void InitializeFromChampion(Champion champion)
    {
        entityName = champion.Data.championName;
        entitySprite = champion.Data.icon;
        currentHealth = champion.CurrentHealth;
        maxHealth = champion.Data.maxHealth;
        attack = champion.Attack;
        defense = champion.Defense;
        isChampion = true;

        SetupVisual();
    }

    public void InitializeFromMonster(MonsterData monster)
    {
        entityName = monster.monsterName;
        entitySprite = monster.icon;
        currentHealth = monster.health;
        maxHealth = monster.health;
        attack = monster.attack;
        defense = monster.defense;
        isChampion = false;

        SetupVisual();
    }

    private void SetupVisual()
    {
        if (spriteRenderer != null && entitySprite != null)
        {
            spriteRenderer.sprite = entitySprite;
            spriteRenderer.sortingOrder = 10; // High order to be above battle background
        }

        // Scale based on type
        transform.localScale = Vector3.one * (isChampion ? 1.2f : 1.0f);

        gameObject.name = $"BattleEntity_{entityName}";
    }

    public void TakeDamage(int damage)
    {
        int actualDamage = Mathf.Max(1, damage - defense);
        currentHealth -= actualDamage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($"{entityName} took {actualDamage} damage! Remaining health: {currentHealth}/{maxHealth}");

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

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }
}
