using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    
    [Header("Grid Settings")]
    [SerializeField] private float cellSize = 0.48f;  // 48 pixels / 100 pixels per unit
    
    private int gridWidth;
    private int gridHeight;
    
    private Vector2Int gridOffset;
    
    public float CellSize => cellSize;
    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        // Calculate grid size based on 1920x1080 screen
        // 1920 / 48 = 40 cells horizontally
        // 1080 / 48 = 22.5 cells vertically (round up to 23)
        gridWidth = 40;
        gridHeight = 23;
        
        gridOffset = new Vector2Int(gridWidth / 2, gridHeight / 2);
    }
    
    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        float worldX = (gridPosition.x - gridOffset.x) * cellSize;
        float worldY = (gridPosition.y - gridOffset.y) * cellSize;
        return new Vector3(worldX, worldY, 0);
    }
    
    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        int gridX = Mathf.RoundToInt(worldPosition.x / cellSize) + gridOffset.x;
        int gridY = Mathf.RoundToInt(worldPosition.y / cellSize) + gridOffset.y;
        return new Vector2Int(gridX, gridY);
    }
    
    public Vector3 SnapToGrid(Vector3 worldPosition)
    {
        Vector2Int gridPos = WorldToGridPosition(worldPosition);
        return GridToWorldPosition(gridPos);
    }
    
    public bool IsValidGridPosition(Vector2Int gridPosition)
    {
        return gridPosition.x >= 0 && gridPosition.x < gridWidth &&
               gridPosition.y >= 0 && gridPosition.y < gridHeight;
    }
}