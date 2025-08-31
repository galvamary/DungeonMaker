using UnityEngine;

public enum RoomType
{
    Battle,
    Treasure,
    Boss,
    Entrance
}

public class Room : MonoBehaviour
{
    [Header("Room Properties")]
    [SerializeField] private RoomType roomType;
    [SerializeField] private Vector2Int gridPosition;
    [SerializeField] private Vector2Int roomSize = new Vector2Int(1, 1); // 1x1 grid cells by default
    
    private SpriteRenderer spriteRenderer;
    
    public RoomType Type => roomType;
    public Vector2Int GridPosition => gridPosition;
    public Vector2Int Size => roomSize;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
    }
    
    public void Initialize(RoomType type, Vector2Int position, Sprite roomSprite)
    {
        roomType = type;
        gridPosition = position;
        
        if (spriteRenderer != null && roomSprite != null)
        {
            spriteRenderer.sprite = roomSprite;
            spriteRenderer.sortingOrder = 1; // Above grid
        }
        
        // Position the room at grid center
        if (GridManager.Instance != null)
        {
            transform.position = GridManager.Instance.GridToWorldPosition(position);
        }
        
        gameObject.name = $"Room_{type}_{position.x}_{position.y}";
    }
    
    public bool OccupiesPosition(Vector2Int checkPos)
    {
        // Check if this room occupies the given grid position
        for (int x = 0; x < roomSize.x; x++)
        {
            for (int y = 0; y < roomSize.y; y++)
            {
                if (gridPosition.x + x == checkPos.x && gridPosition.y + y == checkPos.y)
                {
                    return true;
                }
            }
        }
        return false;
    }
}