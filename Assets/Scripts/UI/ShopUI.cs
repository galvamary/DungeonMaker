using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button shopToggleButton;
    [SerializeField] private Button closeShopButton;
    [SerializeField] private Transform shopItemContainer;
    [SerializeField] private GameObject shopItemPrefab;
    
    private void Start()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
        
        if (shopToggleButton != null)
        {
            shopToggleButton.onClick.AddListener(ToggleShop);
        }
        
        if (closeShopButton != null)
        {
            closeShopButton.onClick.AddListener(CloseShop);
        }
    }
    
    private void OnDestroy()
    {
        if (shopToggleButton != null)
        {
            shopToggleButton.onClick.RemoveListener(ToggleShop);
        }
        
        if (closeShopButton != null)
        {
            closeShopButton.onClick.RemoveListener(CloseShop);
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
    
    public void OpenShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            PopulateShopItems();
            Debug.Log("Shop opened");
        }
    }
    
    public void CloseShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
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
        TextMeshProUGUI nameText = item.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI costText = item.transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();
        Button buyButton = item.transform.Find("BuyButton")?.GetComponent<Button>();
        Image iconImage = item.transform.Find("Icon")?.GetComponent<Image>();
        
        if (nameText != null)
        {
            nameText.text = monster.monsterName;
        }
        
        if (costText != null)
        {
            costText.text = $"{monster.cost} Gold";
        }
        
        if (iconImage != null && monster.icon != null)
        {
            iconImage.sprite = monster.icon;
        }
        
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => OnBuyButtonClicked(monster));
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