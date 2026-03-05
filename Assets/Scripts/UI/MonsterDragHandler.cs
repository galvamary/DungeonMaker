using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MonsterDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private MonsterData monsterData;
    private int monsterCount;
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private GameObject draggedIcon;
    private Camera mainCamera;

    // Public property to access monster data
    public MonsterData MonsterData => monsterData;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        canvas = GetComponentInParent<Canvas>();
        while (canvas == null && transform.parent != null)
        {
            canvas = transform.parent.GetComponentInParent<Canvas>();
        }

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        mainCamera = Camera.main;
    }

    public void Initialize(MonsterData data, int count)
    {
        monsterData = data;
        monsterCount = count;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        if (monsterData == null || monsterCount <= 0) return;

        if (ShopManager.Instance != null && ShopManager.Instance.SellMonster(monsterData))
        {
            Debug.Log($"Sold {monsterData.monsterName} for {monsterData.cost} gold!");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (monsterData == null || monsterCount <= 0) return;

        // Create a drag icon that follows the mouse
        draggedIcon = new GameObject("DraggedMonster");
        draggedIcon.transform.SetParent(canvas.transform, false);
        draggedIcon.transform.SetAsLastSibling();

        Image dragImage = draggedIcon.AddComponent<Image>();
        dragImage.sprite = monsterData.icon;
        dragImage.raycastTarget = false;

        RectTransform dragRect = draggedIcon.GetComponent<RectTransform>();
        dragRect.sizeDelta = new Vector2(75, 75);

        // Make the original slot semi-transparent
        canvasGroup.alpha = 0.5f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedIcon == null) return;

        // Update the drag icon position
        RectTransform dragRect = draggedIcon.GetComponent<RectTransform>();
        dragRect.position = eventData.position;
    }

    /// <summary>
    /// Clean up the drag operation (destroy drag icon and restore alpha)
    /// This should be called when drag ends, either from OnEndDrag or externally (e.g., from ShopUI.OnDrop)
    /// </summary>
    public void CleanupDrag()
    {
        if (draggedIcon != null)
        {
            Debug.Log($"CleanupDrag: Destroying draggedIcon: {draggedIcon.name}");
            Destroy(draggedIcon);
            draggedIcon = null;
        }

        canvasGroup.alpha = 1f;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        CleanupDrag();

        if (monsterData == null || monsterCount <= 0) return;

        // 드롭 위치의 방에 몬스터 배치 시도
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(eventData.position);
        Vector2Int gridPos = GridManager.Instance.WorldToGridPosition(worldPos);

        Room room = RoomManager.Instance.GetRoomAtPosition(gridPos);
        if (room != null && room.Type != RoomType.Entrance && room.Type != RoomType.Treasure)
        {
            if (room.PlaceMonster(monsterData))
            {
                ShopManager.Instance.UseMonsterFromInventory(monsterData);
            }
        }
    }
}