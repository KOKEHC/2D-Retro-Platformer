using UnityEngine;
using UnityEngine.UI;

public class HeartsUI : MonoBehaviour
{
    public PlayerController player;  // Ссылка на игрока
    public Image[] hearts;         // Массив объектов сердечек на экране Canvas

    void Update()
    {
        if (player == null) return;

        for (int i = 0; i < hearts.Length; i++)
        {
            // Если индекс сердечка меньше текущего здоровья — показываем его,
            // иначе — полностью отключаем объект, и он исчезает с экрана.
            if (i < player.CurrentHealth)
            {
                hearts[i].gameObject.SetActive(true); // Включаем картинку
            }
            else
            {
                hearts[i].gameObject.SetActive(false); // Полностью скрываем картинку
            }
        }
    }
}