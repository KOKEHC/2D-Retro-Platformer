using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [Header("Настройки предмета")]
    [Tooltip("Сколько очков дает этот предмет при сборе")]

    [Header("Звуки")]
    public AudioClip collectSound; // Звук подбора предмета
    public int pointsValue = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Проверяем, коснулся ли предмета Игрок
        if (collision.CompareTag("Player"))
        {
            // Находим компонент сборщика на игроке
            CoinCollector collector = collision.GetComponent<CoinCollector>();
            if (collector != null)
            {
                collector.AddCoins(pointsValue); // Добавляем монетку в счетчик игрока

                if (collectSound != null)
                {
                    AudioSource.PlayClipAtPoint(collectSound, Camera.main.transform.position, 1f);
                }

                Destroy(gameObject); // Уничтожаем монетку со сцены
            }
        }
    }
}