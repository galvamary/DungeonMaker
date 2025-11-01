using UnityEngine;

public class Champion : MonoBehaviour
{
    [Header("Champion Data")]
    [SerializeField] private ChampionData championData;

    [Header("Current Stats")]
    private int currentHealth;
    private int attack;
    private int defense;

    private SpriteRenderer spriteRenderer;
    private Room currentRoom;

    public ChampionData Data => championData;
    public int CurrentHealth => currentHealth;
    public int Attack => attack;
    public int Defense => defense;
    public Room CurrentRoom => currentRoom;
    public bool IsAlive => currentHealth > 0;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
    }

    public void Initialize(ChampionData data, Room startRoom)
    {
        championData = data;
        currentRoom = startRoom;

        // Initialize stats
        currentHealth = data.maxHealth;
        attack = data.attack;
        defense = data.defense;

        // Set visual
        if (spriteRenderer != null && data.icon != null)
        {
            spriteRenderer.sprite = data.icon;
            spriteRenderer.sortingOrder = 3; // Above monsters
        }

        // Position at room center
        if (currentRoom != null)
        {
            transform.position = currentRoom.transform.position;
        }

        // Scale the champion
        transform.localScale = Vector3.one * 0.2f;

        gameObject.name = $"Champion_{data.championName}";
    }

    public void MoveToRoom(Room newRoom)
    {
        if (newRoom == null) return;

        currentRoom = newRoom;
        transform.position = newRoom.transform.position;

        Debug.Log($"{championData.championName} moved to room at {newRoom.GridPosition}");
    }

    private void Die()
    {
        Debug.Log($"{championData.championName} has been defeated!");

        // TODO: Drop loot, play death animation, etc.

        Destroy(gameObject);
    }
}
