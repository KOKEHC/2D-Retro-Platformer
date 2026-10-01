using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public static float HorizontalInput { get; private set; }
    public static float VerticalInput { get; private set; }

    [Header("Ссылки на UI")]
    public RectTransform joystickBackground; // Фон джойстика (большой круг)
    public RectTransform joystickHandle;     // Ручка джойстика (маленький круг)

    private Vector2 inputVector;

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground,
            eventData.position,
            eventData.pressEventCamera,
            out pos))
        {
            // Вычисляем координаты смещения пальца от центра джойстика
            pos.x = (pos.x / joystickBackground.sizeDelta.x);
            pos.y = (pos.y / joystickBackground.sizeDelta.y);

            inputVector = new Vector2(pos.x * 2, pos.y * 2);
            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

            // Двигаем визуальную ручку джойстика на экране
            joystickHandle.anchoredPosition = new Vector2(
                inputVector.x * (joystickBackground.sizeDelta.x / 2),
                inputVector.y * (joystickBackground.sizeDelta.y / 2)
            );
            // Передаем значения осей (от -1 до 1) в глобальные переменные
            HorizontalInput = inputVector.x;
            VerticalInput = inputVector.y;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Сбрасываем значения и возвращаем ручку в центр при отпускании пальца
        inputVector = Vector2.zero;
        joystickHandle.anchoredPosition = Vector2.zero;
        HorizontalInput = 0f;
        VerticalInput = 0f;
    }
}