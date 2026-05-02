using System.Collections.Generic;
using UnityEngine;

public class SquadMember : MonoBehaviour
{
    public Transform player; // Посилання на гравця
    public float speed = 4f; 
    [Header("Формація")]
    public float formationRadius = 2f; // На якій відстані від командира стоять солдати
    public float separationDistance = 1.5f; 
    
    [Header("Стрибки")]
    public float jumpForce = 5f; 
    public float gravity = -15f; 
    
    [Header("Стрільба")]
    public GameObject bulletPrefab; // Префаб кулі
    public float fireRate = 0.3f; // Раз на скільки секунд солдат стріляє
    private float nextFireTime = 0f;

    private float yVelocity = 0f;
    private bool shouldJump = false; 
    private CharacterController controller;
    
    [Header("Анімація")]
    public Animator animator; // Компонент для керування анімаціями
    
    private bool isAiming = false; // Чи віддав командир наказ цілитись
    private Vector3 aimTarget;     // Точка, в яку цілимось

    // Загальний список усіх солдатів
    private static List<SquadMember> allMembers = new List<SquadMember>();

    void Start()
    {
        allMembers.Add(this);
        controller = GetComponent<CharacterController>();
    }

    void OnDestroy()
    {
        allMembers.Remove(this);
    }

    void Update()
    {
        Vector3 horizontalMove = Vector3.zero;

        if (player != null)
        {
            Vector3 moveDirection = Vector3.zero;
            
            // 1. Рух на свою позицію у формації (коло навколо гравця)
            int myIndex = allMembers.IndexOf(this);
            int totalMembers = allMembers.Count;
            
            // Розраховуємо кут для цього конкретного солдата (в радіанах)
            float angle = myIndex * (Mathf.PI * 2f / Mathf.Max(1, totalMembers));
            
            // Обчислюємо зміщення відносно гравця
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * formationRadius;
            
            Vector3 playerPosXZ = new Vector3(player.position.x, transform.position.y, player.position.z);
            Vector3 targetPosition = playerPosXZ + offset;
            
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
            
            // Якщо ми ще не дійшли до своєї позиції - рухаємось до неї
            if (distanceToTarget > 0.5f)
            {
                moveDirection = (targetPosition - transform.position).normalized;
            }

            // --- КУДИ ДИВИТЬСЯ СОЛДАТ ---
            if (isAiming)
            {
                // Якщо є наказ цілитись — дивимося на маркер прицілу
                Vector3 lookPos = new Vector3(aimTarget.x, transform.position.y, aimTarget.z);
                transform.LookAt(lookPos);
            }
            else 
            {
                // Якщо не цілимося — дивимося туди ж, куди й командир (щоб завжди бачити куди стріляти)
                transform.rotation = player.rotation;
            }

            // 2. Відштовхування від сусідів
            Vector3 separationDirection = Vector3.zero;
            foreach (var member in allMembers)
            {
                if (member != this)
                {
                    float distToMember = Vector3.Distance(transform.position, member.transform.position);
                    if (distToMember < separationDistance && distToMember > 0)
                    {
                        Vector3 pushAway = transform.position - member.transform.position;
                        pushAway.y = 0;
                        separationDirection += pushAway.normalized / distToMember; 
                    }
                }
            }

            // 3. Змішуємо два рухи
            if (moveDirection != Vector3.zero || separationDirection != Vector3.zero)
            {
                horizontalMove = (moveDirection + separationDirection).normalized * speed;
            }
        }

        // 4. Логіка стрибка
        if (controller.isGrounded)
        {
            yVelocity = -0.5f; 
            if (shouldJump)
            {
                yVelocity = jumpForce;
                shouldJump = false; 
            }
        }
        else
        {
            yVelocity += gravity * Time.deltaTime;
        }

        Vector3 finalVelocity = horizontalMove;
        finalVelocity.y = yVelocity;
        controller.Move(finalVelocity * Time.deltaTime);

        // --- АНІМАЦІЯ ---
        if (animator != null)
        {
            // Передаємо швидкість (довжину вектора руху) в Animator
            animator.SetFloat("Speed", horizontalMove.magnitude);
        }
    }

    public void Jump()
    {
        shouldJump = true; 
    }

    // --- МЕТОДИ СТРІЛЬБИ ---
    public void AimAt(Vector3 target)
    {
        isAiming = true;
        aimTarget = target;
    }

    public void StopAiming()
    {
        isAiming = false;
    }

    public void TryShoot()
    {
        // Якщо пройшов час після минулого пострілу і нам дали префаб кулі
        if (Time.time >= nextFireTime && bulletPrefab != null)
        {
            nextFireTime = Time.time + fireRate;
            
            // Створюємо кулю. Вона з'являється трохи попереду і вище центру солдата.
            Vector3 spawnPos = transform.position + Vector3.up * 0.5f + transform.forward * 1f;
            Instantiate(bulletPrefab, spawnPos, transform.rotation);
        }
    }
}
