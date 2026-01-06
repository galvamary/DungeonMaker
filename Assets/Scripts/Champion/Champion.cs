using UnityEngine;
using System.Collections;

public class Champion : MonoBehaviour
{
    [Header("Champion Data")]
    [SerializeField] private  ChampionData championData;

    [Header("Current Stats")]
    private int currentHealth;
    private int currentMP;
    private float currentFatigue = 0f;  // 피로도 (0 ~ 100)
    private SpriteRenderer spriteRenderer;
    private Room currentRoom;

    [Header("Fatigue Settings")]
    [SerializeField] private float fatiguePerRoom = 7f;  // 방 이동 시 증가하는 피로도
    [SerializeField] private float maxFatigue = 100f;    // 최대 피로도
    [SerializeField] private float fatigueStatPenalty = 0.5f;  // 최대 피로도일 때 스탯 감소율 (50%)

    public ChampionData Data => championData;
    public int CurrentHealth => currentHealth;
    public int CurrentMP => currentMP;
    public float CurrentFatigue => currentFatigue;
    public Room CurrentRoom => currentRoom;
    public bool IsAlive => currentHealth > 0;

    // 피로도로 감소된 실제 스탯 계산
    public int EffectiveAttack => CalculateReducedStat(championData.attack);
    public int EffectiveDefense => CalculateReducedStat(championData.defense);
    public int EffectiveSpeed => CalculateReducedStat(championData.speed);

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
        currentFatigue = 0f;  // 초기 피로도는 0

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
    /// 피로도에 따라 감소된 스탯 계산
    /// 피로도 0% = 스탯 100%, 피로도 100% = 스탯 50%
    /// </summary>
    private int CalculateReducedStat(int baseStat)
    {
        // 피로도 비율 계산 (0.0 ~ 1.0)
        float fatigueRatio = Mathf.Clamp01(currentFatigue / maxFatigue);

        // 스탯 감소율 계산 (피로도에 비례해서 최대 fatigueStatPenalty만큼 감소)
        float reductionMultiplier = 1f - (fatigueRatio * fatigueStatPenalty);

        // 최소 1은 보장
        return Mathf.Max(1, Mathf.RoundToInt(baseStat * reductionMultiplier));
    }

    /// <summary>
    /// 피로도 증가
    /// </summary>
    public void AddFatigue(float amount)
    {
        float oldFatigue = currentFatigue;
        currentFatigue = Mathf.Clamp(currentFatigue + amount, 0f, maxFatigue);

        Debug.Log($"{championData.championName} fatigue increased: {oldFatigue:F1} -> {currentFatigue:F1} " +
                  $"(Attack: {EffectiveAttack}, Defense: {EffectiveDefense}, Speed: {EffectiveSpeed})");
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

        // 방 이동 시 피로도 증가
        AddFatigue(fatiguePerRoom);

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
