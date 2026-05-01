using System.Collections.Generic;
using UnityEngine;

public class SquadMember : MonoBehaviour
{
    public Transform player; // Посилання на гравця
    public float speed = 4f; // Швидкість бота
    public float stoppingDistance = 2f; // Відстань, на якій бот зупиняється біля гравця
    public float separationDistance = 1.5f; // Мінімальна відстань між солдатами
    
    public float jumpForce = 5f; // Сила стрибка
    public float gravity = -15f; // Гравітація
    
    private float yVelocity = 0f;
    private bool shouldJump = false; // Чи отримав наказ стрибати
    private CharacterController controller;

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

            // 3. Змішуємо два рухи (по горизонталі)
            if (moveDirection != Vector3.zero || separationDirection != Vector3.zero)
            {
                horizontalMove = (moveDirection + separationDirection).normalized * speed;
            }
        }

        // 4. Логіка стрибка з CharacterController
        if (controller.isGrounded)
        {
            yVelocity = -0.5f; // Легке притискання до землі

            // Якщо отримали наказ стрибнути
            if (shouldJump)
            {
                yVelocity = jumpForce;
                shouldJump = false; // Скидаємо прапорець, бо вже стрибнули
            }
        }
        else
        {
            // Якщо в повітрі — падаємо вниз
            yVelocity += gravity * Time.deltaTime;
        }

        // Збираємо горизонтальний і вертикальний рух разом
        Vector3 finalVelocity = horizontalMove;
        finalVelocity.y = yVelocity;

        // Рухаємо бота з урахуванням стін та кубів!
        controller.Move(finalVelocity * Time.deltaTime);
    }

    // Метод, який викликає командир
    public void Jump()
    {
        shouldJump = true; // Запам'ятовуємо наказ
    }
}
