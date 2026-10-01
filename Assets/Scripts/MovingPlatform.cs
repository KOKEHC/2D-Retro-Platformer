using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Путь платформы")]
    public Transform[] waypoints;    // Ваши независимые точки на сцене
    public float speed = 3f;         // Скорость платформы

    private int targetIndex = 0;

    void Start()
    {
        // При старте игры мгновенно телепортируем платформу в точку А,
        // чтобы она не летела туда через всю карту.
        if (waypoints.Length > 0 && waypoints[0] != null)
        {
            transform.position = waypoints[0].position;
        }
    }

    void Update()
    {
        if (waypoints.Length == 0) return;

        // Плавное движение к текущей точке
        transform.position = Vector3.MoveTowards(transform.position, waypoints[targetIndex].position, speed * Time.deltaTime);

        // Если долетели — меняем цель на следующую
        if (Vector3.Distance(transform.position, waypoints[targetIndex].position) < 0.05f)
        {
            targetIndex = (targetIndex + 1) % waypoints.Length;
        }
    }

    // Физика катания игрока на платформе
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.contacts[0].normal.y < -0.5f)
            {
                collision.transform.SetParent(transform);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}