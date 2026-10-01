using UnityEngine;
using UnityEngine.EventSystems;

public class MobileJumpButton : MonoBehaviour, IPointerDownHandler
{
    // Глобальный флаг нажатия прыжка
    public static bool WasPressed { get; private set; }

    public void OnPointerDown(PointerEventData eventData)
    {
        WasPressed = true;
    }

    private void LateUpdate()
    {
        // Автоматически сбрасываем флаг в конце каждого кадра,
        // чтобы симулировать одиночный щелчок (wasPressedThisFrame)
        WasPressed = false;
    }
}