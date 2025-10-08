using UnityEngine;
using UnityEngine.EventSystems;

public class MonsterInRoomDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private MonsterData monsterData;
    private Room parentRoom;
    private int monsterIndex;
    private GameObject draggedIcon;
    private Camera mainCamera;
    private Canvas canvas;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private void Awake()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Find canvas for UI dragging
        canvas = FindObjectOfType<Canvas>();
    }

    public void Initialize(MonsterData data, Room room, int index)
    {
        monsterData = data;
        parentRoom = room;
        monsterIndex = index;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Optional: Handle click events if needed
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (monsterData == null) return;

        // Create a drag icon that follows the mouse
        draggedIcon = new GameObject("DraggedMonster");

        if (canvas != null)
        {
            draggedIcon.transform.SetParent(canvas.transform, false);
            draggedIcon.transform.SetAsLastSibling();

            UnityEngine.UI.Image dragImage = draggedIcon.AddComponent<UnityEngine.UI.Image>();
            dragImage.sprite = monsterData.icon;
            dragImage.raycastTarget = false;

            RectTransform dragRect = draggedIcon.GetComponent<RectTransform>();
            dragRect.sizeDelta = new Vector2(50, 50);
        }

        // Make the original monster semi-transparent
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedIcon == null) return;

        // Update the drag icon position
        RectTransform dragRect = draggedIcon.GetComponent<RectTransform>();
        if (dragRect != null)
        {
            dragRect.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedIcon != null)
        {
            Destroy(draggedIcon);
        }

        // Restore original transparency
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        if (monsterData == null || parentRoom == null) return;

        // Check if we dropped on the inventory panel
        GameObject droppedOn = eventData.pointerCurrentRaycast.gameObject;

        if (droppedOn != null)
        {
            // Check if dropped on inventory or inventory item
            MonsterInventoryUI inventoryUI = droppedOn.GetComponentInParent<MonsterInventoryUI>();
            if (inventoryUI != null)
            {
                // Return monster to inventory
                ReturnToInventory();
                return;
            }
        }

        // Check if dropped on another room
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(eventData.position);
        Vector2Int gridPos = GridManager.Instance.WorldToGridPosition(worldPos);
        Room targetRoom = RoomManager.Instance.GetRoomAtPosition(gridPos);

        if (targetRoom != null && targetRoom != parentRoom && targetRoom.Type != RoomType.Entrance)
        {
            // Try to move monster to another room
            if (!targetRoom.IsFullOfMonsters)
            {
                if (targetRoom.PlaceMonster(monsterData))
                {
                    // Remove from current room
                    parentRoom.RemoveMonster(monsterIndex);
                    Debug.Log($"Moved {monsterData.monsterName} to another room");
                }
                else
                {
                    Debug.Log("Cannot place monster in target room");
                }
            }
            else
            {
                Debug.Log("Target room is full");
            }
        }
    }

    private void ReturnToInventory()
    {
        // Return monster to inventory
        ShopManager.Instance.ReturnMonsterToInventory(monsterData);

        // Remove from room
        parentRoom.RemoveMonster(monsterIndex);

        Debug.Log($"Returned {monsterData.monsterName} to inventory");
    }
}