using UnityEngine;
using UnityEngine.EventSystems;

public class GridClickHandler : MonoBehaviour
{
    private Camera mainCamera;
    private GridManager gridManager;
    private RoomManager roomManager;
    
    private void Start()
    {
        mainCamera = Camera.main;
        gridManager = GridManager.Instance;
        roomManager = RoomManager.Instance;
        
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found!");
        }
    }
    
    private void Update()
    {
        // UI 위에 마우스가 있으면 그리드 클릭 무시
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            HandleLeftClick();
        }
        else if (Input.GetMouseButtonDown(1)) // Right click
        {
            HandleRightClick();
        }
    }
    
    private void HandleLeftClick()
    {
        // Check if we clicked on a monster first
        if (IsClickOnMonster())
        {
            return; // Don't process room click if clicking on monster (for dragging)
        }

        Vector2Int gridPos = GetGridPositionFromMouse();

        Debug.Log($"Left clicked grid position: {gridPos}");

        if (roomManager == null) return;

        // 선택된 방 타입
        RoomType selectedType = RoomSelectionPanel.Instance != null
            ? RoomSelectionPanel.Instance.SelectedRoomType
            : RoomType.Battle;

        // 이미 방이 있으면 종류 변경
        if (roomManager.IsPositionOccupied(gridPos))
        {
            bool changed = roomManager.ChangeRoomType(gridPos, selectedType);
            if (changed && RoomSelectionPanel.Instance != null)
            {
                if (selectedType == RoomType.Lock)
                {
                    Room lockRoom = roomManager.GetRoomAt(gridPos);
                    RoomSelectionPanel.Instance.SwitchToKeyPlacement(lockRoom);
                }
                else if (selectedType == RoomType.Key)
                {
                    Room keyRoom = roomManager.GetRoomAt(gridPos);
                    RoomSelectionPanel.Instance.OnKeyPlaced(keyRoom);
                }
            }
            return;
        }

        // 인접한 방이 없으면 배치 불가
        if (!roomManager.IsValidPlacement(gridPos))
        {
            Debug.Log($"Cannot place room at {gridPos} - must be adjacent to existing room!");
            return;
        }

        // 방 배치
        bool placed = roomManager.PlaceRoom(gridPos, selectedType);

        if (placed && RoomSelectionPanel.Instance != null)
        {
            // 잠금방 배치 완료 → 열쇠방 배치 모드로 전환
            if (selectedType == RoomType.Lock)
            {
                Room lockRoom = roomManager.GetRoomAt(gridPos);
                RoomSelectionPanel.Instance.SwitchToKeyPlacement(lockRoom);
            }
            // 열쇠방 배치 완료 → 잠금방 모드로 복귀
            else if (selectedType == RoomType.Key)
            {
                Room keyRoom = roomManager.GetRoomAt(gridPos);
                RoomSelectionPanel.Instance.OnKeyPlaced(keyRoom);
            }
        }
    }
    
    private void HandleRightClick()
    {
        Vector2Int gridPos = GetGridPositionFromMouse();

        Debug.Log($"Right clicked grid position: {gridPos}");

        // Remove room at this position
        if (roomManager != null)
        {
            Room room = roomManager.GetRoomAt(gridPos);
            if (room != null && room.Type != RoomType.Entrance)
            {
                roomManager.RemoveRoom(gridPos);
                Debug.Log($"Removed room at {gridPos}");
            }
            else if (room != null && room.Type == RoomType.Entrance)
            {
                Debug.Log("Cannot remove entrance room!");
            }
        }
    }

    
    private Vector2Int GetGridPositionFromMouse()
    {
        // Convert mouse position to world position
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -mainCamera.transform.position.z;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        worldPos.z = 0;

        // Convert world position to grid position
        return gridManager.WorldToGridPosition(worldPos);
    }

    private bool IsClickOnMonster()
    {
        // Raycast to check if we clicked on a monster
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -mainCamera.transform.position.z;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 0f);

        if (hit.collider != null)
        {
            // Check if the collider belongs to a monster
            MonsterInRoomDragHandler monsterDragHandler = hit.collider.GetComponent<MonsterInRoomDragHandler>();
            if (monsterDragHandler != null)
            {
                return true; // Clicked on a monster
            }
        }

        return false; // Didn't click on a monster
    }
}