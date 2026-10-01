using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [Header("Настройки времени")]
    [SerializeField] private float fallDelay = 0.5f; // Задержка перед падением

    private Rigidbody2D rb;
    private bool isFalling = false;

    void Start()
    {
        // Получаем ссылку на Rigidbody 2D платформы
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. Проверяем, наступил ли игрок на платформу сверху
        if (collision.gameObject.CompareTag("Player") && !isFalling)
        {
            // Проверка, что игрок находится именно сверху платформы (не задел сбоку/снизу)
            if (collision.contacts[0].normal.y < -0.5f)
            {
                StartCoroutine(FallRoutine());
            }
        }

        // 2. Проверяем столкновение с шипами
        if (collision.gameObject.CompareTag("DeadlySpikes"))
        {
            Destroy(gameObject); // Уничтожаем платформу
        }
    }

    private IEnumerator FallRoutine()
    {
        isFalling = true;

        // Ждем указанное время (0.5 секунды)
        yield return new WaitForSeconds(fallDelay);

        // Переключаем тип Rigidbody на Dynamic, чтобы сработала гравитация и платформа упала
        rb.bodyType = RigidbodyType2D.Dynamic;
    }
}