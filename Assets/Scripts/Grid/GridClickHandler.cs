using UnityEngine;

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
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            HandleGridClick();
        }
    }
    
    private void HandleGridClick()
    {
        // Convert mouse position to world position
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -mainCamera.transform.position.z;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        worldPos.z = 0;
        
        // Convert world position to grid position
        Vector2Int gridPos = gridManager.WorldToGridPosition(worldPos);
        
        Debug.Log($"Clicked grid position: {gridPos}");
        
        // Place room at clicked position
        if (roomManager != null)
        {
            roomManager.PlaceRoom(gridPos, RoomType.Battle);
        }
    }
}