using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RoomSelectionPanel : MonoBehaviour
{
    public static RoomSelectionPanel Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject roomItemPrefab;

    [Header("선택 표시 색상")]
    [SerializeField] private Color selectedColor = new Color(1f, 0.85f, 0.3f, 1f);
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 1f);

    private RoomType selectedRoomType = RoomType.Battle;
    private readonly Dictionary<RoomType, Image> roomItemImages = new();

    public RoomType SelectedRoomType => selectedRoomType;

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
        PopulateRoomItems();
    }

    private void PopulateRoomItems()
    {
        if (RoomManager.Instance == null || itemContainer == null || roomItemPrefab == null) return;

        // 기존 아이템 제거
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }

        roomItemImages.Clear();

        // Battle, Treasure, Boss 순서로 생성 (Entrance 제외)
        RoomType[] roomTypes = { RoomType.Battle, RoomType.Treasure};

        foreach (RoomType roomType in roomTypes)
        {
            GameObject item = Instantiate(roomItemPrefab, itemContainer);
            SetupRoomItem(item, roomType);
        }

        // 기본 선택: Battle
        UpdateSelectionVisual();
    }

    private void SetupRoomItem(GameObject item, RoomType roomType)
    {
        // Image 설정
        Transform imageTransform = item.transform.Find("Image");
        if (imageTransform != null)
        {
            Image iconImage = imageTransform.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.sprite = RoomManager.Instance.GetRoomSprite(roomType);
                roomItemImages[roomType] = iconImage;
            }
        }

        // 비용 설정
        Transform costTransform = item.transform.Find("CostText");
        if (costTransform != null)
        {
            TextMeshProUGUI costText = costTransform.GetComponent<TextMeshProUGUI>();
            if (costText != null)
            {
                costText.text = $"{RoomManager.Instance.GetRoomCost(roomType)}G";
            }
        }

        // 버튼 클릭 이벤트
        Button button = item.GetComponent<Button>();
        if (button != null)
        {
            RoomType capturedType = roomType;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SelectRoom(capturedType));
        }
    }

    private void SelectRoom(RoomType roomType)
    {
        selectedRoomType = roomType;
        UpdateSelectionVisual();
        Debug.Log($"방 선택: {GetRoomName(roomType)}");
    }

    private void UpdateSelectionVisual()
    {
        foreach (var kvp in roomItemImages)
        {
            kvp.Value.color = kvp.Key == selectedRoomType ? selectedColor : normalColor;
        }
    }

    private string GetRoomName(RoomType roomType)
    {
        return roomType switch
        {
            RoomType.Battle => "전투",
            RoomType.Treasure => "보물",
            RoomType.Boss => "보스",
            _ => roomType.ToString()
        };
    }
}
