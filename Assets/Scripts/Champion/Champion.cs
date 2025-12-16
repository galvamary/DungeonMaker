using UnityEngine;
using System.Collections;

public class Champion : MonoBehaviour
{
    [Header("Champion Data")]
    [SerializeField] private ChampionData championData;

    [Header("Current Stats")]
    private int currentHealth;
    private int currentMP;
    private SpriteRenderer spriteRenderer;
    private Room currentRoom;

    public ChampionData Data => championData;
    public int CurrentHealth => currentHealth;
    public int CurrentMP => currentMP;
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
        currentMP = data.maxMP;

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
        transform.localScale = Vector3.one * 2.5f;

        gameObject.name = $"Champion_{data.championName}";
    }

    /// <summary>
    /// Updates champion's current HP and MP from battle entity
    /// </summary>
    public void UpdateStatsFromBattle(int newHealth, int newMP)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, championData.maxHealth);
        currentMP = Mathf.Clamp(newMP, 0, championData.maxMP);

        Debug.Log($"{championData.championName} stats updated - HP: {currentHealth}/{championData.maxHealth}, MP: {currentMP}/{championData.maxMP}");
    }

    public void MoveToRoom(Room newRoom)
    {
        if (newRoom == null) return;

        StartCoroutine(MoveToRoomWithFade(newRoom));
    }

    private IEnumerator MoveToRoomWithFade(Room newRoom)
    {
        // Fade out (darken screen)
        if (FadeEffect.Instance != null)
        {
            yield return FadeEffect.Instance.FadeOut();
        }

        // Move champion and camera during dark screen
        currentRoom = newRoom;
        transform.position = newRoom.transform.position;

        CameraController cameraController = FindFirstObjectByType<CameraController>();
        if (cameraController != null)
        {
            cameraController.FocusOnPosition(transform.position);
        }

        Debug.Log($"{championData.championName} moved to room at {newRoom.GridPosition}");

        // Wait while screen is completely black
        yield return new WaitForSeconds(0.5f);

        // Fade in (brighten screen)
        if (FadeEffect.Instance != null)
        {
            yield return FadeEffect.Instance.FadeIn();
        }
    }

    private void Die()
    {
        Debug.Log($"{championData.championName} has been defeated!");

        // TODO: Drop loot, play death animation, etc.

        Destroy(gameObject);
    }
}
