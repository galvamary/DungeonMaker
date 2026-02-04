using UnityEngine;
using System.Collections;

/// <summary>
/// Handles all battle animations for entities
/// Manages movement animations, position tracking, and return animations
/// </summary>
public class BattleEntityAnimator : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector3 originalPosition;
    private bool isChampion;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(bool championType)
    {
        isChampion = championType;
    }

    /// <summary>
    /// Saves the original position of this entity
    /// </summary>
    public void SaveOriginalPosition()
    {
        if (rectTransform != null)
        {
            originalPosition = rectTransform.position;
            Debug.Log($"[{gameObject.name}] SaveOriginalPosition: {originalPosition}");
        }
    }

    /// <summary>
    /// Animates movement towards target for attack
    /// </summary>
    public IEnumerator MoveToAttackAnimation(BattleEntity target)
    {
        BattleEntityVisual targetVisual = target.GetComponent<BattleEntityVisual>();
        if (targetVisual == null || targetVisual.RectTransform == null)
        {
            Debug.LogError($"Target {target.EntityName} has no RectTransform!");
            yield break;
        }

        Vector3 startPos = rectTransform.position;
        Vector3 targetPos = targetVisual.RectTransform.position;

        // Calculate attack position with offset
        // Champion attacks from left (positive offset), Monster from right (negative offset)
        float xOffset = isChampion ? 400f : -400f;
        Vector3 attackPos = new Vector3(targetPos.x + xOffset, targetPos.y, targetPos.z);

        Debug.Log($"Attack Animation - Start: {startPos}, Target: {targetPos}, AttackPos: {attackPos}");

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rectTransform.position = Vector3.Lerp(startPos, attackPos, t);
            yield return null;
        }

        rectTransform.position = attackPos;
    }

    /// <summary>
    /// Animates return to original position
    /// </summary>
    public IEnumerator ReturnToPosition()
    {
        Vector3 startPos = rectTransform.position;
        Debug.Log($"[{gameObject.name}] ReturnToPosition - From: {startPos}, To: {originalPosition}");

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rectTransform.position = Vector3.Lerp(startPos, originalPosition, t);
            yield return null;
        }

        rectTransform.position = originalPosition;
        Debug.Log($"[{gameObject.name}] Return complete. Final position: {rectTransform.position}");
    }

    /// <summary>
    /// Animates a slight movement to the right when monster's turn starts
    /// </summary>
    public IEnumerator MoveTurnStart()
    {
        if (isChampion)
        {
            yield break; // Champions don't move on turn start
        }

        Vector3 startPos = originalPosition;
        Vector3 targetPos = originalPosition + new Vector3(250f, 0f, 0f); // Move 50 pixels to the right

        float duration = 0.15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rectTransform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        rectTransform.position = targetPos;
    }

    /// <summary>
    /// Animates return to original position when monster's turn ends
    /// </summary>
    public IEnumerator MoveTurnEnd()
    {
        if (isChampion)
        {
            yield break; // Champions don't need turn end movement
        }

        Vector3 startPos = rectTransform.position;

        float duration = 0.15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rectTransform.position = Vector3.Lerp(startPos, originalPosition, t);
            yield return null;
        }

        rectTransform.position = originalPosition;
    }

    public RectTransform RectTransform => rectTransform;
}
