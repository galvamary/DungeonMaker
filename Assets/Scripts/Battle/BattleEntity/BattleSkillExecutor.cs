using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Handles skill execution, effects, and damage calculation for battle entities
/// Manages skill animations, visual effects, and skill type implementations
/// </summary>
public class BattleSkillExecutor : MonoBehaviour
{
    private BattleEntity entity;
    private BattleEntityAnimator animator;
    private BattleEntityVisual visual;

    public void Initialize(BattleEntity battleEntity, BattleEntityAnimator entityAnimator, BattleEntityVisual entityVisual)
    {
        entity = battleEntity;
        animator = entityAnimator;
        visual = entityVisual;
    }

    /// <summary>
    /// Performs a skill with full animation and effects
    /// </summary>
    public IEnumerator PerformSkillWithAnimation(SkillData skill, BattleEntity target, List<BattleEntity> allTargets = null)
    {
        if (skill == null)
        {
            Debug.LogWarning($"{entity.EntityName} tried to use a null skill!");
            yield break;
        }

        if (!entity.CanUseMP(skill.mpCost))
        {
            Debug.LogWarning($"{entity.EntityName} doesn't have enough MP to use {skill.skillName}! (Required: {skill.mpCost}, Current: {entity.CurrentMP})");
            yield break;
        }

        entity.UseMP(skill.mpCost);
        Debug.Log($"{entity.EntityName} uses {skill.skillName}!");

        if (skill.mpCost > 0)
        {
        // Show skill name text
        ShowSkillNameText(skill.skillName);
        }

        // Play skill sound effect
        if (skill.soundEffect != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(skill.soundEffect);
        }

        // Attack animation - move towards target (only for single-target attack skills)
        bool movedForAttack = false;
        if (skill.skillType == SkillType.Attack && skill.targetType == SkillTarget.SingleEnemy && target != null && animator != null)
        {
            yield return animator.StartCoroutine(animator.MoveToAttackAnimation(target));
            movedForAttack = true;
        }

        // Show skill effect
        if (skill.effectPrefab != null)
        {
            ShowSkillEffect(skill, target, allTargets);
        }

        // Execute skill effect
        switch (skill.skillType)
        {
            case SkillType.Attack:
                ExecuteAttackSkill(skill, target, allTargets);
                break;
            case SkillType.Heal:
                ExecuteHealSkill(skill, target);
                break;
            default:
                Debug.LogWarning($"Skill type {skill.skillType} not implemented yet!");
                break;
        }

        // Wait a bit to see the impact
        yield return new WaitForSeconds(1.0f);

        // Return to original position (only if moved)
        if (movedForAttack && animator != null)
        {
            yield return animator.StartCoroutine(animator.ReturnToPosition());
        }
    }

    /// <summary>
    /// Shows skill visual effects based on target type
    /// </summary>
    private void ShowSkillEffect(SkillData skill, BattleEntity target, List<BattleEntity> allTargets)
    {
        if (skill.effectPrefab == null)
            return;

        switch (skill.targetType)
        {
            case SkillTarget.SingleEnemy:
            case SkillTarget.SingleAlly:
                if (target != null)
                {
                    BattleEntityVisual targetVisual = target.GetComponent<BattleEntityVisual>();
                    if (targetVisual != null && targetVisual.RectTransform != null)
                    {
                        SpawnEffect(skill.effectPrefab, targetVisual.RectTransform.position);
                    }
                }
                break;

            case SkillTarget.AllEnemies:
                if (allTargets != null)
                {
                    foreach (var enemy in allTargets)
                    {
                        if (enemy != null && enemy.IsAlive)
                        {
                            BattleEntityVisual enemyVisual = enemy.GetComponent<BattleEntityVisual>();
                            if (enemyVisual != null && enemyVisual.RectTransform != null)
                            {
                                SpawnEffect(skill.effectPrefab, enemyVisual.RectTransform.position);
                            }
                        }
                    }
                }
                break;

            case SkillTarget.Self:
                if (visual != null && visual.RectTransform != null)
                {
                    SpawnEffect(skill.effectPrefab, visual.RectTransform.position);
                }
                break;
        }
    }

    /// <summary>
    /// Spawns a skill effect at the specified position
    /// </summary>
    private void SpawnEffect(GameObject effectPrefab, Vector3 position)
    {
        GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity);

        // Set as child of same canvas for proper rendering
        if (visual != null && visual.RectTransform != null && visual.RectTransform.parent != null)
        {
            Canvas canvas = visual.RectTransform.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                effect.transform.SetParent(canvas.transform, true);
                effect.transform.position = position;
            }
        }

        // Rotate effect 180 degrees if used by a champion
        if (entity != null && entity.IsChampion)
        {
            effect.transform.eulerAngles = new Vector3(0f, 180f, 0f);
        }

        // Get animation length and destroy after it completes
        Animator effectAnimator = effect.GetComponent<Animator>();
        float destroyTime = 1.0f; // Default
        float animationSpeed = 2.0f; // Animation playback speed

        if (effectAnimator != null)
        {
            effectAnimator.speed = animationSpeed;

            if (effectAnimator.runtimeAnimatorController != null)
            {
                AnimationClip[] clips = effectAnimator.runtimeAnimatorController.animationClips;
                if (clips.Length > 0)
                {
                    destroyTime = clips[0].length / animationSpeed;
                }
            }
        }

        Destroy(effect, destroyTime);
    }

    /// <summary>
    /// Executes an attack skill with damage calculation
    /// </summary>
    private void ExecuteAttackSkill(SkillData skill, BattleEntity target, List<BattleEntity> allTargets)
    {
        switch (skill.targetType)
        {
            case SkillTarget.SingleEnemy:
                if (target != null && target.IsAlive)
                {
                    // Damage = skill power + attacker's attack - target's defense (minimum 1)
                    int totalDamage = skill.power + entity.Attack;
                    int damage = Mathf.Max(1, totalDamage - target.Defense);
                    target.TakeDamage(damage);
                }
                break;

            case SkillTarget.AllEnemies:
                if (allTargets != null)
                {
                    foreach (var enemy in allTargets)
                    {
                        if (enemy != null && enemy.IsAlive)
                        {
                            int totalDamage = skill.power + entity.Attack;
                            int damage = Mathf.Max(1, totalDamage - enemy.Defense);
                            enemy.TakeDamage(damage);
                        }
                    }
                }
                break;

            default:
                Debug.LogWarning($"Target type {skill.targetType} not supported for attack skills!");
                break;
        }
    }

    /// <summary>
    /// Executes a healing skill
    /// </summary>
    private void ExecuteHealSkill(SkillData skill, BattleEntity target)
    {
        switch (skill.targetType)
        {
            case SkillTarget.Self:
                entity.Heal(skill.power);
                break;

            case SkillTarget.SingleAlly:
                if (target != null && target.IsAlive)
                {
                    target.Heal(skill.power);
                }
                break;

            default:
                Debug.LogWarning($"Target type {skill.targetType} not supported for heal skills!");
                break;
        }
    }

    private static Coroutine skillNameCoroutine;

    private void ShowSkillNameText(string skillName)
    {
        if (BattleManager.Instance == null || BattleManager.Instance.SkillNameText == null) return;

        // 이전 페이드 중단
        if (skillNameCoroutine != null)
        {
            BattleManager.Instance.StopCoroutine(skillNameCoroutine);
        }

        TextMeshProUGUI tmp = BattleManager.Instance.SkillNameText;
        tmp.text = $"-{skillName}-";
        Color c = tmp.color;
        c.a = 1f;
        tmp.color = c;
        tmp.gameObject.SetActive(true);

        skillNameCoroutine = BattleManager.Instance.StartCoroutine(SkillNameFadeOut());
    }

    private static IEnumerator SkillNameFadeOut()
    {
        TextMeshProUGUI tmp = BattleManager.Instance.SkillNameText;

        // 유지 시간
        yield return new WaitForSeconds(1.2f);

        // 빠르게 페이드 아웃
        float fadeDuration = 0.2f;
        float elapsed = 0f;
        Color c = tmp.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            tmp.color = c;
            yield return null;
        }

        tmp.gameObject.SetActive(false);
        skillNameCoroutine = null;
    }
}
