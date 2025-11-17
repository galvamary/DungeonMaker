using UnityEngine;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }
    
    // [Header("Room Prefabs")]
    // [SerializeField] private GameObject roomPrefab;
    
    [Header("Room Sprites")]
    [SerializeField] private Sprite battleRoomSprite;
    [SerializeField] private Sprite treasureRoomSprite;
    [SerializeField] private Sprite bossRoomSprite;
    [SerializeField] private Sprite entranceRoomSprite;
    
    [Header("Room Costs")]
    [SerializeField] private int battleRoomCost = 10;
    [SerializeField] private int treasureRoomCost = 30;
    [SerializeField] private int bossRoomCost = 50;
    
    private Dictionary<Vector2Int, Room> placedRooms = new Dictionary<Vector2Int, Room>();
    private Transform roomContainer;
    
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
        
        // Create container for rooms
         GameObject container = new GameObject("Rooms");
        roomContainer = container.transform;
    }
    
    private void Start()
    {
        // Place entrance at (0,0) on start
        PlaceRoom(new Vector2Int(0, 0), RoomType.Entrance);
        Debug.Log("Placed entrance room at (0,0)");
    }
    
    public bool PlaceRoom(Vector2Int gridPosition, RoomType roomType)
    {
        // Check if position is already occupied
        if (placedRooms.ContainsKey(gridPosition))
        {
            Debug.Log($"Grid position {gridPosition} is already occupied!");
            return false;
        }
        
        // Check cost for non-entrance rooms
        if (roomType != RoomType.Entrance)
        {
            int cost = GetRoomCost(roomType);
            if (!GameManager.Instance.CanAfford(cost))
            {
                Debug.Log($"Not enough gold to place {roomType} room! Cost: {cost}, Current Gold: {GameManager.Instance.CurrentGold}");
                return false;
            }
            
            // Deduct cost
            GameManager.Instance.SpendGold(cost);
        }
        
        // Get sprite for room type
        Sprite roomSprite = GetRoomSprite(roomType);
        if (roomSprite == null)
        {
            Debug.LogWarning($"No sprite found for room type: {roomType}");
            return false;
        }
        
        // Create room object
        GameObject roomObj;
        // if (roomPrefab != null)
        // {
        //     roomObj = Instantiate(roomPrefab, roomContainer);
        // }
        // else
        // {
            roomObj = new GameObject($"Room_{roomType}");
            roomObj.transform.SetParent(roomContainer);
        // }
        
        // Initialize room component
        Room room = roomObj.GetComponent<Room>();
        if (room == null)
        {
            room = roomObj.AddComponent<Room>();
        }
        
        room.Initialize(roomType, gridPosition, roomSprite);
        
        // Add to dictionary
        placedRooms[gridPosition] = room;
        
        Debug.Log($"Placed {roomType} room at grid position {gridPosition}");
        return true;
    }
    
    public void RemoveRoom(Vector2Int gridPosition)
    {
        if (placedRooms.TryGetValue(gridPosition, out Room room))
        {
            // Return all monsters in this room to inventory
            if (room.HasMonster)
            {
                foreach (MonsterData monster in room.PlacedMonsters)
                {
                    ShopManager.Instance.ReturnMonsterToInventory(monster);
                    Debug.Log($"Returned {monster.monsterName} to inventory");
                }
            }

            // Refund the cost for non-entrance rooms
            if (room.Type != RoomType.Entrance)
            {
                int refund = GetRoomCost(room.Type);
                GameManager.Instance.AddGold(refund);
                Debug.Log($"Refunded {refund} gold for removing {room.Type} room");
            }

            Destroy(room.gameObject);
            placedRooms.Remove(gridPosition);
        }
    }
    
    public Room GetRoomAt(Vector2Int gridPosition)
    {
        placedRooms.TryGetValue(gridPosition, out Room room);
        return room;
    }
    
    public bool IsPositionOccupied(Vector2Int gridPosition)
    {
        return placedRooms.ContainsKey(gridPosition);
    }
    
    public void CycleRoomAt(Vector2Int gridPosition)
    {
        Room existingRoom = GetRoomAt(gridPosition);
        
        if (existingRoom == null)
        {
            // Check if position is valid for placement
            if (!IsValidPlacement(gridPosition))
            {
                Debug.Log($"Cannot place room at {gridPosition} - must be adjacent to existing room!");
                return;
            }
            
            // Check if player can afford a battle room
            if (!GameManager.Instance.CanAfford(battleRoomCost))
            {
                Debug.Log($"Not enough gold for Battle room! Cost: {battleRoomCost}, Current Gold: {GameManager.Instance.CurrentGold}");
                return;
            }
            
            // Empty -> Battle
            PlaceRoom(gridPosition, RoomType.Battle);
        }
        else
        {
            // Don't allow changing entrance rooms
            if (existingRoom.Type == RoomType.Entrance)
            {
                Debug.Log("Cannot change entrance room!");
                return;
            }

            // Don't allow changing rooms with monsters
            if (existingRoom.HasMonster)
            {
                Debug.Log("Cannot change room type while monsters are placed! Remove monsters first.");
                return;
            }

            switch (existingRoom.Type)
            {
                case RoomType.Battle:
                    // Battle -> Treasure
                    // Check if player can afford the difference (30 - 10 = 20 more gold)
                    int upgradeCost = treasureRoomCost - battleRoomCost;
                    if (!GameManager.Instance.CanAfford(upgradeCost))
                    {
                        Debug.Log($"Not enough gold to upgrade to Treasure room! Need {upgradeCost} more gold");
                        return;
                    }
                    
                    // Spend the difference
                    GameManager.Instance.SpendGold(upgradeCost);
                    
                    // Change sprite without removing the room
                    existingRoom.Initialize(RoomType.Treasure, gridPosition, GetRoomSprite(RoomType.Treasure));
                    break;
                    
                case RoomType.Treasure:
                    // Treasure rooms stay as treasure (no cycling)
                    Debug.Log("Treasure room cannot be cycled. Use right-click to remove.");
                    break;
                    
                default:
                    // Any other type stays as is
                    break;
            }
        }
    }
    
    public Sprite GetRoomSprite(RoomType roomType)
    {
        switch (roomType)
        {
            case RoomType.Battle:
                return battleRoomSprite;
            case RoomType.Treasure:
                return treasureRoomSprite;
            case RoomType.Boss:
                return bossRoomSprite;
            case RoomType.Entrance:
                return entranceRoomSprite;
            default:
                return battleRoomSprite;
        }
    }
    
    private int GetRoomCost(RoomType roomType)
    {
        switch (roomType)
        {
            case RoomType.Battle:
                return battleRoomCost;
            case RoomType.Treasure:
                return treasureRoomCost;
            case RoomType.Boss:
                return bossRoomCost;
            default:
                return 0;
        }
    }
    
    private bool IsValidPlacement(Vector2Int position)
    {
        // Check if this position is adjacent to any existing room
        Vector2Int[] adjacentOffsets = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // Up
            new Vector2Int(0, -1),  // Down
            new Vector2Int(-1, 0),  // Left
            new Vector2Int(1, 0)    // Right
        };
        
        foreach (var offset in adjacentOffsets)
        {
            Vector2Int adjacentPos = position + offset;
            if (placedRooms.ContainsKey(adjacentPos))
            {
                Room adjacentRoom = placedRooms[adjacentPos];
                
                // Special rule for entrance: can only connect to its right
                if (adjacentRoom.Type == RoomType.Entrance)
                {
                    // Check if the new position is to the right of entrance
                    if (position == adjacentRoom.GridPosition + new Vector2Int(1, 0))
                    {
                        return true; // Valid: connecting to right of entrance
                    }
                    // If we're trying to connect to entrance from other directions, continue checking
                    continue;
                }
                
                // For non-entrance rooms, any adjacent connection is valid
                return true;
            }
        }
        
        return false;
    }
    
    // @deprecated
    private bool IsConnectedToEntrance(Vector2Int position, HashSet<Vector2Int> visited = null)
    {
        // Helper method to check if a position has a path to the entrance
        if (visited == null)
            visited = new HashSet<Vector2Int>();
            
        if (visited.Contains(position))
            return false;
            
        visited.Add(position);
        
        // Check if this is the entrance or adjacent to entrance
        if (placedRooms.TryGetValue(position, out Room room))
        {
            if (room.Type == RoomType.Entrance)
                return true;
        }
        
        // Check all adjacent rooms
        Vector2Int[] adjacentOffsets = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // Up
            new Vector2Int(0, -1),  // Down
            new Vector2Int(-1, 0),  // Left
            new Vector2Int(1, 0)    // Right
        };
        
        foreach (var offset in adjacentOffsets)
        {
            Vector2Int adjacentPos = position + offset;
            if (placedRooms.ContainsKey(adjacentPos) && IsConnectedToEntrance(adjacentPos, visited))
            {
                return true;
            }
        }
        
        return false;
    }

    public Room GetRoomAtPosition(Vector2Int gridPosition)
    {
        placedRooms.TryGetValue(gridPosition, out Room room);
        return room;
    }

    public Room GetEntranceRoom()
    {
        foreach (var room in placedRooms.Values)
        {
            if (room.Type == RoomType.Entrance)
            {
                return room;
            }
        }
        return null;
    }

    public Room GetTreasureRoom()
    {
        foreach (var room in placedRooms.Values)
        {
            if (room.Type == RoomType.Treasure)
            {
                return room;
            }
        }
        return null;
    }

    public bool AreAllRoomsConnected()
    {
        if (placedRooms.Count == 0) return false;

        // Find entrance room as starting point
        Room entranceRoom = GetEntranceRoom();
        if (entranceRoom == null) return false;

        // BFS to find all connected rooms
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        System.Collections.Generic.Queue<Vector2Int> queue = new System.Collections.Generic.Queue<Vector2Int>();

        queue.Enqueue(entranceRoom.GridPosition);
        visited.Add(entranceRoom.GridPosition);

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // Up
            new Vector2Int(0, -1),  // Down
            new Vector2Int(-1, 0),  // Left
            new Vector2Int(1, 0)    // Right
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            foreach (var direction in directions)
            {
                Vector2Int neighbor = current + direction;

                if (placedRooms.ContainsKey(neighbor) && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        // Check if all rooms were visited
        return visited.Count == placedRooms.Count;
    }
}