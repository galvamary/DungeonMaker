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

    [Header("Camera References")]
    [SerializeField] private CameraController cameraController;

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

        currentPhase = GamePhase.Exploration;
        Debug.Log("Starting exploration phase!");

        // Disable preparation UI
        if (shopUI != null) shopUI.SetActive(false);
        if (monsterInventoryUI != null) monsterInventoryUI.SetActive(false);
        if (startButton != null) startButton.SetActive(false);

        // Disable grid click handler (but keep grid visible)
        GridClickHandler clickHandler = FindObjectOfType<GridClickHandler>();
        if (clickHandler != null) clickHandler.enabled = false;

        // Disable camera movement
        if (cameraController != null) cameraController.enabled = false;

        // Note: Monster dragging is disabled by checking IsPreparationPhase in MonsterInRoomDragHandler

        // TODO: Start hero spawning and pathfinding
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

        SetPreparationPhase();
    }

    private void SetPreparationPhase()
    {
        // Enable preparation UI
        if (shopUI != null) shopUI.SetActive(true);
        if (monsterInventoryUI != null) monsterInventoryUI.SetActive(true);
        if (startButton != null) startButton.SetActive(true);

        // Enable grid click handler
        GridClickHandler clickHandler = FindObjectOfType<GridClickHandler>();
        if (clickHandler != null) clickHandler.enabled = true;

        // Enable camera movement
        if (cameraController != null) cameraController.enabled = true;

        // Note: Monster dragging is automatically enabled when IsPreparationPhase is true
    }
}
