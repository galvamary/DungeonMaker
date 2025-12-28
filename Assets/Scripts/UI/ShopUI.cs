using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button shopToggleButton;
    [SerializeField] private Transform shopItemContainer;
    [SerializeField] private GameObject shopItemPrefab;
    
    private void Start()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }
 
    public void ToggleShop()
    {
        if (shopPanel != null)
        {
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

        // Image 스프라이트 설정 및 툴팁 이벤트 핸들러 추가
        if (imageTransform != null)
        {
            Image iconImage = imageTransform.GetComponent<Image>();
            if (iconImage != null && monster.icon != null)
            {
                iconImage.sprite = monster.icon;
            }

            // Image에만 ShopMonsterPanel 컴포넌트 추가
            ShopMonsterPanel monsterPanel = imageTransform.GetComponent<ShopMonsterPanel>();
            if (monsterPanel == null)
            {
                monsterPanel = imageTransform.gameObject.AddComponent<ShopMonsterPanel>();
            }
            monsterPanel.SetMonsterData(monster);
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
                buttonText.text = $"{monster.cost} Gold";
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
}