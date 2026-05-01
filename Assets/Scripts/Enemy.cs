using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 3; // Скільки куль треба, щоб його вбити
    public float speed = 2.5f; // Швидкість бігу зомбі/ворога
    
    [HideInInspector]
    public Transform player; // Ціль (автоматично дається Спавнером)
    
    // Зберігаємо всі частини 3D-моделі та їхні кольори
    private Renderer[] renderers;
    private Color[] originalColors;

    void Start()
    {
        // Збираємо всі малювалки (навіть якщо 3D-модель складається з багатьох частин)
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material != null)
            {
                originalColors[i] = renderers[i].material.color;
            }
        }
    }

    void Update()
    {
        // Якщо ворог знає, де гравець — він до нього біжить!
        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0; // Щоб він не літав, якщо ми підстрибнули
            
            transform.position += direction * speed * Time.deltaTime;
            
            // Дивиться прямо на гравця
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        
        // Робимо всі частини ворога червоними
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null)
            {
                renderers[i].material.color = Color.red;
            }
        }
        
        Invoke("ResetColor", 0.1f); 

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    void ResetColor()
    {
        // Повертаємо оригінальні кольори
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null)
            {
                renderers[i].material.color = originalColors[i];
            }
        }
    }
}
