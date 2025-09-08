using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [SerializeField] private int currentGold = 1000;
    
    public int CurrentGold => currentGold;
    
    public delegate void GoldChangedDelegate(int newGold);
    public event GoldChangedDelegate OnGoldChanged;
    
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
    
    public bool SpendGold(int amount)
    {
        if (amount <= 0 || currentGold < amount)
        {
            return false;
        }
        
        currentGold -= amount;
        OnGoldChanged?.Invoke(currentGold);
        return true;
    }
    
    public void AddGold(int amount)
    {
        if (amount > 0)
        {
            currentGold += amount;
            OnGoldChanged?.Invoke(currentGold);
        }
    }
    
    public bool CanAfford(int amount)
    {
        return currentGold >= amount;
    }
}