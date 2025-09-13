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
        TextMeshProUGUI nameText = item.GetComponentInChildren<TextMeshProUGUI>();
        Button buyButton = item.GetComponentInChildren<Button>();
        
        // ShopMonsterPanel 자체가 아닌 자식 Image 컴포넌트 찾기
        Image[] images = item.GetComponentsInChildren<Image>();
        Image iconImage = null;
        foreach (Image img in images)
        {
            // Button의 Image가 아닌 별도의 Image 컴포넌트 찾기
            if (img.gameObject != item && img.GetComponent<Button>() == null)
            {
                iconImage = img;
                break;
            }
        }
        
        if (nameText != null)
        {
            nameText.text = monster.monsterName;
        }
        
        if (iconImage != null && monster.icon != null)
        {
            iconImage.sprite = monster.icon;
        }
        
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => OnBuyButtonClicked(monster));
            
            TextMeshProUGUI buttonText = buyButton.GetComponentInChildren<TextMeshProUGUI>();
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