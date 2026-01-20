using UnityEngine;
using System.Collections;

/// <summary>
/// Handles monster turn execution and player input
/// Shows action panel UI and waits for player selection
/// </summary>
public class MonsterController : MonoBehaviour
{
    private BattleSetup battleSetup;
    private MonsterActionPanel actionPanel;
    private BattleEntity currentMonster;
    private bool isWaitingForInput = false;
    private MonsterAction selectedAction = MonsterAction.None;
    private SkillData selectedSkill = null; // Store selected skill
    private BattleEntity selectedAllyTarget = null; // Store selected ally target

    private enum MonsterAction
    {
        None,
        BasicAttack,
        Skill,
        Defend
    }

    public void Initialize(BattleSetup setup, MonsterActionPanel panel)
    {
        battleSetup = setup;
        actionPanel = panel;

        // Subscribe to UI events
        if (actionPanel == null)
        {
            Debug.LogError("MonsterActionPanel not assigned!");
        }
        else
        {
            actionPanel.OnBasicAttackClicked += HandleBasicAttack;
            actionPanel.OnSkillSelected += HandleSkillSelected;
            actionPanel.OnSkillWithTargetSelected += HandleSkillWithTargetSelected;
            actionPanel.OnDefendClicked += HandleDefend;
        }
    }

    /// <summary>
    /// Executes a monster turn (waits for player input via UI)
    /// </summary>
    public IEnumerator ExecuteTurn(BattleEntity monster)
    {
        if (monster == null || !monster.IsAlive)
        {
            Debug.LogWarning("Monster is null or dead!");
            yield break;
        }

        currentMonster = monster;
        selectedAction = MonsterAction.None;
        selectedSkill = null;
        selectedAllyTarget = null;
        isWaitingForInput = true;
        

        Debug.Log($"Waiting for player input for {monster.EntityName}...");

        // Show action panel
        if (actionPanel != null)
        {
            actionPanel.Show(monster);
        }
        else
        {
            Debug.LogError("MonsterActionPanel is null! Cannot show UI.");
            yield break;
        }

        // Wait for player to select an action
        while (isWaitingForInput)
        {
            yield return null;
        }

        // Execute selected action
        yield return ExecuteSelectedAction();

        currentMonster = null;
        selectedSkill = null;
    }

    /// <summary>
    /// Executes the action selected by player
    /// </summary>
    private IEnumerator ExecuteSelectedAction()
    {
        if (currentMonster == null) yield break;

        switch (selectedAction)
        {
            case MonsterAction.BasicAttack:
                yield return ExecuteMonsterAttack(currentMonster, currentMonster.BasicAttackSkill);
                break;

            case MonsterAction.Skill:
                if (selectedSkill != null)
                {
                    yield return ExecuteMonsterAttack(currentMonster, selectedSkill);
                }
                else
                {
                    Debug.LogWarning("No skill selected!");
                }
                break;

            case MonsterAction.Defend:
                yield return ExecuteMonsterDefend(currentMonster);
                break;

            default:
                Debug.LogWarning("No action selected!");
                break;
        }
    }

    /// <summary>
    /// Handles basic attack button click
    /// </summary>
    private void HandleBasicAttack()
    {
        selectedAction = MonsterAction.BasicAttack;
        isWaitingForInput = false;
    }

    /// <summary>
    /// Handles skill selection from skill panel
    /// </summary>
    private void HandleSkillSelected(SkillData skill)
    {
        selectedAction = MonsterAction.Skill;
        selectedSkill = skill;
        selectedAllyTarget = null; // No specific target selected
        isWaitingForInput = false;
    }

    /// <summary>
    /// Handles skill selection with specific ally target
    /// </summary>
    private void HandleSkillWithTargetSelected(SkillData skill, BattleEntity allyTarget)
    {
        selectedAction = MonsterAction.Skill;
        selectedSkill = skill;
        selectedAllyTarget = allyTarget;
        isWaitingForInput = false;
    }

    /// <summary>
    /// Handles defend button click
    /// </summary>
    private void HandleDefend()
    {
        selectedAction = MonsterAction.Defend;
        isWaitingForInput = false;
    }

    /// <summary>
    /// Executes monster attack with a skill
    /// </summary>
    private IEnumerator ExecuteMonsterAttack(BattleEntity monster, SkillData skill)
    {
        if (skill == null)
        {
            Debug.LogWarning($"{monster.EntityName} has no skill to use!");
            yield break;
        }

        // Select appropriate target based on skill target type
        BattleEntity target = null;
        System.Collections.Generic.List<BattleEntity> allTargets = null;

        switch (skill.targetType)
        {
            case SkillTarget.SingleEnemy:
                // Target the champion
                target = battleSetup?.ChampionEntity;
                if (target == null || !target.IsAlive)
                {
                    Debug.LogWarning("No alive champion to target!");
                    yield break;
                }
                break;

            case SkillTarget.AllEnemies:
                // Target all enemies (currently only the champion)
                target = battleSetup?.ChampionEntity;
                if (target != null && target.IsAlive)
                {
                    allTargets = new System.Collections.Generic.List<BattleEntity> { target };
                }
                else
                {
                    Debug.LogWarning("No alive champion to target!");
                    yield break;
                }
                break;

            case SkillTarget.Self:
                // No target needed (caster heals/buffs themselves)
                target = null;
                break;

            case SkillTarget.SingleAlly:
                // Use player-selected ally target if available, otherwise select random
                if (selectedAllyTarget != null && selectedAllyTarget.IsAlive)
                {
                    target = selectedAllyTarget;
                    Debug.Log($"Using player-selected ally target: {target.EntityName}");
                }
                else
                {
                
                    Debug.LogWarning($"{monster.EntityName} has no alive allies to target with {skill.skillName}!");
                    yield break;
                
                }
                break;

            case SkillTarget.AllAllies:
                // Target all ally monsters
                allTargets = GetAllAllyMonsters();
                if (allTargets == null || allTargets.Count == 0)
                {
                    Debug.LogWarning($"{monster.EntityName} has no alive allies to target with {skill.skillName}!");
                    yield break;
                }
                break;

            default:
                Debug.LogWarning($"Unsupported skill target type: {skill.targetType}");
                yield break;
        }

        // Execute skill with animation
        yield return monster.StartCoroutine(monster.PerformSkillWithAnimation(skill, target, allTargets));
    }

    /// <summary>
    /// Executes defend action for a monster
    /// </summary>
    private IEnumerator ExecuteMonsterDefend(BattleEntity monster)
    {
        if (monster == null || !monster.IsAlive)
        {
            Debug.LogWarning("Monster is null or dead!");
            yield break;
        }

        monster.StartDefending();
        yield return new WaitForSeconds(1.0f);
    }

    private System.Collections.Generic.List<BattleEntity> GetAllAllyMonsters()
    {
        System.Collections.Generic.List<BattleEntity> aliveAllies = new System.Collections.Generic.List<BattleEntity>();

        if (battleSetup == null || battleSetup.MonsterEntities == null)
        {
            return aliveAllies;
        }

        foreach (var monster in battleSetup.MonsterEntities)
        {
            if (monster != null && monster.IsAlive)
            {
                aliveAllies.Add(monster);
            }
        }

        return aliveAllies;
    }

    private void OnDestroy()
    {
        // Unsubscribe from UI events
        if (actionPanel != null)
        {
            actionPanel.OnBasicAttackClicked -= HandleBasicAttack;
            actionPanel.OnSkillSelected -= HandleSkillSelected;
            actionPanel.OnSkillWithTargetSelected -= HandleSkillWithTargetSelected;
            actionPanel.OnDefendClicked -= HandleDefend;
        }
    }
}
