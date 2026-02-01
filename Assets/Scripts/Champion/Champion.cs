using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    private Sprite currentSprite; // Current sprite based on fatigue

    [Header("Skills")]
    private List<SkillData> availableSkills = new List<SkillData>(); // Filtered skills based on treasure room count

    [Header("Fatigue Settings")]
    [SerializeField] private float fatiguePerRoom = 7f;  // 방 이동 시 증가하는 피로도
    [SerializeField] private float maxFatigue = 100f;    // 최대 피로도
    [SerializeField] private float fatigueStatPenalty = 0.4f;  // 최대 피로도일 때 스탯 감소율 (50%)

    public ChampionData Data => championData;
    public int CurrentHealth => currentHealth;
    public int CurrentMP => currentMP;
    public float CurrentFatigue => currentFatigue;
    public float FatiguePerRoom => fatiguePerRoom;
    public Room CurrentRoom => currentRoom;
    public bool IsAlive => currentHealth > 0;
    public List<SkillData> AvailableSkills => availableSkills;
    public Sprite CurrentSprite => currentSprite;

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

        // Get reputation for stat scaling
        int reputation = GameManager.Instance?.CurrentReputation ?? 1;
        reputation = Mathf.Max(1, reputation); // Minimum 1

        // Initialize stats with reputation scaling
        // HP and MP are multiplied by reputation
        currentHealth = data.maxHealth * reputation;
        currentMP = data.maxMP * reputation;
        currentFatigue = 0f;  // 초기 피로도는 0

        // Adjust fatigue rate: 8 - n, minimum 1
        fatiguePerRoom = Mathf.Max(1f, 8f - reputation);

        // Filter skills based on reputation
        availableSkills.Clear();
        if (data.skills != null)
        {
            foreach (SkillData skill in data.skills)
            {
                if (skill != null && reputation >= skill.requiredTreasureRooms)
                {
                    availableSkills.Add(skill);
                    Debug.Log($"Unlocked skill: {skill.skillName} (requires reputation {skill.requiredTreasureRooms})");
                }
                else if (skill != null)
                {
                    Debug.Log($"Locked skill: {skill.skillName} (requires reputation {skill.requiredTreasureRooms}, current: {reputation})");
                }
            }
        }

        Debug.Log($"Champion initialized with reputation {reputation}: " +
                  $"HP={currentHealth}, MP={currentMP}, FatigueRate={fatiguePerRoom}, Skills={availableSkills.Count}");

        // Set initial sprite based on fatigue (should be normal at start)
        UpdateSpriteByFatigue();

        // Set visual
        if (spriteRenderer != null)
        {
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

        // Update sprite based on fatigue
        UpdateSpriteByFatigue();
    }

    /// <summary>
    /// Updates champion sprite based on current fatigue level
    /// </summary>
    private void UpdateSpriteByFatigue()
    {
        if (championData == null)
        {
            return;
        }

        float fatiguePercentage = (currentFatigue / maxFatigue) * 100f;

        Sprite newSprite;

        if (fatiguePercentage < 50f)
        {
            // Normal state (0-50%)
            newSprite = championData.normalSprite;
        }
        else if (fatiguePercentage < 90f)
        {
            // Tired state (50-90%)
            newSprite = championData.tiredSprite;
        }
        else
        {
            // Exhausted state (90-100%)
            newSprite = championData.exhaustedSprite;
        }

        // Store current sprite
        if (newSprite != currentSprite)
        {
            currentSprite = newSprite;

            // Update sprite renderer if available
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = newSprite;
            }

            Debug.Log($"{championData.championName} sprite changed (Fatigue: {fatiguePercentage:F1}%)");
        }
    }

    /// <summary>
    /// Updates champion's current HP and MP from battle entity
    /// </summary>
    public void UpdateStatsFromBattle(int newHealth, int newMP)
    {
        int reputation = GameManager.Instance != null ? GameManager.Instance.CurrentReputation : 1;
        currentHealth = Mathf.Clamp(newHealth, 0, championData.maxHealth * reputation);
        currentMP = Mathf.Clamp(newMP, 0, championData.maxMP * reputation);

        Debug.Log($"{championData.championName} stats updated - HP: {currentHealth}/{championData.maxHealth * reputation}, MP: {currentMP}/{championData.maxMP * reputation}");
    }

    public IEnumerator MoveToRoom(Room newRoom, float speedMultiplier = 1f)
    {
        if (newRoom == null) yield break;

        // 방 이동 시 피로도 증가
        AddFatigue(fatiguePerRoom);

        yield return MoveToRoomWithFade(newRoom, speedMultiplier);
    }

    private IEnumerator MoveToRoomWithFade(Room newRoom, float speedMultiplier = 1f)
    {
        // Fade out (darken screen)
        if (FadeEffect.Instance != null)
        {
            yield return FadeEffect.Instance.FadeOut(speedMultiplier);
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

        // Wait while screen is completely black (scaled by speed)
        yield return new WaitForSeconds(0.5f / speedMultiplier);

        // Fade in (brighten screen)
        if (FadeEffect.Instance != null)
        {
            yield return FadeEffect.Instance.FadeIn(speedMultiplier);
        }
    }

    private void Die()
    {
        Debug.Log($"{championData.championName} has been defeated!");

        // TODO: Drop loot, play death animation, etc.

        Destroy(gameObject);
    }
}
