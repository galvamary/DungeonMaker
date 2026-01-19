using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChampionPathfinder : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speedCurveExponent = 2f;  // Controls acceleration curve (higher = faster acceleration)
    [SerializeField] private float speedDivisor = 20f;  // Controls when speed starts ramping up (lower = earlier)

    private Champion champion;
    private Stack<Room> pathStack = new Stack<Room>();
    private HashSet<Room> visitedRooms = new HashSet<Room>();
    private HashSet<Room> visitedTreasureRooms = new HashSet<Room>();
    private int totalTreasureRooms = 0;
    private bool isExploring = false;

    public void StartExploration(Champion targetChampion)
    {
        if (targetChampion == null)
        {
            Debug.LogError("No champion to explore with!");
            return;
        }

        champion = targetChampion;
        pathStack.Clear();
        visitedRooms.Clear();
        visitedTreasureRooms.Clear();

        // Count total treasure rooms in the dungeon
        totalTreasureRooms = RoomManager.Instance?.GetTreasureRoomCount() ?? 0;
        Debug.Log($"Total treasure rooms in dungeon: {totalTreasureRooms}");

        isExploring = true;

        StartCoroutine(ExploreCoroutine());
    }

    public void StopExploration()
    {
        isExploring = false;
        StopAllCoroutines();
    }

    private IEnumerator ExploreCoroutine()
    {
        Room currentRoom = champion.CurrentRoom;

        while (isExploring && champion != null && champion.IsAlive)
        {
            

            // Mark current room as visited
            visitedRooms.Add(currentRoom);

            // Check if current room is a treasure room
            if (currentRoom.Type == RoomType.Treasure && !visitedTreasureRooms.Contains(currentRoom))
            {
                visitedTreasureRooms.Add(currentRoom);
                Debug.Log($"{champion.Data.championName} found treasure room {visitedTreasureRooms.Count}/{totalTreasureRooms} at {currentRoom.GridPosition}!");
                ConvertTreasureToBattle(currentRoom);

                // Check if all treasure rooms have been visited
                if (visitedTreasureRooms.Count >= totalTreasureRooms)
                {
                    Debug.Log($"{champion.Data.championName} found all treasure rooms! Now returning to entrance...");

                    OnAllTreasuresFound();
                    yield break;
                }
            }

            // Handle combat if there are monsters
            if (currentRoom.HasMonster)
            {
                Debug.Log($"{champion.Data.championName} encountered monsters in room at {currentRoom.GridPosition}!");

                // Start battle
                if (BattleManager.Instance != null)
                {
                    BattleManager.Instance.StartBattle(champion, currentRoom);

                    // Wait for battle to finish
                    yield return new WaitUntil(() => !BattleManager.Instance.IsBattleActive);

                    // Check if champion survived
                    if (!champion.IsAlive)
                    {
                        Debug.Log($"{champion.Data.championName} was defeated in battle!");

                        // Restore all treasure rooms since champion died
                        RestoreTreasureRooms();

                        VictoryUI.Instance.ShowVictory(champion);
                        yield break;
                    }
                }
            }

            // Get unvisited adjacent rooms
            List<Room> unvisitedNeighbors = GetUnvisitedAdjacentRooms(currentRoom);

            if (unvisitedNeighbors.Count > 0)
            {
                // Choose random unvisited room
                Room nextRoom = unvisitedNeighbors[Random.Range(0, unvisitedNeighbors.Count)];

                // Remember current path
                pathStack.Push(currentRoom);

                // Move to next room
                Debug.Log($"{champion.Data.championName} exploring from {currentRoom.GridPosition} to {nextRoom.GridPosition}");
                float speedMultiplier = CalculateSpeedMultiplier();
                yield return champion.MoveToRoom(nextRoom, speedMultiplier);
                currentRoom = nextRoom;

            }
            else
            {
                // Dead end - backtrack
                if (pathStack.Count > 0)
                {
                    Room previousRoom = pathStack.Pop();
                    Debug.Log($"{champion.Data.championName} backtracking from {currentRoom.GridPosition} to {previousRoom.GridPosition}");
                    float speedMultiplier = CalculateSpeedMultiplier();
                    yield return champion.MoveToRoom(previousRoom, speedMultiplier);
                    currentRoom = previousRoom;
                }
                else
                {
                    // No more rooms to explore
                    if (visitedTreasureRooms.Count < totalTreasureRooms)
                    {
                        Debug.LogWarning($"{champion.Data.championName} explored all reachable rooms but only found {visitedTreasureRooms.Count}/{totalTreasureRooms} treasure rooms!");
                    }

                    // If at least one treasure was found, return to entrance
                    if (visitedTreasureRooms.Count > 0)
                    {
                        Debug.Log($"{champion.Data.championName} returning to entrance with {visitedTreasureRooms.Count} treasures...");
                        OnAllTreasuresFound();
                    }
                    yield break;
                }
            }
        }
    }

    private List<Room> GetUnvisitedAdjacentRooms(Room currentRoom)
    {
        List<Room> adjacentRooms = new List<Room>();
        Vector2Int currentPos = currentRoom.GridPosition;

        // Check all 4 directions
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // Up
            new Vector2Int(0, -1),  // Down
            new Vector2Int(-1, 0),  // Left
            new Vector2Int(1, 0)    // Right
        };

        foreach (var direction in directions)
        {
            Vector2Int neighborPos = currentPos + direction;
            Room neighborRoom = RoomManager.Instance?.GetRoomAtPosition(neighborPos);

            if (neighborRoom != null && !visitedRooms.Contains(neighborRoom))
            {
                adjacentRooms.Add(neighborRoom);
            }
        }

        return adjacentRooms;
    }

    /// <summary>
    /// Converts a treasure room to battle room if it was visited
    /// </summary>
    private void ConvertTreasureToBattle(Room room)
    {
        if (room == null)
        {
            Debug.Log("ConvertTreasureToBattle: room is null");
            return;
        }

        Debug.Log($"ConvertTreasureToBattle: Checking room at {room.GridPosition}, Type: {room.Type}, InVisitedList: {visitedTreasureRooms.Contains(room)}");

        if (visitedTreasureRooms.Contains(room) && room.Type == RoomType.Treasure)
        {
            Sprite battleSprite = RoomManager.Instance.GetRoomSprite(RoomType.Battle);
            if (battleSprite != null)
            {
                room.ChangeRoomType(RoomType.Battle, battleSprite);
                Debug.Log($"✓ Converted treasure room at {room.GridPosition} to battle room (champion leaving)");
            }
            else
            {
                Debug.LogError("ConvertTreasureToBattle: battleSprite is null!");
            }
        }
        else
        {
            Debug.Log($"ConvertTreasureToBattle: Skipping room at {room.GridPosition} - Not in visited list or not treasure type");
        }
    }

    /// <summary>
    /// Restores all converted treasure rooms back to treasure type
    /// Called when champion dies during exploration
    /// </summary>
    private void RestoreTreasureRooms()
    {
        Sprite treasureSprite = RoomManager.Instance.GetRoomSprite(RoomType.Treasure);
        if (treasureSprite != null)
        {
            foreach (Room room in visitedTreasureRooms)
            {
                if (room != null && room.Type == RoomType.Battle)
                {
                    room.ChangeRoomType(RoomType.Treasure, treasureSprite);
                    Debug.Log($"Restored battle room at {room.GridPosition} back to treasure room");
                }
            }
        }
    }

    private void OnAllTreasuresFound()
    {
        isExploring = false;
        Debug.Log($"{champion.Data.championName} collected all treasures ({visitedTreasureRooms.Count})! Now returning to entrance...");

        // Start returning to entrance using shortest path
        StartCoroutine(ReturnToEntranceCoroutine());
    }

    private IEnumerator ReturnToEntranceCoroutine()
    {
        Room entranceRoom = RoomManager.Instance.GetEntranceRoom();
        if (entranceRoom == null)
        {
            Debug.LogError("Cannot find entrance room to return to!");
            yield break;
        }

        // Find shortest path from current room to entrance using BFS
        List<Room> pathToEntrance = FindShortestPath(champion.CurrentRoom, entranceRoom);

        if (pathToEntrance == null || pathToEntrance.Count == 0)
        {
            Debug.LogError("No path found to entrance!");
            yield break;
        }

        // Follow the path back to entrance
        foreach (Room nextRoom in pathToEntrance)
        {
            Debug.Log($"{champion.Data.championName} returning to entrance: moving to {nextRoom.GridPosition}");
            float speedMultiplier = CalculateSpeedMultiplier();
            yield return champion.MoveToRoom(nextRoom, speedMultiplier);
        }

        // Reached entrance - all treasure rooms should already be converted to battle rooms
        OnDungeonCompleted();
    }

    private List<Room> FindShortestPath(Room start, Room end)
    {
        // BFS to find shortest path
        Queue<Room> queue = new Queue<Room>();
        Dictionary<Room, Room> cameFrom = new Dictionary<Room, Room>();
        HashSet<Room> visited = new HashSet<Room>();

        queue.Enqueue(start);
        visited.Add(start);
        cameFrom[start] = null;

        while (queue.Count > 0)
        {
            Room current = queue.Dequeue();

            // Found the target
            if (current == end)
            {
                return ReconstructPath(cameFrom, start, end);
            }

            // Check all adjacent rooms
            Vector2Int currentPos = current.GridPosition;
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(0, 1),   // Up
                new Vector2Int(0, -1),  // Down
                new Vector2Int(-1, 0),  // Left
                new Vector2Int(1, 0)    // Right
            };

            foreach (var direction in directions)
            {
                Vector2Int neighborPos = currentPos + direction;
                Room neighborRoom = RoomManager.Instance?.GetRoomAtPosition(neighborPos);

                if (neighborRoom != null && !visited.Contains(neighborRoom))
                {
                    visited.Add(neighborRoom);
                    cameFrom[neighborRoom] = current;
                    queue.Enqueue(neighborRoom);
                }
            }
        }

        // No path found
        return null;
    }

    private List<Room> ReconstructPath(Dictionary<Room, Room> cameFrom, Room start, Room end)
    {
        List<Room> path = new List<Room>();
        Room current = end;

        // Build path backwards from end to start
        while (current != start)
        {
            path.Add(current);
            current = cameFrom[current];
        }

        // Reverse to get path from start to end
        path.Reverse();

        return path;
    }

    private void OnDungeonCompleted()
    {
        Debug.Log($"{champion.Data.championName} successfully completed the dungeon and returned to entrance!");
        Debug.Log("Player defeated! Champion escaped with the treasure!");

        // Show defeat UI
        if (DefeatUI.Instance != null)
        {
            DefeatUI.Instance.ShowDefeat(champion);
        }
    }

    public bool IsExploring => isExploring;

    /// <summary>
    /// Calculate movement speed multiplier based on number of visited rooms
    /// Uses exponential curve: speedMultiplier = 1 + (visitedCount / speedDivisor) ^ speedCurveExponent
    /// Maximum speed multiplier is capped at 3.0x
    /// </summary>
    private float CalculateSpeedMultiplier()
    {
        int visitedCount = visitedRooms.Count;

        // Exponential speed increase: starts slow, accelerates rapidly
        // Example with default values (exponent=2, divisor=10):
        // 1 room: 1.01x, 5 rooms: 1.25x, 10 rooms: 2.0x, 20 rooms: 5.0x, 30 rooms: 10.0x
        float speedMultiplier = 1f + Mathf.Pow(visitedCount / speedDivisor, speedCurveExponent);

        // Cap maximum speed at 3.0x
        speedMultiplier = Mathf.Min(speedMultiplier, 2f);

        Debug.Log($"Speed multiplier: {speedMultiplier:F2}x (visited {visitedCount} rooms)");

        return speedMultiplier;
    }
}
