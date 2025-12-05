using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Battle State")]
    private bool isBattleActive = false;
    private Champion currentChampion;
    private List<MonsterData> currentMonsters;
    private Room currentRoom;

    [Header("UI Sprites")]
    [SerializeField] private Sprite defenseShieldSprite; // Assign defense shield sprite in Inspector

    [Header("Battle Entities")]
    private BattleEntity championEntity;
    private List<BattleEntity> monsterEntities = new List<BattleEntity>();

    [Header("Turn System")]
    private List<BattleEntity> turnOrder = new List<BattleEntity>();
    private int currentTurnIndex = 0;

    public bool IsBattleActive => isBattleActive;
    public BattleEntity CurrentTurnEntity => turnOrder.Count > 0 && currentTurnIndex < turnOrder.Count ? turnOrder[currentTurnIndex] : null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartBattle(Champion champion, Room room)
    {
        if (isBattleActive)
        {
            Debug.LogWarning("Battle already in progress!");
            return;
        }

        if (room == null || !room.HasMonster)
        {
            Debug.LogWarning("Cannot start battle: No monsters in room!");
            return;
        }

        isBattleActive = true;
        currentChampion = champion;
        currentRoom = room;
        currentMonsters = new List<MonsterData>(room.PlacedMonsters);

        Debug.Log($"Battle started! {champion.Data.championName} vs {currentMonsters.Count} monsters in room {room.GridPosition}");

        // Show battle UI
        if (BattleUI.Instance != null)
        {
            BattleUI.Instance.ShowBattleBackground();
        }

        // Spawn battle entities
        StartCoroutine(SpawnBattleEntities());
    }

    private IEnumerator SpawnBattleEntities()
    {
        // Wait for fade/background animation
        yield return new WaitForSeconds(0.5f);

        // Clear any existing entities
        ClearBattleEntities();

        if (BattleUI.Instance == null)
        {
            Debug.LogError("BattleUI instance not found!");
            yield break;
        }

        // Spawn champion entity
        RectTransform championContainer = BattleUI.Instance.ChampionPositionContainer;
        if (championContainer != null)
        {
            GameObject championObj = new GameObject("ChampionEntity", typeof(RectTransform));
            championEntity = championObj.AddComponent<BattleEntity>();

            // Set defense shield sprite before initialization
            if (defenseShieldSprite != null)
            {
                championEntity.SetDefenseShieldSprite(defenseShieldSprite);
            }

            championEntity.InitializeFromChampion(currentChampion);

            // Set as child of position container
            RectTransform rectTransform = championObj.GetComponent<RectTransform>();
            rectTransform.SetParent(championContainer);
            rectTransform.localPosition = Vector3.zero;
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(100, 100);
            rectTransform.eulerAngles = new Vector3(0f, 180f, 0f);

            // Save original position after parent is set
            championEntity.SaveOriginalPosition();

            Debug.Log($"Champion spawned at {championContainer.position}");
        }
        else
        {
            Debug.LogError("Champion position container is not assigned in BattleUI!");
        }

        // Spawn monster entities
        RectTransform[] monsterContainers = BattleUI.Instance.MonsterPositionContainers;
        for (int i = 0; i < currentMonsters.Count; i++)
        {
            RectTransform containerTransform = GetMonsterPositionContainer(i, currentMonsters.Count, monsterContainers);

            if (containerTransform != null)
            {
                GameObject monsterObj = new GameObject($"MonsterEntity_{i}", typeof(RectTransform));
                BattleEntity monsterEntity = monsterObj.AddComponent<BattleEntity>();

                // Set defense shield sprite before initialization
                if (defenseShieldSprite != null)
                {
                    monsterEntity.SetDefenseShieldSprite(defenseShieldSprite);
                }

                monsterEntity.InitializeFromMonster(currentMonsters[i]);

                // Set as child of position container
                RectTransform rectTransform = monsterObj.GetComponent<RectTransform>();
                rectTransform.SetParent(containerTransform);
                rectTransform.localPosition = Vector3.zero;
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.sizeDelta = new Vector2(100, 100);

                // Save original position after parent is set
                monsterEntity.SaveOriginalPosition();

                monsterEntities.Add(monsterEntity);
                Debug.Log($"Monster {currentMonsters[i].monsterName} spawned at {containerTransform.position}");
            }
            else
            {
                Debug.LogError($"Monster position container {i} is not assigned in BattleUI!");
            }
        }

        Debug.Log($"Battle entities spawned: 1 champion vs {monsterEntities.Count} monsters");

        // Initialize turn order
        InitializeTurnOrder();
    }

    private RectTransform GetMonsterPositionContainer(int index, int totalCount, RectTransform[] containers)
    {
        switch (totalCount)
        {
            case 1:
                // Single monster in middle position (index 1)
                return containers[1];
            case 2:
                // Two monsters: top (0) and bottom (2)
                return (index == 0) ? containers[0] : containers[2];
            case 3:
                // Three monsters: use all positions
                return containers[index];
            default:
                return containers[1]; // Fallback to middle
        }
    }

    private void InitializeTurnOrder()
    {
        turnOrder.Clear();
        currentTurnIndex = 0;

        // Add all alive entities to turn order
        if (championEntity != null && championEntity.IsAlive)
        {
            turnOrder.Add(championEntity);
        }

        foreach (var monster in monsterEntities)
        {
            if (monster != null && monster.IsAlive)
            {
                turnOrder.Add(monster);
            }
        }

        // Sort by speed (descending - highest speed goes first)
        turnOrder.Sort((a, b) => b.Speed.CompareTo(a.Speed));

        Debug.Log("Turn order initialized:");
        for (int i = 0; i < turnOrder.Count; i++)
        {
            Debug.Log($"{i + 1}. {turnOrder[i].EntityName} (Speed: {turnOrder[i].Speed})");
        }

        // Start first turn
        if (turnOrder.Count > 0)
        {
            StartTurn();
        }
    }

    private void StartTurn()
    {
        BattleEntity currentEntity = CurrentTurnEntity;
        if (currentEntity == null || !currentEntity.IsAlive)
        {
            NextTurn();
            return;
        }

        Debug.Log($"=== {currentEntity.EntityName}'s turn (Speed: {currentEntity.Speed}) ===");

        // Champion uses AI, Monsters are controlled by player
        if (currentEntity.IsChampion)
        {
            StartCoroutine(ExecuteChampionTurn(currentEntity));
        }
        else
        {
            // TODO: Show monster action UI for player to choose
            Debug.Log("Waiting for player input for monster turn...");
        }
    }

    private IEnumerator ExecuteChampionTurn(BattleEntity champion)
    {
        // Wait a bit for visual clarity
        yield return new WaitForSeconds(0.5f);

        // Get alive monsters
        List<BattleEntity> aliveMonsters = GetAliveMonsters();

        if (aliveMonsters.Count == 0)
        {
            Debug.LogWarning("No alive monsters to attack!");
            yield return new WaitForSeconds(1.0f);
            NextTurn();
            yield break;
        }

        // Champion AI: Choose action with weighted probabilities
        // Basic Attack: 40%, Skill: 40%, Defend: 20%
        ChampionAction selectedAction = ChooseChampionAction(champion);

        switch (selectedAction)
        {
            case ChampionAction.BasicAttack:
                yield return ExecuteChampionAttack(champion, champion.BasicAttackSkill, aliveMonsters);
                break;

            case ChampionAction.Skill:
                SkillData selectedSkill = SelectUsableSkill(champion);
                if (selectedSkill != null)
                {
                    yield return ExecuteChampionAttack(champion, selectedSkill, aliveMonsters);
                }
                else
                {
                    // Fallback to basic attack if no usable skill
                    yield return ExecuteChampionAttack(champion, champion.BasicAttackSkill, aliveMonsters);
                }
                break;

            case ChampionAction.Defend:
                champion.StartDefending();
                yield return new WaitForSeconds(1.0f);
                break;
        }

        // Wait a bit before next turn
        yield return new WaitForSeconds(0.5f);

        NextTurn();
    }

    // Enum for champion actions
    private enum ChampionAction
    {
        BasicAttack,
        Skill,
        Defend
    }

    // Choose action based on weighted probabilities (4:4:2 ratio)
    private ChampionAction ChooseChampionAction(BattleEntity champion)
    {
        // Total weight: 10 (4 + 4 + 2)
        float randomValue = Random.Range(0f, 10f);

        if (randomValue < 4f)
        {
            // 0-4: Basic Attack (40%)
            return ChampionAction.BasicAttack;
        }
        else if (randomValue < 8f)
        {
            // 4-8: Skill (40%)
            return ChampionAction.Skill;
        }
        else
        {
            // 8-10: Defend (20%)
            return ChampionAction.Defend;
        }
    }

    // Select a random usable skill
    private SkillData SelectUsableSkill(BattleEntity champion)
    {
        if (champion.AvailableSkills == null || champion.AvailableSkills.Count == 0)
        {
            return null;
        }

        // Filter skills that can be used (have enough MP)
        List<SkillData> usableSkills = new List<SkillData>();
        foreach (var skill in champion.AvailableSkills)
        {
            if (champion.CanUseMP(skill.mpCost))
            {
                usableSkills.Add(skill);
            }
        }

        if (usableSkills.Count == 0)
        {
            return null;
        }

        // Return random usable skill
        return usableSkills[Random.Range(0, usableSkills.Count)];
    }

    // Execute attack or skill with animation
    private IEnumerator ExecuteChampionAttack(BattleEntity champion, SkillData skill, List<BattleEntity> aliveMonsters)
    {
        if (skill == null)
        {
            Debug.LogWarning($"{champion.EntityName} has no skill to use!");
            yield break;
        }

        // Select target only for single-target skills
        BattleEntity target = null;
        if (skill.targetType == SkillTarget.SingleEnemy || skill.targetType == SkillTarget.SingleAlly)
        {
            target = aliveMonsters[Random.Range(0, aliveMonsters.Count)];
        }

        yield return champion.StartCoroutine(champion.PerformSkillWithAnimation(skill, target, aliveMonsters));
    }

    private List<BattleEntity> GetAliveMonsters()
    {
        List<BattleEntity> aliveMonsters = new List<BattleEntity>();
        foreach (var monster in monsterEntities)
        {
            if (monster != null && monster.IsAlive)
            {
                aliveMonsters.Add(monster);
            }
        }
        return aliveMonsters;
    }

    public void NextTurn()
    {
        currentTurnIndex++;

        // If we've gone through all entities, start a new round
        if (currentTurnIndex >= turnOrder.Count)
        {
            currentTurnIndex = 0;
            Debug.Log("--- New Round ---");

            // Update defense status for all entities at the start of new round
            UpdateAllDefenseStatuses();
        }

        // Check battle end conditions
        if (CheckBattleEnd())
        {
            return;
        }

        // Skip dead entities
        BattleEntity currentEntity = CurrentTurnEntity;
        if (currentEntity != null && !currentEntity.IsAlive)
        {
            NextTurn();
            return;
        }

        StartTurn();
    }

    // Update defense status for all battle entities
    private void UpdateAllDefenseStatuses()
    {
        if (championEntity != null && championEntity.IsAlive)
        {
            championEntity.UpdateDefenseStatus();
        }

        foreach (var monster in monsterEntities)
        {
            if (monster != null && monster.IsAlive)
            {
                monster.UpdateDefenseStatus();
            }
        }
    }

    private bool CheckBattleEnd()
    {
        bool championAlive = championEntity != null && championEntity.IsAlive;
        bool anyMonsterAlive = false;

        foreach (var monster in monsterEntities)
        {
            if (monster != null && monster.IsAlive)
            {
                anyMonsterAlive = true;
                break;
            }
        }

        if (!championAlive)
        {
            Debug.Log("All champions defeated! Monsters win!");
            EndBattle(false);
            return true;
        }

        if (!anyMonsterAlive)
        {
            Debug.Log("All monsters defeated! Champion wins!");
            EndBattle(true);
            return true;
        }

        return false;
    }

    private void ClearBattleEntities()
    {
        if (championEntity != null)
        {
            Destroy(championEntity.gameObject);
            championEntity = null;
        }

        foreach (var entity in monsterEntities)
        {
            if (entity != null)
            {
                Destroy(entity.gameObject);
            }
        }
        monsterEntities.Clear();
    }

    public void EndBattle(bool championWon)
    {
        if (!isBattleActive)
        {
            Debug.LogWarning("No battle to end!");
            return;
        }

        Debug.Log($"Battle ended! Champion won: {championWon}");

        // Clear battle entities
        ClearBattleEntities();

        // Hide battle UI
        if (BattleUI.Instance != null)
        {
            BattleUI.Instance.HideBattleBackground();
        }

        // Clear monsters from room if champion won
        if (championWon && currentRoom != null)
        {
            currentRoom.RemoveAllMonsters();
        }

        isBattleActive = false;
        currentChampion = null;
        currentMonsters = null;
        currentRoom = null;

        // TODO: Resume exploration
    }
}
