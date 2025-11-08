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
                // TODO: Implement combat
                yield return new WaitForSeconds(moveDelay);
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

                yield return new WaitForSeconds(moveDelay);
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

                    yield return new WaitForSeconds(moveDelay);
                }
                else
                {
                    // No more rooms to explore
                    Debug.LogWarning("Champion explored all reachable rooms but didn't find treasure!");
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

    private void OnTreasureFound()
    {
        isExploring = false;
        Debug.Log($"{champion.Data.championName} successfully completed the dungeon!");

        // TODO: Show victory UI, give rewards, etc.
    }

    public bool IsExploring => isExploring;
}
