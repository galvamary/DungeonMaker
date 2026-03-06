using UnityEngine;

/// <summary>
/// 피로도 이펙트 - Animator 제어 래퍼
/// 탐험(World Space Canvas)과 전투(UI) 양쪽에서 재사용
/// </summary>
public class FatigueEffect : MonoBehaviour
{
    private Animator animator;

    public void SetAnimator(Animator anim)
    {
        animator = anim;
    }

    public void Show(float speedMultiplier = 1f)
    {
        gameObject.SetActive(true);
        if (animator != null)
        {
            animator.speed = speedMultiplier;
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
