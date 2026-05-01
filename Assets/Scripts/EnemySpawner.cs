using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab; // Шаблон ворога
    public Transform player; // За ким їм полювати
    
    public float spawnInterval = 1.5f; // Кожні 1.5 секунди з'являється ворог
    public float spawnRadius = 15f; // На якій відстані від гравця (щоб вони з'являлися за межами екрану)

    private float timer;

    void Update()
    {
        if (player == null || enemyPrefab == null) return;

        timer += Time.deltaTime;
        
        // Якщо прийшов час — створюємо ворога
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        // Вибираємо випадковий кут (від 0 до 360 градусів) навколо гравця
        float angle = Random.Range(0f, 360f);
        
        // Вираховуємо математичну точку на цьому колі
        Vector3 spawnPosition = player.position + new Vector3(
            Mathf.Cos(angle) * spawnRadius,
            0f,
            Mathf.Sin(angle) * spawnRadius
        );

        spawnPosition.y = 1f; // Щоб ворог не з'явився високо в небі чи під землею

        // Створюємо ворога в цій точці
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        
        // Шукаємо на ньому наш скрипт Enemy і кажемо йому: "Ось твій гравець, біжи за ним!"
        Enemy enemyScript = newEnemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.player = player;
        }
    }
}
