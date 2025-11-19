using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChampionPathfinder : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveDelay = 1f;  // Delay between room movements

    private Champion champion;
    private Stack<Room> pathStack = new Stack<Room>();
    private HashSet<Room> visitedRooms = new HashSet<Room>();
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

            // Check if reached treasure room
            if (currentRoom.Type == RoomType.Treasure)
            {
                Debug.Log($"{champion.Data.championName} found the treasure room!");
                OnTreasureFound();
                yield break;
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
                champion.MoveToRoom(nextRoom);
                currentRoom = nextRoom;

            }
            else
            {
                // Dead end - backtrack
                if (pathStack.Count > 0)
                {
                    Room previousRoom = pathStack.Pop();
                    Debug.Log($"{champion.Data.championName} backtracking from {currentRoom.GridPosition} to {previousRoom.GridPosition}");
                    champion.MoveToRoom(previousRoom);
                    currentRoom = previousRoom;
                }
                else
                {
                    // No more rooms to explore
                    Debug.LogWarning("Champion explored all reachable rooms but didn't find treasure!");
                    yield break;
                }
            }
            yield return new WaitForSeconds(moveDelay);
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

    private void OnTreasureFound()
    {
        isExploring = false;
        Debug.Log($"{champion.Data.championName} found the treasure! Now returning to entrance...");

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

        // Remember treasure room to convert it when leaving
        Room treasureRoom = champion.CurrentRoom;

        // Find shortest path from current room to entrance using BFS
        List<Room> pathToEntrance = FindShortestPath(champion.CurrentRoom, entranceRoom);

        if (pathToEntrance == null || pathToEntrance.Count == 0)
        {
            Debug.LogError("No path found to entrance!");
            yield break;
        }

        // Follow the path back to entrance
        bool firstMove = true;
        foreach (Room nextRoom in pathToEntrance)
        {
            if (firstMove && treasureRoom != null && treasureRoom.Type == RoomType.Treasure)
            {
                Sprite battleSprite = RoomManager.Instance.GetRoomSprite(RoomType.Battle);
                if (battleSprite != null)
                {
                    treasureRoom.ChangeRoomType(RoomType.Battle, battleSprite);
                }
                firstMove = false;
            }
            Debug.Log($"{champion.Data.championName} returning to entrance: moving to {nextRoom.GridPosition}");
            champion.MoveToRoom(nextRoom);
            yield return new WaitForSeconds(moveDelay);

            // Convert treasure room to battle room after first move (leaving treasure room)
            
        }

        // Reached entrance
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
            DefeatUI.Instance.ShowDefeat();
        }
    }

    public bool IsExploring => isExploring;
}
