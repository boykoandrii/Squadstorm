using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f; // Швидкість польоту кулі
    public float lifeTime = 2f; // Через скільки секунд куля зникне

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    // Коли куля в щось врізається
    void OnTriggerEnter(Collider other)
    {
        // 1. Перевіряємо, чи ми не врізалися у свого (командира або члена загону)
        if (other.GetComponent<Squadstorm.Core.PlayerMovement>() != null || other.GetComponent<SquadMember>() != null)
        {
            return; // Це свої! Куля летить далі крізь них.
        }

        // 2. Перевіряємо, чи це ворог
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(1); // Завдаємо йому 1 одиницю шкоди
        }

        // 3. Знищуємо кулю (об стіну, об ворога, об що завгодно крім своїх)
        Destroy(gameObject);
    }
}
