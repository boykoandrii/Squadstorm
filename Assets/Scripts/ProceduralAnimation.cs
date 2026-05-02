using UnityEngine;

public class ProceduralAnimation : MonoBehaviour
{
    [Header("Налаштування анімації")]
    public float walkSpeed = 10f; // Наскільки швидко рухаються "ноги"
    public float wobbleAmount = 0.04f; // Сила підстрибування/зтискання
    
    private Vector3 originalScale;
    private float wobbleTimer = 0f;
    private Vector3 lastPosition;

    void Start()
    {
        // Запам'ятовуємо нормальний розмір та початкову позицію
        originalScale = transform.localScale;
        lastPosition = transform.position;
    }

    void Update()
    {
        // Рахуємо реальну пройдену відстань за кадр (лише по горизонталі)
        Vector3 currentPosXZ = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 lastPosXZ = new Vector3(lastPosition.x, 0, lastPosition.z);
        
        float distanceMoved = Vector3.Distance(currentPosXZ, lastPosXZ);
        float speed = distanceMoved / Time.deltaTime; // Реальна швидкість об'єкта
        
        lastPosition = transform.position; // Оновлюємо позицію для наступного кадру

        // Якщо швидкість більша за мінімальну
        if (speed > 0.5f)
        {
            wobbleTimer += Time.deltaTime * walkSpeed;
            
            // Математика для ефекту дихання/кроків
            float yWobble = Mathf.Sin(wobbleTimer) * wobbleAmount; 
            float xzWobble = Mathf.Cos(wobbleTimer) * (wobbleAmount * 0.5f); 

            transform.localScale = new Vector3(
                originalScale.x - xzWobble, 
                originalScale.y + yWobble, 
                originalScale.z - xzWobble
            );
        }
        else
        {
            // Плавно повертаємо до нормального розміру
            wobbleTimer = 0f;
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * 15f);
        }
    }
}
