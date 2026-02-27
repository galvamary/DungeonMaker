using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterTooltip : MonoBehaviour
{
    public static MonsterTooltip Instance { get; private set; }

    [Header("패널")]
    [SerializeField] private GameObject tooltipPanel;

    [Header("기본 정보")]
    [SerializeField] private TextMeshProUGUI monsterNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("스탯")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI mpText;
    [SerializeField] private TextMeshProUGUI atkText;
    [SerializeField] private TextMeshProUGUI defText;
    [SerializeField] private TextMeshProUGUI spdText;

    [Header("스킬")]
    [SerializeField] private TextMeshProUGUI skillsText;

    private RectTransform tooltipRect;
    private MonsterData currentMonster;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        HideTooltip();
    }

    public void ToggleTooltip(MonsterData monster, RectTransform anchorRect)
    {
        if (monster == null || tooltipPanel == null) return;

        // 같은 몬스터를 다시 누르면 닫기
        if (tooltipPanel.activeSelf && currentMonster == monster)
        {
            HideTooltip();
            return;
        }

        // 정보 채우기
        currentMonster = monster;

        if (monsterNameText != null)
            monsterNameText.text = monster.monsterName;

        if (descriptionText != null)
            descriptionText.text = monster.description;

        if (hpText != null) hpText.text = monster.maxHealth.ToString();
        if (mpText != null) mpText.text = monster.maxMP.ToString();
        if (atkText != null) atkText.text = monster.attack.ToString();
        if (defText != null) defText.text = monster.defense.ToString();
        if (spdText != null) spdText.text = monster.speed.ToString();

        if (skillsText != null)
        {
            string skillInfo = "";

            foreach (var skill in monster.skills)
            {
                if (skill != null)
                {
                    if (skillInfo.Length > 0) skillInfo += "\n";
                    skillInfo += FormatSkill(skill);
                }
            }

            if (skillInfo == "")
            {
                skillInfo = "No Skill";
            }

            skillsText.text = skillInfo;
        }

        // ShopMonsterPanel 오른쪽에 위치시키기
        PositionNextTo(anchorRect);
        tooltipPanel.SetActive(true);
    }

    private void PositionNextTo(RectTransform anchorRect)
    {
        if (tooltipRect == null || anchorRect == null) return;

        // 앵커의 월드 좌표 → 툴팁 부모의 로컬 좌표로 변환
        Vector3 anchorWorldPos = anchorRect.position;
        RectTransform parentRect = tooltipRect.parent as RectTransform;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            RectTransformUtility.WorldToScreenPoint(null, anchorWorldPos),
            null,
            out localPoint
        );

        float anchorHalfWidth = anchorRect.rect.width * anchorRect.lossyScale.x / parentRect.lossyScale.x * 0.5f;
        float tooltipHalfWidth = tooltipRect.rect.width * 0.5f;
        float gap = 10f;

        // 오른쪽에 배치 시도
        float rightX = localPoint.x + anchorHalfWidth + tooltipHalfWidth + gap;

        // 화면 오른쪽 경계를 넘으면 왼쪽에 배치
        float parentHalfWidth = parentRect.rect.width * 0.5f;
        float tooltipRightEdge = rightX + tooltipHalfWidth;

        float finalX;
        if (tooltipRightEdge > parentHalfWidth)
        {
            finalX = localPoint.x - anchorHalfWidth - tooltipHalfWidth - gap;
        }
        else
        {
            finalX = rightX;
        }

        tooltipRect.anchoredPosition = new Vector2(finalX, localPoint.y);
    }

    private string FormatSkill(SkillData skill)
    {
        string typeLabel = skill.skillType switch
        {
            SkillType.Attack => "Attack",
            SkillType.Heal => "Heal",
            SkillType.Buff => "Buff",
            SkillType.Debuff => "Debuff",
            _ => ""
        };

        string line = $"{skill.skillName}  [{typeLabel}]  MP {skill.mpCost}  power {skill.power}";
        if (!string.IsNullOrEmpty(skill.description))
        {
            line += $"\n  <color=#AAAAAA>{skill.description}</color>";
        }
        return line;
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
        currentMonster = null;
    }
}
