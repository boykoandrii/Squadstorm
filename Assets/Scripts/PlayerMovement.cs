using UnityEngine;

namespace Squadstorm.Core
{
    public class PlayerMovement : MonoBehaviour
    {
        public float moveSpeed = 5f;
        public float jumpForce = 5f;
        public float gravity = -15f;
        
        public SimpleJoystick joystick; // Посилання на наш екранний джойстик
        
        private float yVelocity = 0f;
        private CharacterController controller;
        private bool mobileJumpRequested = false; // Чи натиснули кнопку на екрані

        void Start()
        {
            controller = GetComponent<CharacterController>();
        }

        // Цей метод ми прив'яжемо до кнопки в інтерфейсі
        public void MobileJump()
        {
            mobileJumpRequested = true;
        }

        void Update()
        {
            // Зчитуємо клавіатуру
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            // Якщо клавіатура мовчить, беремо дані з джойстика (якщо він є)
            if (joystick != null)
            {
                if (horizontal == 0) horizontal = joystick.inputVector.x;
                if (vertical == 0) vertical = joystick.inputVector.y;
            }

            // Напрямок руху
            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
            Vector3 velocity = direction * moveSpeed;

            // Обертаємо капсулу
            if (direction.magnitude >= 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.deltaTime);
            }

            // ЛОГІКА СТРИБКА
            if (controller.isGrounded)
            {
                yVelocity = -0.5f; 

                // Стрибаємо, якщо натиснули Пробіл АБО екранну кнопку
                if (Input.GetKeyDown(KeyCode.Space) || mobileJumpRequested)
                {
                    mobileJumpRequested = false; // Скидаємо натискання
                    yVelocity = jumpForce; 
                    
                    // Наказуємо всім солдатам стрибнути
                    SquadMember[] squad = FindObjectsOfType<SquadMember>();
                    foreach (var soldier in squad)
                    {
                        soldier.Jump();
                    }
                }
            }
            else
            {
                mobileJumpRequested = false; // Ігноруємо натискання в повітрі
                yVelocity += gravity * Time.deltaTime;
            }

            velocity.y = yVelocity;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}
