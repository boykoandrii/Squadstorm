using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    private RectTransform background;
    private RectTransform handle;
    
    [HideInInspector]
    public Vector2 inputVector;

    void Start()
    {
        background = GetComponent<RectTransform>();
        // Беремо перший дочірній об'єкт як "ручку" джойстика
        handle = transform.GetChild(0).GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 position;
        // Визначаємо, де палець торкнувся екрану відносно фону джойстика
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out position))
        {
            position.x = (position.x / background.sizeDelta.x) * 2;
            position.y = (position.y / background.sizeDelta.y) * 2;

            inputVector = new Vector2(position.x, position.y);
            // Обмежуємо вихід ручки за межі кола
            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

            // Рухаємо саму візуальну ручку
            handle.anchoredPosition = new Vector2(
                inputVector.x * (background.sizeDelta.x / 2),
                inputVector.y * (background.sizeDelta.y / 2));
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Коли відпускаємо палець — джойстик повертається в центр
        inputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }
}
