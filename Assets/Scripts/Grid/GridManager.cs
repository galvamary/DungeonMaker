using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    
    [Header("Grid Settings")]
    [SerializeField] private float cellSize = 3f;  // 48 pixels / 16 PPU = 3 units
    
    public float CellSize => cellSize;
    
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
    }
    
    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        float worldX = gridPosition.x * cellSize + cellSize / 2f;  // 칸의 중심점
        float worldY = gridPosition.y * cellSize + cellSize / 2f;  // 칸의 중심점
        return new Vector3(worldX, worldY, 0);
    }
    
    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        int gridX = Mathf.FloorToInt(worldPosition.x / cellSize);
        int gridY = Mathf.FloorToInt(worldPosition.y / cellSize);
        return new Vector2Int(gridX, gridY);
    }
    
    public Vector3 SnapToGrid(Vector3 worldPosition)
    {
        Vector2Int gridPos = WorldToGridPosition(worldPosition);
        return GridToWorldPosition(gridPos);
    }
}