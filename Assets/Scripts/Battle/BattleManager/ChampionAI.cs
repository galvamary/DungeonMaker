using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles champion AI decision-making and action execution
/// Uses weighted probability system for action selection (Basic: 40%, Skill: 40%, Defend: 20%)
/// </summary>
public class ChampionAI : MonoBehaviour
{
    // Action types for champion
    private enum ChampionAction
    {
        BasicAttack,
        Skill,
        Defend
    }

    [Header("AI Settings")]
    [Range(0f, 1f)] public float basicAttackWeight = 0.45f;  // 40%
    [Range(0f, 1f)] public float skillWeight = 0.45f;        // 40%
    [Range(0f, 1f)] public float defendWeight = 0.1f;       // 20%

    private BattleSetup battleSetup;

    public void Initialize(BattleSetup setup)
    {
        battleSetup = setup;
    }

    /// <summary>
    /// Executes a full champion turn with AI decision making
    /// </summary>
    public IEnumerator ExecuteTurn(BattleEntity champion)
    {
        if (champion == null || !champion.IsAlive)
        {
            Debug.LogWarning("Champion is null or dead!");
            yield break;
        }

        // Wait a bit for visual clarity
        yield return new WaitForSeconds(0.5f);

        // Get alive monsters
        List<BattleEntity> aliveMonsters = battleSetup?.GetAliveMonsters();

        if (aliveMonsters == null || aliveMonsters.Count == 0)
        {
            Debug.LogWarning("No alive monsters to attack!");
            yield return new WaitForSeconds(1.0f);
            yield break;
        }

        // Choose action based on weighted probabilities
        ChampionAction selectedAction = ChooseAction(champion);

        // Execute chosen action
        switch (selectedAction)
        {
            case ChampionAction.BasicAttack:
                yield return ExecuteAttack(champion, champion.BasicAttackSkill, aliveMonsters);
                break;

            case ChampionAction.Skill:
                SkillData selectedSkill = SelectUsableSkill(champion);
                if (selectedSkill != null)
                {
                    yield return ExecuteAttack(champion, selectedSkill, aliveMonsters);
                }
                else
                {
                    // Fallback to basic attack if no usable skill
                    Debug.Log($"{champion.EntityName} has no usable skills. Using basic attack instead.");
                    yield return ExecuteAttack(champion, champion.BasicAttackSkill, aliveMonsters);
                }
                break;

            case ChampionAction.Defend:
                yield return ExecuteDefend(champion);
                break;
        }

        // Wait a bit before ending turn
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// Chooses an action based on weighted probabilities
    /// Default: Basic Attack 40%, Skill 40%, Defend 20%
    /// </summary>
    private ChampionAction ChooseAction(BattleEntity champion)
    {
        // Normalize weights to ensure they sum to 1.0
        float totalWeight = basicAttackWeight + skillWeight + defendWeight;
        float normalizedBasicAttack = basicAttackWeight / totalWeight;
        float normalizedSkill = skillWeight / totalWeight;
        float normalizedDefend = defendWeight / totalWeight;

        float randomValue = Random.Range(0f, 1f);

        if (randomValue < normalizedBasicAttack)
        {
            return ChampionAction.BasicAttack;
        }
        else if (randomValue < normalizedBasicAttack + normalizedSkill)
        {
            return ChampionAction.Skill;
        }
        else
        {
            return ChampionAction.Defend;
        }
    }

    /// <summary>
    /// Selects a random usable skill from champion's available skills
    /// </summary>
    private SkillData SelectUsableSkill(BattleEntity champion)
    {
        if (champion.AvailableSkills == null || champion.AvailableSkills.Count == 0)
        {
            return null;
        }

        // Filter skills that can be used (have enough MP and meet conditions)
        List<SkillData> usableSkills = new List<SkillData>();
        foreach (var skill in champion.AvailableSkills)
        {
            if (!champion.CanUseMP(skill.mpCost))
            {
                continue; // Not enough MP
            }

            // Check if heal skill should be used
            if (skill.skillType == SkillType.Heal)
            {
                int missingHP = champion.MaxHealth - champion.CurrentHealth;
                if (missingHP < skill.power)
                {
                    // Don't use heal if missing HP is less than heal amount
                    Debug.Log($"Skipping heal skill: missing HP ({missingHP}) < heal power ({skill.power})");
                    continue;
                }
            }

            usableSkills.Add(skill);
        }

        if (usableSkills.Count == 0)
        {
            return null;
        }

        // Return random usable skill
        return usableSkills[Random.Range(0, usableSkills.Count)];
    }

    /// <summary>
    /// Executes an attack or skill with animation
    /// </summary>
    private IEnumerator ExecuteAttack(BattleEntity champion, SkillData skill, List<BattleEntity> aliveMonsters)
    {
        if (skill == null)
        {
            Debug.LogWarning($"{champion.EntityName} has no skill to use!");
            yield break;
        }

        // Select target based on skill type
        BattleEntity target = SelectTarget(skill, aliveMonsters);

        // Execute skill with animation
        yield return champion.StartCoroutine(champion.PerformSkillWithAnimation(skill, target, aliveMonsters));
    }

    /// <summary>
    /// Selects a target for the skill
    /// </summary>
    private BattleEntity SelectTarget(SkillData skill, List<BattleEntity> aliveMonsters)
    {
        // Only select target for single-target skills
        if (skill.targetType == SkillTarget.SingleEnemy || skill.targetType == SkillTarget.SingleAlly)
        {
            if (aliveMonsters.Count == 0)
            {
                return null;
            }

            // Simple AI: random target
            // TODO: Add smarter targeting (lowest HP, highest threat, etc.)
            return aliveMonsters[Random.Range(0, aliveMonsters.Count)];
        }

        return null;
    }

    /// <summary>
    /// Executes defend action
    /// </summary>
    private IEnumerator ExecuteDefend(BattleEntity champion)
    {
        champion.StartDefending();
        yield return new WaitForSeconds(1.0f);
    }

    /// <summary>
    /// Sets custom action weights (for testing or difficulty adjustment)
    /// </summary>
    public void SetActionWeights(float basicAttack, float skill, float defend)
    {
        basicAttackWeight = Mathf.Clamp01(basicAttack);
        skillWeight = Mathf.Clamp01(skill);
        defendWeight = Mathf.Clamp01(defend);
    }
}
