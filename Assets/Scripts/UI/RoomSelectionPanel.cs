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
    [SerializeField] private Color selectedColor = new Color(0.3f, 1f, 0.3f, 1f);
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 1f);

    private RoomType selectedRoomType = RoomType.Battle;
    private readonly Dictionary<RoomType, Image> roomItemImages = new();
    private readonly HashSet<RoomType> lockedRoomTypes = new();

    // Lock→Key 전환 상태
    private bool isWaitingForKeyPlacement = false;
    private Room pendingLockRoom;  // 배치된 잠금방 (열쇠방 배치 대기 중)

    public RoomType SelectedRoomType => selectedRoomType;
    public bool IsWaitingForKeyPlacement => isWaitingForKeyPlacement;

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

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnReputationChanged += OnReputationChanged;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnReputationChanged -= OnReputationChanged;
        }
    }

    private void OnReputationChanged(int newReputation)
    {
        // 명성 변경 시 잠금 상태 재확인
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
        lockedRoomTypes.Clear();

        // Battle, Treasure, Fire, Ice, Lock 순서로 생성
        RoomType[] roomTypes = { RoomType.Battle, RoomType.Treasure, RoomType.Fire, RoomType.Ice, RoomType.Lock };

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

        // 해금 여부 확인
        bool isUnlocked = RoomManager.Instance.IsRoomUnlocked(roomType);

        // 버튼 클릭 이벤트
        Button button = item.GetComponent<Button>();
        if (button != null)
        {
            if (isUnlocked)
            {
                RoomType capturedType = roomType;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => SelectRoom(capturedType));
            }
            else
            {
                button.interactable = false;
                lockedRoomTypes.Add(roomType);
            }
        }

        // 잠금 상태면 어둡게 + 필요 명성 텍스트
        if (!isUnlocked)
        {
            // BlackImage 활성화로 전체 어둡게
            Transform blackImage = item.transform.Find("BlackImage");
            if (blackImage != null)
            {
                blackImage.gameObject.SetActive(true);
            }

            int requiredRep = RoomManager.Instance.GetRoomUnlockReputation(roomType);
            Transform lockTextTransform = item.transform.Find("LockText");
            TextMeshProUGUI lockText;
            if (lockTextTransform != null)
            {
                lockTextTransform.gameObject.SetActive(true);
                lockText = lockTextTransform.GetComponent<TextMeshProUGUI>();
                lockText.text = $"{requiredRep}";
            }
        }
    }

    private void SelectRoom(RoomType roomType)
    {
        if (lockedRoomTypes.Contains(roomType)) return;
        // 열쇠방 배치 대기 중에는 다른 방 선택 불가
        if (isWaitingForKeyPlacement) return;

        selectedRoomType = roomType;
        UpdateSelectionVisual();
        Debug.Log($"방 선택: {GetRoomName(roomType)}");
    }

    /// <summary>
    /// 잠금방 배치 후 열쇠방 배치 모드로 전환
    /// </summary>
    public void SwitchToKeyPlacement(Room lockRoom)
    {
        isWaitingForKeyPlacement = true;
        pendingLockRoom = lockRoom;
        selectedRoomType = RoomType.Key;

        // Lock 버튼 이미지를 열쇠방 스프라이트로 변경
        if (roomItemImages.ContainsKey(RoomType.Lock))
        {
            roomItemImages[RoomType.Lock].sprite = RoomManager.Instance.GetRoomSprite(RoomType.Key);
        }

        UpdateSelectionVisual();
        Debug.Log("열쇠방 배치 모드로 전환");
    }

    /// <summary>
    /// 열쇠방 배치 완료 후 잠금방 모드로 복귀
    /// </summary>
    public void OnKeyPlaced(Room keyRoom)
    {
        // Lock-Key 연결
        if (pendingLockRoom != null && keyRoom != null)
        {
            RoomManager.Instance.LinkLockAndKeyRooms(pendingLockRoom, keyRoom);
        }

        isWaitingForKeyPlacement = false;
        pendingLockRoom = null;
        selectedRoomType = RoomType.Lock;

        // Lock 버튼 이미지를 잠금방 스프라이트로 복귀
        if (roomItemImages.ContainsKey(RoomType.Lock))
        {
            roomItemImages[RoomType.Lock].sprite = RoomManager.Instance.GetRoomSprite(RoomType.Lock);
        }

        UpdateSelectionVisual();
        Debug.Log("잠금방 모드로 복귀");
    }

    /// <summary>
    /// 열쇠방 배치 취소 (잠금방 삭제 시 호출)
    /// </summary>
    public void CancelKeyPlacement()
    {
        isWaitingForKeyPlacement = false;
        pendingLockRoom = null;
        selectedRoomType = RoomType.Battle;

        // Lock 버튼 이미지를 잠금방 스프라이트로 복귀
        if (roomItemImages.ContainsKey(RoomType.Lock))
        {
            roomItemImages[RoomType.Lock].sprite = RoomManager.Instance.GetRoomSprite(RoomType.Lock);
        }

        UpdateSelectionVisual();
        Debug.Log("열쇠방 배치 취소");
    }

    private void UpdateSelectionVisual()
    {
        foreach (var kvp in roomItemImages)
        {
            if (lockedRoomTypes.Contains(kvp.Key)) continue;
            kvp.Value.color = kvp.Key == selectedRoomType ||
                              (isWaitingForKeyPlacement && kvp.Key == RoomType.Lock)
                              ? selectedColor : normalColor;
        }
    }

    private string GetRoomName(RoomType roomType)
    {
        return roomType switch
        {
            RoomType.Battle => "전투",
            RoomType.Treasure => "보물",
            RoomType.Boss => "보스",
            RoomType.Fire => "불꽃",
            RoomType.Ice => "얼음",
            RoomType.Lock => "잠금",
            RoomType.Key => "열쇠",
            _ => roomType.ToString()
        };
    }
}
