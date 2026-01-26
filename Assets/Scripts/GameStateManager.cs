using UnityEngine;

public enum GamePhase
{
    Preparation,  // 준비 단계: 방 배치, 몬스터 배치
    Exploration   // 탐험 단계: 영웅 침입
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [Header("Game Phase")]
    private GamePhase currentPhase = GamePhase.Preparation;

    [Header("UI References")]
    [SerializeField] private GameObject shopUI;
    [SerializeField] private GameObject monsterInventoryUI;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject shopButton;

    [Header("Camera References")]
    [SerializeField] private CameraController cameraController;

    [Header("Champion References")]
    [SerializeField] private ChampionSpawner championSpawner;
    [SerializeField] private ChampionPathfinder championPathfinder;

    public GamePhase CurrentPhase => currentPhase;
    public bool IsPreparationPhase => currentPhase == GamePhase.Preparation;

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
    }

    private void Start()
    {
        // Start in preparation phase
        SetPreparationPhase();
    }

    public void StartExploration()
    {
        if (currentPhase == GamePhase.Exploration)
        {
            Debug.Log("Already in exploration phase!");
            return;
        }

        // Check if enough treasure rooms are built based on reputation
        if (GameManager.Instance != null && RoomManager.Instance != null)
        {
            int currentReputation = GameManager.Instance.CurrentReputation;
            int treasureRoomCount = RoomManager.Instance.GetTreasureRoomCount();

            if (treasureRoomCount < currentReputation)
            {
                Debug.LogWarning($"Cannot start exploration! You must build at least {currentReputation} treasure rooms (current reputation). Currently built: {treasureRoomCount} treasure rooms.");
                return;
            }
        }

        // Check if all rooms are connected
        if (RoomManager.Instance != null && !RoomManager.Instance.AreAllRoomsConnected())
        {
            Debug.LogWarning("Cannot start exploration! All rooms must be connected to the entrance.");
            return;
        }

        currentPhase = GamePhase.Exploration;
        Debug.Log("Starting exploration phase!");

        // Disable preparation UI
        if (shopUI != null) shopUI.SetActive(false);
        if (monsterInventoryUI != null) monsterInventoryUI.SetActive(false);
        if (startButton != null) startButton.SetActive(false);
        if (shopButton != null) shopButton.SetActive(false);

        // Disable grid click handler (but keep grid visible)
        GridClickHandler clickHandler = FindObjectOfType<GridClickHandler>();
        if (clickHandler != null) clickHandler.enabled = false;

        // Note: Monster dragging is disabled by checking IsPreparationPhase in MonsterInRoomDragHandler

        // Spawn champion at entrance
        Champion champion = null;
        if (championSpawner != null)
        {
            champion = championSpawner.SpawnChampionAtEntrance();
        }
        else
        {
            Debug.LogWarning("ChampionSpawner not assigned!");
        }

        // Focus camera on champion and disable manual control
        if (cameraController != null && champion != null)
        {
            cameraController.FocusOnPosition(champion.transform.position);
            cameraController.ResetToDefaultZoom();
            cameraController.DisableManualControl();
        }

        // Enable fade image for exploration phase
        if (FadeEffect.Instance != null)
        {
            FadeEffect.Instance.EnableFadeImage();
        }

        // Start champion exploration
        if (championPathfinder != null && champion != null)
        {
            championPathfinder.StartExploration(champion);
        }
        else
        {
            Debug.LogWarning("ChampionPathfinder not assigned!");
        }
    }

    public void ReturnToPreparation()
    {
        if (currentPhase == GamePhase.Preparation)
        {
            Debug.Log("Already in preparation phase!");
            return;
        }

        currentPhase = GamePhase.Preparation;
        Debug.Log("Returning to preparation phase!");

        // Stop champion exploration
        if (championPathfinder != null)
        {
            championPathfinder.StopExploration();
        }

        // Clear current champion
        if (championSpawner != null)
        {
            championSpawner.ClearCurrentChampion();
        }

        SetPreparationPhase();
    }

    private void SetPreparationPhase()
    {
        // Enable preparation UI
        if (shopUI != null) shopUI.SetActive(false);
        if (monsterInventoryUI != null) monsterInventoryUI.SetActive(true);
        if (startButton != null) startButton.SetActive(true);
        if (shopButton != null) shopButton.SetActive(true);

        // Enable grid click handler
        GridClickHandler clickHandler = FindObjectOfType<GridClickHandler>();
        if (clickHandler != null) clickHandler.enabled = true;

        // Enable manual camera control
        if (cameraController != null)
        {
            cameraController.EnableManualControl();
        }

        // Disable fade image for preparation phase (so it doesn't block grid clicks)
        if (FadeEffect.Instance != null)
        {
            FadeEffect.Instance.DisableFadeImage();
        }

        // Note: Monster dragging is automatically enabled when IsPreparationPhase is true
    }
}
