using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MonsterInventoryUI : MonoBehaviour
{
    public static MonsterInventoryUI Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform inventoryItemContainer;
    [SerializeField] private GameObject inventoryItemPrefab;
    
    private Dictionary<MonsterData, GameObject> inventoryItems = new Dictionary<MonsterData, GameObject>();
    
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
    
    private void Start()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
        }
    }
    
    public void UpdateMonsterInventory(MonsterData monster, int count)
    {
        if (inventoryItems.ContainsKey(monster))
        {
            // 이미 있는 몬스터면 개수만 업데이트
            UpdateInventoryItemCount(inventoryItems[monster], count);
        }
        else if (count > 0)
        {
            // 새로운 몬스터 추가
            CreateInventoryItem(monster, count);
        }
    }
    
    private void CreateInventoryItem(MonsterData monster, int count)
    {
        if (inventoryItemPrefab == null || inventoryItemContainer == null) return;
        
        GameObject item = Instantiate(inventoryItemPrefab, inventoryItemContainer);
        SetupInventoryItem(item, monster, count);
        inventoryItems[monster] = item;
    }
    
    private void SetupInventoryItem(GameObject item,  MonsterData monster, int count)
    {
        // 몬스터 아이콘 설정
        Transform iconTransform = item.transform.Find("MonsterIcon");
        if (iconTransform != null)
        {
            Image iconImage = iconTransform.GetComponent<Image>();
            if (iconImage != null && monster.icon != null)
            {
                iconImage.sprite = monster.icon;
            }
        }

        // 개수 텍스트 설정
        Transform countTransform = item.transform.Find("CountText");
        if (countTransform != null)
        {
            TextMeshProUGUI countText = countTransform.GetComponent<TextMeshProUGUI>();
            if (countText != null)
            {
                countText.text = count.ToString();
            }
        }

        // Add drag handler component
        MonsterDragHandler dragHandler = item.GetComponent<MonsterDragHandler>();
        if (dragHandler == null)
        {
            dragHandler = item.AddComponent<MonsterDragHandler>();
        }
        dragHandler.Initialize(monster, count);
    }
    
    private void UpdateInventoryItemCount(GameObject item, int count)
    {
        if (count <= 0)
        {
            // 개수가 0이면 아이템 제거
            // Remove from dictionary first
            MonsterData monsterToRemove = null;
            foreach (var kvp in inventoryItems)
            {
                if (kvp.Value == item)
                {
                    monsterToRemove = kvp.Key;
                    break;
                }
            }
            if (monsterToRemove != null)
            {
                inventoryItems.Remove(monsterToRemove);
            }
            Destroy(item);
            return;
        }

        Transform countTransform = item.transform.Find("CountText");
        if (countTransform != null)
        {
            TextMeshProUGUI countText = countTransform.GetComponent<TextMeshProUGUI>();
            if (countText != null)
            {
                countText.text = count.ToString();
            }
        }

        // Update drag handler count
        MonsterDragHandler dragHandler = item.GetComponent<MonsterDragHandler>();
        if (dragHandler != null)
        {
            MonsterData monster = null;
            foreach (var kvp in inventoryItems)
            {
                if (kvp.Value == item)
                {
                    monster = kvp.Key;
                    break;
                }
            }
            if (monster != null)
            {
                dragHandler.Initialize(monster, count);
            }
        }
    }
}