using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // За ким слідувати (Командир)
    public Vector3 offset; // Відстань камери від гравця (висота і кут)
    public float smoothSpeed = 10f; // Наскільки плавно камера наздоганяє гравця

    void Start()
    {
        // Автоматично запам'ятовуємо відступ камери від гравця на старті сцени
        if (target != null && offset == Vector3.zero)
        {
            offset = transform.position - target.position;
        }
    }

    // LateUpdate викликається щокадру, але суворо ПІСЛЯ того, як гравець вже походив
    void LateUpdate()
    {
        if (target != null)
        {
            // Ідеальна точка, де зараз має знаходитися камера
            Vector3 desiredPosition = target.position + offset;
            
            // Плавно "підтягуємо" камеру до цієї точки
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
    }
}
