using UnityEngine;
using System.Collections.Generic;

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
    // [SerializeField] private Vector2Int roomSize = new Vector2Int(1, 1); // 1x1 grid cells by default

    [Header("Monster")]
    [SerializeField] private List<MonsterData> placedMonsters = new List<MonsterData>();
    private List<GameObject> monsterVisuals = new List<GameObject>();
    private const int MAX_MONSTERS = 3;

    private SpriteRenderer spriteRenderer;

    public RoomType Type => roomType;
    public Vector2Int GridPosition => gridPosition;
    // public Vector2Int Size => roomSize;
    public List<MonsterData> PlacedMonsters => placedMonsters;
    public bool HasMonster => placedMonsters.Count > 0;
    public bool IsFullOfMonsters => placedMonsters.Count >= MAX_MONSTERS;
    
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
    
    // public bool OccupiesPosition(Vector2Int checkPos)
    // {
    //     // Check if this room occupies the given grid position
    //     for (int x = 0; x < roomSize.x; x++)
    //     {
    //         for (int y = 0; y < roomSize.y; y++)
    //         {
    //             if (gridPosition.x + x == checkPos.x && gridPosition.y + y == checkPos.y)
    //             {
    //                 return true;
    //             }
    //         }
    //     }
    //     return false;
    // }

    public bool PlaceMonster(MonsterData monster)
    {
        if (IsFullOfMonsters)
        {
            Debug.Log($"Room is full! Already has {MAX_MONSTERS} monsters");
            return false;
        }

        placedMonsters.Add(monster);
        CreateMonsterVisual(monster);
        UpdateMonsterPositions();
        return true;
    }

    public void RemoveMonster(int index)
    {
        if (index < 0 || index >= placedMonsters.Count) return;

        placedMonsters.RemoveAt(index);
        if (index < monsterVisuals.Count)
        {
            Destroy(monsterVisuals[index]);
            monsterVisuals.RemoveAt(index);
        }

        // Re-initialize drag handlers with correct indices
        for (int i = 0; i < monsterVisuals.Count; i++)
        {
            if (monsterVisuals[i] != null)
            {
                MonsterInRoomDragHandler dragHandler = monsterVisuals[i].GetComponent<MonsterInRoomDragHandler>();
                if (dragHandler != null && i < placedMonsters.Count)
                {
                    dragHandler.Initialize(placedMonsters[i], this, i);
                }
            }
        }

        UpdateMonsterPositions();
    }

    public void RemoveAllMonsters()
    {
        placedMonsters.Clear();
        foreach (var visual in monsterVisuals)
        {
            if (visual != null) Destroy(visual);
        }
        monsterVisuals.Clear();
    }

    private void CreateMonsterVisual(MonsterData monster)
    {
        if (monster == null || monster.icon == null) return;

        // Create a visual indicator for the monster
        GameObject monsterVisual = new GameObject($"MonsterVisual_{monsterVisuals.Count}");
        monsterVisual.transform.SetParent(transform);

        SpriteRenderer monsterSprite = monsterVisual.AddComponent<SpriteRenderer>();
        monsterSprite.sprite = monster.icon;
        monsterSprite.sortingOrder = 2; // Above room sprite

        // Scale the monster
        monsterVisual.transform.localScale = Vector3.one * 2.5f;

        // Add collider for interaction
        BoxCollider2D collider = monsterVisual.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.26f, 0.21f);

        // Add drag handler
        MonsterInRoomDragHandler dragHandler = monsterVisual.AddComponent<MonsterInRoomDragHandler>();
        dragHandler.Initialize(monster, this, monsterVisuals.Count);

        monsterVisuals.Add(monsterVisual);
    }

    private void UpdateMonsterPositions()
    {
        // Arrange monsters in the room based on count
        for (int i = 0; i < monsterVisuals.Count; i++)
        {
            if (monsterVisuals[i] == null) continue;

            Vector3 position = GetMonsterPosition(i, monsterVisuals.Count);
            monsterVisuals[i].transform.localPosition = position;
        }
    }

    private Vector3 GetMonsterPosition(int index, int totalCount)
    {
        float spacing = 0.6f; // Increased spacing between monsters

        switch (totalCount)
        {
            case 1:
                // Single monster in center
                return Vector3.zero;
            case 2:
                // Two monsters side by side
                float xOffset = (index == 0) ? -spacing : spacing;
                return new Vector3(xOffset, 0, 0);
            case 3:
                // Three monsters in triangle formation
                if (index == 0)
                    return new Vector3(0, spacing * 0.8f, 0);  // Top
                else if (index == 1)
                    return new Vector3(-spacing, -spacing * 0.4f, 0);  // Bottom left
                else
                    return new Vector3(spacing, -spacing * 0.4f, 0);   // Bottom right
            default:
                return Vector3.zero;
        }
    }

    public void ChangeRoomType(RoomType newType, Sprite newSprite)
    {
        roomType = newType;

        if (spriteRenderer != null && newSprite != null)
        {
            spriteRenderer.sprite = newSprite;
        }

        gameObject.name = $"Room_{newType}_{gridPosition.x}_{gridPosition.y}";
        Debug.Log($"Room at {gridPosition} changed from Treasure to {newType}");
    }
}