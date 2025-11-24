using UnityEngine;
using UnityEngine.UI;

public class BattleEntity : MonoBehaviour
{
    [Header("Entity Data")]
    private string entityName;
    private Sprite entitySprite;
    private int currentHealth;
    private int maxHealth;
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
        attack = champion.Attack;
        defense = champion.Defense;
        speed = champion.Data.speed;
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
}
