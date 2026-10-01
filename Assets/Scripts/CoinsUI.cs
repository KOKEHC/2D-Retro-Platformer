using UnityEngine;
using TMPro; // Подключаем библиотеку для текста TextMeshPro

public class CoinsUI : MonoBehaviour
{
    public CoinCollector collector; // Ссылка на игрока (его скрипт CoinCollector)
    public TMP_Text scoreText;      // Ссылка на компонент текста на экране

    void Update()
    {
        if (collector == null || scoreText == null) return;

        // Выводим текст в вашем точном формате: "Очки: Собрано/Всего" (например, "Очки: 1/5")
        scoreText.text = $"Points: {collector.CollectedCoins}/{collector.TotalCoinsInLevel}";
    }
}