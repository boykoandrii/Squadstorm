using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f; // Швидкість польоту кулі
    public float lifeTime = 2f; // Через скільки секунд куля зникне, щоб не забивати пам'ять

    void Start()
    {
        // Запускаємо таймер самознищення
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Куля просто летить прямо
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // Якщо куля врізається в щось (крім наших солдатів), вона руйнується
        if (!other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
