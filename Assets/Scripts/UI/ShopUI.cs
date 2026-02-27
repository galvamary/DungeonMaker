using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ShopUI : MonoBehaviour, IDropHandler
{
    public static ShopUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button shopToggleButton;
    [SerializeField] private Transform shopItemContainer;
    [SerializeField] private GameObject shopItemPrefab;
    private bool isInitialized = false;

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
                // Close shop on start - do this in Start to ensure proper initialization
        if (shopPanel != null && isInitialized)
        {
            shopPanel.SetActive(false);
        }
    }
 
    public void ToggleShop()
    {
        if (shopPanel == null)
        {
            Debug.LogError("ShopPanel is null! Please assign it in the Inspector.");
            return;
        }

        // 처음 호출될 때는 Start()에서 이미 초기화했으므로, 무조건 열기
        if (!isInitialized)
        {
            shopPanel.SetActive(true);
            isInitialized = true;
            PopulateShopItems();
            Debug.Log("Shop opened (first time)");
            return;
        }

        // 이후에는 정상적으로 토글
        bool isActive = !shopPanel.activeSelf;

        shopPanel.SetActive(isActive);

        if (isActive)
        {
            PopulateShopItems();
            Debug.Log("Shop opened");
        }
        else
        {
            Debug.Log("Shop closed");
        }
    }

    private void PopulateShopItems()
    {
        if (ShopManager.Instance == null || shopItemContainer == null) return;

        foreach (Transform child in shopItemContainer)
        {
            Destroy(child.gameObject);
        }
        
        var monsters = ShopManager.Instance.GetAvailableMonsters();
        foreach (var monster in monsters)
        {
            if (shopItemPrefab != null)
            {
                GameObject item = Instantiate(shopItemPrefab, shopItemContainer);
                SetupShopItem(item, monster);
            }
        }
    }
    
    private void SetupShopItem(GameObject item, MonsterData monster)
    {
        // 특정 이름으로 자식 요소들 찾기
        Transform monsterNameTransform = item.transform.Find("MonsterName");
        Transform imageTransform = item.transform.Find("Image");
        Transform buyButtonTransform = item.transform.Find("BuyButton");

        // MonsterName 텍스트 설정
        if (monsterNameTransform != null)
        {
            TextMeshProUGUI nameText = monsterNameTransform.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = monster.monsterName;
            }
        }

        // Image 스프라이트 설정
        if (imageTransform != null)
        {
            Image iconImage = imageTransform.GetComponent<Image>();
            if (iconImage != null && monster.icon != null)
            {
                iconImage.sprite = monster.icon;
            }
        }

        // ShopMonsterPanel 컴포넌트를 아이템 루트에 추가
        ShopMonsterPanel monsterPanel = item.GetComponent<ShopMonsterPanel>();
        if (monsterPanel == null)
        {
            monsterPanel = item.AddComponent<ShopMonsterPanel>();
        }
        monsterPanel.SetMonsterData(monster);

        // InfoButton 연결
        Transform infoButtonTransform = item.transform.Find("InfoButton");
        if (infoButtonTransform != null)
        {
            Button infoButton = infoButtonTransform.GetComponent<Button>();
            if (infoButton != null)
            {
                infoButton.onClick.RemoveAllListeners();
                infoButton.onClick.AddListener(() => monsterPanel.ShowInfo());
            }
        }

        // BuyButton 설정
        if (buyButtonTransform != null)
        {
            Button buyButton = buyButtonTransform.GetComponent<Button>();
            if (buyButton != null)
            {
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(() => OnBuyButtonClicked(monster));
            }
        }

        Transform buttonTextTransform = buyButtonTransform.transform.Find("BuyButtonText");
        if (buttonTextTransform != null)
        {
            TextMeshProUGUI buttonText = buttonTextTransform.GetComponent<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = $"{monster.cost}G";
            }
        }
    }
    
    private void OnBuyButtonClicked(MonsterData monster)
    {
        if (ShopManager.Instance.PurchaseMonster(monster))
        {
            Debug.Log($"Successfully purchased {monster.monsterName}");
        }
        else
        {
            Debug.Log($"Failed to purchase {monster.monsterName}");
        }
    }

    /// <summary>
    /// Called when monster is dropped on the shop UI
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        // Get the dragged monster data from the drag handler
        GameObject draggedObject = eventData.pointerDrag;
        if (draggedObject == null) return;

        MonsterDragHandler dragHandler = draggedObject.GetComponent<MonsterDragHandler>();
        if (dragHandler != null && dragHandler.MonsterData != null)
        {
            // Sell the monster
            if (ShopManager.Instance != null)
            {
                bool sold = ShopManager.Instance.SellMonster(dragHandler.MonsterData);
                if (sold)
                {
                    int sellPrice = dragHandler.MonsterData.cost;
                    Debug.Log($"Sold {dragHandler.MonsterData.monsterName} for {sellPrice} gold!");
                }
            }

            // Clean up the drag operation (since OnEndDrag won't be called when OnDrop handles it)
            dragHandler.CleanupDrag();
        }
    }
}