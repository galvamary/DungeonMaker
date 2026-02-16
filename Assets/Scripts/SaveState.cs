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

        public RoomState(Vector2Int pos, RoomType type, List<MonsterData> monsters)
        {
            gridPosition = pos;
            roomType = type;
            placedMonsters = new List<MonsterData>(monsters);
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
