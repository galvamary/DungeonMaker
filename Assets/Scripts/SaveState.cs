using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores a snapshot of the game state at the start of exploration
/// Used to restore the game state when the player is defeated
/// </summary>
[Serializable]
public class SaveState
{
    // GameManager state
    public int savedGold;
    public int savedReputation;

    // RoomManager state
    [Serializable]
    public class RoomState
    {
        public Vector2Int gridPosition;
        public RoomType roomType;
        public List<MonsterData> placedMonsters;
        public bool hasLinkedRoom = false;
        public Vector2Int linkedRoomPosition;  // Lock-Key 연결

        public RoomState(Vector2Int pos, RoomType type, List<MonsterData> monsters, bool hasLinked = false, Vector2Int linkedPos = default)
        {
            gridPosition = pos;
            roomType = type;
            placedMonsters = new List<MonsterData>(monsters);
            hasLinkedRoom = hasLinked;
            linkedRoomPosition = linkedPos;
        }
    }
    public List<RoomState> savedRooms;

    // ShopManager state (inventory)
    public List<MonsterData> savedInventory;

    public SaveState()
    {
        savedRooms = new List<RoomState>();
        savedInventory = new List<MonsterData>();
    }

    public void Clear()
    {
        savedGold = 0;
        savedReputation = 0;
        savedRooms.Clear();
        savedInventory.Clear();
    }
}
