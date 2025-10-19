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

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedIcon != null)
        {
            Destroy(draggedIcon);
        }

        canvasGroup.alpha = 1f;

        if (monsterData == null || monsterCount <= 0) return;

        // Check if we dropped on a room
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