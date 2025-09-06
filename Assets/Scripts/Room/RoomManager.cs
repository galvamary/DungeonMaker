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
            
            switch (existingRoom.Type)
            {
                case RoomType.Battle:
                    // Battle -> Treasure
                    RemoveRoom(gridPosition);
                    PlaceRoom(gridPosition, RoomType.Treasure);
                    break;
                    
                case RoomType.Treasure:
                    // Treasure -> Empty
                    RemoveRoom(gridPosition);
                    break;
                    
                default:
                    // Any other type -> Empty
                    RemoveRoom(gridPosition);
                    break;
            }
        }
    }
    
    private Sprite GetRoomSprite(RoomType roomType)
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
}