using UnityEngine;

public class ShopMonsterPanel : MonoBehaviour
{
    private MonsterData monsterData;

    public void SetMonsterData(MonsterData data)
    {
        monsterData = data;
    }

    public void ShowInfo()
    {
        if (monsterData != null && MonsterTooltip.Instance != null)
        {
            RectTransform rect = GetComponent<RectTransform>();
            MonsterTooltip.Instance.ToggleTooltip(monsterData, rect);
        }
    }
}
