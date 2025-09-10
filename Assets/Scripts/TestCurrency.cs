using UnityEngine;

public class TestCurrency : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            GameManager.Instance.AddGold(100);
            Debug.Log($"Added 100 gold. Current: {GameManager.Instance.CurrentGold}");
        }
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            bool success = GameManager.Instance.SpendGold(50);
            if (success)
            {
                Debug.Log($"Spent 50 gold. Current: {GameManager.Instance.CurrentGold}");
            }
            else
            {
                Debug.Log("Not enough gold!");
            }
        }
    }
}