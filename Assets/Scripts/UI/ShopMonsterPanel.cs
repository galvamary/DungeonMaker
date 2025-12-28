using UnityEngine;
using UnityEngine.EventSystems;

public class ShopMonsterPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    private MonsterData monsterData;

    public void SetMonsterData(MonsterData data)
    {
        monsterData = data;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (monsterData != null && MonsterTooltip.Instance != null)
        {
            MonsterTooltip.Instance.ShowTooltip(monsterData, eventData.position);
        }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (monsterData != null && MonsterTooltip.Instance != null)
        {
            MonsterTooltip.Instance.UpdateTooltipPosition(eventData.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (MonsterTooltip.Instance != null)
        {
            MonsterTooltip.Instance.HideTooltip();
        }
    }
}
