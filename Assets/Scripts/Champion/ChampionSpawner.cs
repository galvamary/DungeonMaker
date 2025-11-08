using UnityEngine;

public class ChampionSpawner : MonoBehaviour
{
    public static ChampionSpawner Instance { get; private set; }

    [Header("Champion Settings")]
    [SerializeField] private ChampionData[] availableChampions;
    [SerializeField] private Transform championContainer;

    private Champion currentChampion;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Create container for champions
        if (championContainer == null)
        {
            GameObject container = new GameObject("Champions");
            championContainer = container.transform;
        }
    }

    public Champion SpawnChampionAtEntrance()
    {
        // Find the entrance room using RoomManager
        Room entranceRoom = RoomManager.Instance?.GetEntranceRoom();
        if (entranceRoom == null)
        {
            Debug.LogError("No entrance room found! Cannot spawn champion.");
            return null;
        }

        // Select a random champion (or first one if available)
        if (availableChampions == null || availableChampions.Length == 0)
        {
            Debug.LogError("No champions available to spawn!");
            return null;
        }

        ChampionData selectedChampion = availableChampions[Random.Range(0, availableChampions.Length)];

        // Create champion GameObject
        GameObject championObj = new GameObject("Champion");
        championObj.transform.SetParent(championContainer);

        // Add Champion component and initialize
        Champion champion = championObj.AddComponent<Champion>();
        champion.Initialize(selectedChampion, entranceRoom);

        currentChampion = champion;

        Debug.Log($"Spawned champion '{selectedChampion.championName}' at entrance room");

        return champion;
    }

    public Champion GetCurrentChampion()
    {
        return currentChampion;
    }

    public void ClearCurrentChampion()
    {
        if (currentChampion != null)
        {
            Destroy(currentChampion.gameObject);
            currentChampion = null;
        }
    }
}
