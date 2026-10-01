using UnityEngine;

public class CoinCollector : MonoBehaviour
{
    private int collectedCoins = 0;
    private int totalCoinsInLevel = 0;

    // Свойства для чтения из UI-скрипта
    public int CollectedCoins => collectedCoins;
    public int RemainingCoins => totalCoinsInLevel - collectedCoins;
    public int TotalCoinsInLevel => totalCoinsInLevel;

    void Start()
    {
        // Находим только активные на сцене монетки 
        totalCoinsInLevel = FindObjectsByType<CollectibleItem>(FindObjectsInactive.Exclude).Length;
    }

    public void AddCoins(int amount)
    {
        collectedCoins += amount;
    }
}