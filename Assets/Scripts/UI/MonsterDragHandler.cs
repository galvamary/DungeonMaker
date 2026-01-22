using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MonsterDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
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
        Debug.Log($"OnEndDrag called. draggedIcon null? {draggedIcon == null}");

        // Clean up drag visuals first
        CleanupDrag();

        if (monsterData == null || monsterCount <= 0) return;

        // Check if we dropped on ShopUI (ShopUI will handle the sale via IDropHandler)
        // If dropped on ShopUI, the ShopUI.OnDrop will be called automatically
        // We just need to check if it wasn't handled by ShopUI, then try room placement

        // Check if dropped on a UI element
        if (eventData.pointerEnter != null)
        {
            Debug.Log($"Dropped on UI element: {eventData.pointerEnter.name}");

            // Check if the UI element or its parent is ShopUI
            ShopUI shopUI = eventData.pointerEnter.GetComponentInParent<ShopUI>();
            if (shopUI != null)
            {
                Debug.Log("Dropped on shop - ShopUI.OnDrop will handle it");
                // Dropped on shop - ShopUI.OnDrop will handle it
                return;
            }
        }

        // Not dropped on shop, try to place in room
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(eventData.position);
        Vector2Int gridPos = GridManager.Instance.WorldToGridPosition(worldPos);

        // Check if there's a room at this position
        Room room = RoomManager.Instance.GetRoomAtPosition(gridPos);
        if (room != null && room.Type != RoomType.Entrance && room.Type != RoomType.Treasure)
        {
            // Place the monster in the room
            if (room.PlaceMonster(monsterData))
            {
                // Decrease monster count in inventory
                ShopManager.Instance.UseMonsterFromInventory(monsterData);
            }
            else
            {
                Debug.Log("Cannot place monster in this room");
            }
        }
    }
}