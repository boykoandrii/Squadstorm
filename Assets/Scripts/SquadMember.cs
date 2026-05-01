using System.Collections.Generic;
using UnityEngine;

public class SquadMember : MonoBehaviour
{
    public Transform player; // Посилання на гравця
    public float speed = 4f; 
    public float stoppingDistance = 2f; 
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
            
            // 1. Рух до гравця
            Vector3 playerPosXZ = new Vector3(player.position.x, transform.position.y, player.position.z);
            float distanceToPlayer = Vector3.Distance(transform.position, playerPosXZ);
            
            if (distanceToPlayer > stoppingDistance)
            {
                moveDirection = (playerPosXZ - transform.position).normalized;
            }

            // --- КУДИ ДИВИТЬСЯ СОЛДАТ ---
            if (isAiming)
            {
                // Якщо є наказ цілитись — дивимося на маркер прицілу
                Vector3 lookPos = new Vector3(aimTarget.x, transform.position.y, aimTarget.z);
                transform.LookAt(lookPos);
            }
            else if (moveDirection != Vector3.zero)
            {
                // Якщо не цілимося, але біжимо — дивимося вперед
                transform.LookAt(playerPosXZ);
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
