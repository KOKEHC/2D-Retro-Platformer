using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Кому следовать")]
    public Transform target;

    [Header("Настройки плавности")]
    [Tooltip("Время, за которое камера догоняет игрока. Чем меньше, тем быстрее.")]
    public float smoothTime = 0.15f; 

    [Header("Смещение")]
    public Vector3 offset = new Vector3(0f, 1f, -10f);

    private Vector3 velocity = Vector3.zero; // Внутренняя переменная для работы физики сглаживания

    void FixedUpdate()
    {
        if (target != null)
        {
            // Желаемая позиция камеры
            Vector3 targetPosition = target.position + offset;

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }
}