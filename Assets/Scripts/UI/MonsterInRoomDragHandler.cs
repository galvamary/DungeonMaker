using UnityEngine;
using UnityEngine.EventSystems;

public class MonsterInRoomDragHandler : MonoBehaviour
{
    private MonsterData monsterData;
    private Room parentRoom;
    private int monsterIndex;
    private GameObject draggedIcon;
    private Camera mainCamera;
    private Canvas canvas;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isDragging = false;

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

    private void OnMouseDown()
    {
        // Don't allow dragging during exploration phase
        if (GameStateManager.Instance != null && !GameStateManager.Instance.IsPreparationPhase)
        {
            return;
        }

        if (monsterData == null) return;

        isDragging = true;

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
            dragRect.sizeDelta = new Vector2(75, 75);
            dragRect.position = Input.mousePosition;
        }

        // Make the original monster semi-transparent
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
        }
    }

    private void OnMouseDrag()
    {
        if (!isDragging || draggedIcon == null) return;

        // Update the drag icon position
        RectTransform dragRect = draggedIcon.GetComponent<RectTransform>();
        if (dragRect != null)
        {
            dragRect.position = Input.mousePosition;
        }
    }

    private void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

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

        // Check if we dropped on the inventory panel using raycast
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            MonsterInventoryUI inventoryUI = result.gameObject.GetComponentInParent<MonsterInventoryUI>();
            if (inventoryUI != null)
            {
                // Return monster to inventory
                ReturnToInventory();
                return;
            }
        }

        // Check if dropped on another room
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -mainCamera.transform.position.z;
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        Vector2Int gridPos = GridManager.Instance.WorldToGridPosition(worldPos);
        Room targetRoom = RoomManager.Instance.GetRoomAtPosition(gridPos);

        if (targetRoom != null && targetRoom != parentRoom && targetRoom.Type != RoomType.Entrance && targetRoom.Type != RoomType.Treasure)
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
