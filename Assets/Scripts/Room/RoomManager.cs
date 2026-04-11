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
    [SerializeField] private Sprite fireRoomSprite;
    [SerializeField] private Sprite iceRoomSprite;
    [SerializeField] private Sprite lockRoomSprite;
    [SerializeField] private Sprite keyRoomSprite;

    [Header("Room Costs")]
    [SerializeField] private int battleRoomCost = 10;
    [SerializeField] private int treasureRoomCost = 30;
    [SerializeField] private int bossRoomCost = 50;
    [SerializeField] private int fireRoomCost = 20;
    [SerializeField] private int iceRoomCost = 20;
    [SerializeField] private int lockRoomCost = 15;

    [Header("Room Unlock Reputation (0 = 처음부터 해금)")]
    [SerializeField] private int battleRoomUnlock = 0;
    [SerializeField] private int treasureRoomUnlock = 0;
    [SerializeField] private int bossRoomUnlock = 5;
    [SerializeField] private int fireRoomUnlock = 3;
    [SerializeField] private int iceRoomUnlock = 3;
    [SerializeField] private int lockRoomUnlock = 2;
    
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
        return PlaceRoom(gridPosition, roomType, true);
    }

    /// <summary>
    /// Places a room at the specified grid position
    /// </summary>
    /// <param name="deductCost">If false, skips cost check and deduction (used for restoration)</param>
    public bool PlaceRoom(Vector2Int gridPosition, RoomType roomType, bool deductCost)
    {
        // Check if position is already occupied
        if (placedRooms.ContainsKey(gridPosition))
        {
            Debug.Log($"Grid position {gridPosition} is already occupied!");
            return false;
        }

        // 해금 여부 확인 (비용 차감 모드일 때만, 로드 시에는 스킵)
        if (deductCost && roomType != RoomType.Entrance && !IsRoomUnlocked(roomType))
        {
            Debug.Log($"{roomType} 방은 아직 해금되지 않았습니다. (필요 명성: {GetRoomUnlockReputation(roomType)})");
            return false;
        }

        // Check cost for non-entrance rooms (only if deductCost is true)
        if (deductCost && roomType != RoomType.Entrance)
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
            // Lock/Key 연결된 방도 함께 삭제
            Room linked = room.LinkedRoom;
            if (linked != null)
            {
                RemoveRoomInternal(linked.GridPosition);
            }

            // 잠금방 삭제 시 열쇠방 대기 상태 해제
            if (room.Type == RoomType.Lock && RoomSelectionPanel.Instance != null
                && RoomSelectionPanel.Instance.IsWaitingForKeyPlacement)
            {
                RoomSelectionPanel.Instance.CancelKeyPlacement();
            }

            RemoveRoomInternal(gridPosition);
        }
    }

    private void RemoveRoomInternal(Vector2Int gridPosition)
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

            room.ClearLinkedRoom();
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
    
    public bool ChangeRoomType(Vector2Int gridPosition, RoomType newType)
    {
        Room room = GetRoomAt(gridPosition);
        if (room == null) return false;
        if (room.Type == RoomType.Entrance) return false;
        if (room.Type == RoomType.Lock || room.Type == RoomType.Key) return false;
        if (room.Type == newType) return false;

        int oldCost = GetRoomCost(room.Type);
        int newCost = GetRoomCost(newType);
        int diff = newCost - oldCost;

        if (diff > 0 && !GameManager.Instance.CanAfford(diff))
        {
            Debug.Log($"Not enough gold! Need {diff} more gold to change to {newType}");
            return false;
        }

        if (diff > 0)
            GameManager.Instance.SpendGold(diff);
        else if (diff < 0)
            GameManager.Instance.AddGold(-diff);

        // 보물/잠금/열쇠 방으로 변경 시 몬스터를 인벤토리로 반환
        if ((newType == RoomType.Treasure || newType == RoomType.Lock || newType == RoomType.Key) && room.HasMonster)
        {
            foreach (MonsterData monster in room.PlacedMonsters)
            {
                ShopManager.Instance.ReturnMonsterToInventory(monster);
                Debug.Log($"Returned {monster.monsterName} to inventory (room changed to Treasure)");
            }
            room.RemoveAllMonsters();
        }

        room.Initialize(newType, gridPosition, GetRoomSprite(newType));
        Debug.Log($"Changed room at {gridPosition} to {newType}");
        return true;
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
            case RoomType.Fire:
                return fireRoomSprite;
            case RoomType.Ice:
                return iceRoomSprite;
            case RoomType.Lock:
                return lockRoomSprite;
            case RoomType.Key:
                return keyRoomSprite;
            default:
                return battleRoomSprite;
        }
    }

    public int GetRoomCost(RoomType roomType)
    {
        switch (roomType)
        {
            case RoomType.Battle:
                return battleRoomCost;
            case RoomType.Treasure:
                return treasureRoomCost;
            case RoomType.Boss:
                return bossRoomCost;
            case RoomType.Fire:
                return fireRoomCost;
            case RoomType.Ice:
                return iceRoomCost;
            case RoomType.Lock:
            case RoomType.Key:
                return lockRoomCost;
            default:
                return 0;
        }
    }

    public int GetRoomUnlockReputation(RoomType roomType)
    {
        return roomType switch
        {
            RoomType.Battle => battleRoomUnlock,
            RoomType.Treasure => treasureRoomUnlock,
            RoomType.Boss => bossRoomUnlock,
            RoomType.Fire => fireRoomUnlock,
            RoomType.Ice => iceRoomUnlock,
            RoomType.Lock => lockRoomUnlock,
            RoomType.Key => lockRoomUnlock,
            _ => 0,
        };
    }

    public bool IsRoomUnlocked(RoomType roomType)
    {
        if (GameManager.Instance == null) return true;
        return GameManager.Instance.IsUnlocked(GetRoomUnlockReputation(roomType));
    }

    /// <summary>
    /// Lock/Key 방을 서로 연결
    /// </summary>
    public void LinkLockAndKeyRooms(Room lockRoom, Room keyRoom)
    {
        if (lockRoom != null && keyRoom != null)
        {
            lockRoom.SetLinkedRoom(keyRoom);
            keyRoom.SetLinkedRoom(lockRoom);
            Debug.Log($"Linked Lock room at {lockRoom.GridPosition} with Key room at {keyRoom.GridPosition}");
        }
    }

    public bool IsValidPlacement(Vector2Int position)
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

    /// <summary>
    /// Gets the total count of treasure rooms in the dungeon
    /// </summary>
    public int GetTreasureRoomCount()
    {
        int count = 0;
        foreach (var room in placedRooms.Values)
        {
            if (room.Type == RoomType.Treasure)
            {
                count++;
            }
        }
        return count;
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

    /// <summary>
    /// Lock/Key 메커니즘을 고려하여 입구에서 모든 보물방에 도달 가능한지 검증.
    /// Key 방에 도달하면 연결된 Lock 방이 해제되는 것을 시뮬레이션하는 점진적 BFS.
    /// </summary>
    public bool CanReachAllTreasureRooms()
    {
        Room entrance = GetEntranceRoom();
        if (entrance == null) return false;

        if (GetTreasureRoomCount() == 0) return true;

        HashSet<Vector2Int> reachable = new HashSet<Vector2Int>();
        System.Collections.Generic.Queue<Vector2Int> frontier = new System.Collections.Generic.Queue<Vector2Int>();
        HashSet<Vector2Int> unlockedLocks = new HashSet<Vector2Int>();

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0)
        };

        frontier.Enqueue(entrance.GridPosition);
        reachable.Add(entrance.GridPosition);

        bool changed = true;
        while (changed)
        {
            changed = false;

            while (frontier.Count > 0)
            {
                Vector2Int current = frontier.Dequeue();
                Room currentRoom = GetRoomAtPosition(current);
                if (currentRoom == null) continue;

                // Key 방 도달 시 연결된 Lock 방 해제
                if (currentRoom.Type == RoomType.Key && currentRoom.LinkedRoom != null)
                {
                    Room lockRoom = currentRoom.LinkedRoom;
                    if (lockRoom.Type == RoomType.Lock && !unlockedLocks.Contains(lockRoom.GridPosition))
                    {
                        unlockedLocks.Add(lockRoom.GridPosition);
                        changed = true;
                    }
                }

                foreach (var dir in directions)
                {
                    Vector2Int neighborPos = current + dir;
                    if (reachable.Contains(neighborPos)) continue;

                    if (!placedRooms.ContainsKey(neighborPos)) continue;
                    Room neighbor = placedRooms[neighborPos];

                    // Entrance는 오른쪽으로만 연결
                    if (currentRoom.Type == RoomType.Entrance && neighborPos != current + new Vector2Int(1, 0))
                        continue;

                    // 잠긴 Lock 방은 통과 불가
                    if (neighbor.Type == RoomType.Lock && !unlockedLocks.Contains(neighborPos))
                        continue;

                    reachable.Add(neighborPos);
                    frontier.Enqueue(neighborPos);
                }
            }

            // 새로 해제된 Lock이 있으면, 도달 가능한 인접 방에서 BFS 재개
            if (changed)
            {
                foreach (Vector2Int lockPos in unlockedLocks)
                {
                    if (reachable.Contains(lockPos)) continue;

                    foreach (var dir in directions)
                    {
                        if (reachable.Contains(lockPos + dir))
                        {
                            reachable.Add(lockPos);
                            frontier.Enqueue(lockPos);
                            break;
                        }
                    }
                }
            }
        }

        // 모든 보물방이 도달 가능한지 확인
        foreach (Room room in placedRooms.Values)
        {
            if (room.Type == RoomType.Treasure && !reachable.Contains(room.GridPosition))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Resets all rooms - destroys all placed rooms and recreates entrance at (0,0)
    /// </summary>
    public void ResetAllRooms()
    {
        Debug.Log("Resetting all rooms...");

        // Destroy all room GameObjects
        foreach (var room in placedRooms.Values)
        {
            if (room != null && room.gameObject != null)
            {
                Destroy(room.gameObject);
            }
        }

        // Clear the placed rooms dictionary
        placedRooms.Clear();

        // Recreate entrance at (0,0)
        PlaceRoom(new Vector2Int(0, 0), RoomType.Entrance);

        Debug.Log("All rooms reset. Entrance recreated at (0,0)");
    }

    /// <summary>
    /// Saves the current room state to the save state object
    /// </summary>
    public void SaveRoomState(SaveState saveState)
    {
        saveState.savedRooms.Clear();

        foreach (var kvp in placedRooms)
        {
            Room room = kvp.Value;
            if (room != null)
            {
                bool hasLinked = room.LinkedRoom != null;
                Vector2Int linkedPos = hasLinked ? room.LinkedRoom.GridPosition : Vector2Int.zero;
                SaveState.RoomState roomState = new SaveState.RoomState(
                    room.GridPosition,
                    room.Type,
                    room.PlacedMonsters,
                    hasLinked,
                    linkedPos
                );
                saveState.savedRooms.Add(roomState);
            }
        }

        Debug.Log($"Saved {saveState.savedRooms.Count} rooms to save state");
    }

    /// <summary>
    /// Restores room state from the save state object
    /// </summary>
    public void RestoreRoomState(SaveState saveState)
    {
        Debug.Log("Restoring room state...");

        // Destroy all current room GameObjects
        foreach (var room in placedRooms.Values)
        {
            if (room != null && room.gameObject != null)
            {
                Destroy(room.gameObject);
            }
        }

        // Clear the placed rooms dictionary
        placedRooms.Clear();

        // Recreate rooms from saved state (without deducting cost)
        foreach (var roomState in saveState.savedRooms)
        {
            bool placed = PlaceRoom(roomState.gridPosition, roomState.roomType, false); // false = don't deduct cost

            if (placed)
            {
                // Get the newly placed room from dictionary
                Room restoredRoom = GetRoomAt(roomState.gridPosition);

                if (restoredRoom != null)
                {
                    // Restore monsters in the room
                    foreach (var monster in roomState.placedMonsters)
                    {
                        restoredRoom.PlaceMonster(monster);
                    }
                }
            }
        }

        Debug.Log($"Restored {placedRooms.Count} rooms from save state");
    }
}