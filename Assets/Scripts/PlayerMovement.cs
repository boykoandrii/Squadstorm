using UnityEngine;

namespace Squadstorm.Core
{
    public class PlayerMovement : MonoBehaviour
    {
        public float moveSpeed = 5f;
        public float jumpForce = 5f;
        public float gravity = -15f;
        
        [Header("Керування (Twin-Stick)")]
        public SimpleJoystick moveJoystick; // Лівий джойстик (біг)
        public SimpleJoystick aimJoystick;  // Правий джойстик (приціл і стрільба)
        
        [Header("Приціл")]
        public Transform aimCursor; // Маркер на землі (червоний кружечок)
        public float aimDistance = 8f; // На якій відстані літає приціл

        private float yVelocity = 0f;
        private CharacterController controller;
        private bool mobileJumpRequested = false; 

        void Start()
        {
            controller = GetComponent<CharacterController>();
            // Ховаємо маркер прицілу при старті
            if (aimCursor != null) aimCursor.gameObject.SetActive(false); 
        }

        public void MobileJump()
        {
            mobileJumpRequested = true;
        }

        void Update()
        {
            // --- 1. РУХ (Лівий джойстик / WASD) ---
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            if (moveJoystick != null && moveJoystick.inputVector.magnitude > 0f)
            {
                horizontal = moveJoystick.inputVector.x;
                vertical = moveJoystick.inputVector.y;
            }

            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
            Vector3 velocity = direction * moveSpeed;

            // --- 2. ПРИЦІЛ І СТРІЛЬБА (Правий джойстик) ---
            bool isAiming = false;
            // Якщо правий джойстик відхилено
            if (aimJoystick != null && aimJoystick.inputVector.magnitude > 0.1f)
            {
                isAiming = true;
                
                // Рахуємо куди він відхилений
                Vector3 aimDir = new Vector3(aimJoystick.inputVector.x, 0f, aimJoystick.inputVector.y).normalized;
                
                // Переміщаємо курсор туди
                if (aimCursor != null)
                {
                    aimCursor.position = transform.position + aimDir * aimDistance;
                    aimCursor.gameObject.SetActive(true); // Показуємо маркер
                }

                // Повертаємо гравця лицем до прицілу
                Vector3 lookPos = transform.position + aimDir;
                lookPos.y = transform.position.y;
                transform.LookAt(lookPos);

                // Віддаємо наказ загону: ЦІЛИТИСЬ і СТРІЛЯТИ!
                SquadMember[] squad = FindObjectsOfType<SquadMember>();
                foreach (var soldier in squad)
                {
                    if (aimCursor != null)
                        soldier.AimAt(aimCursor.position);
                    else
                        soldier.AimAt(transform.position + aimDir * aimDistance);
                        
                    soldier.TryShoot(); // Авто-вогонь!
                }
            }
            else
            {
                // Якщо правий джойстик відпущено
                if (aimCursor != null) aimCursor.gameObject.SetActive(false); // Ховаємо приціл

                // Якщо ми не цілимося, то повертаємося туди, куди біжимо
                if (direction.magnitude >= 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.deltaTime);
                }

                // Кажемо загону опустити зброю
                SquadMember[] squad = FindObjectsOfType<SquadMember>();
                foreach (var soldier in squad)
                {
                    soldier.StopAiming();
                }
            }

            // --- 3. СТРИБОК ---
            if (controller.isGrounded)
            {
                yVelocity = -0.5f; 
                if (Input.GetKeyDown(KeyCode.Space) || mobileJumpRequested)
                {
                    mobileJumpRequested = false; 
                    yVelocity = jumpForce; 
                    
                    SquadMember[] squad = FindObjectsOfType<SquadMember>();
                    foreach (var soldier in squad)
                    {
                        soldier.Jump();
                    }
                }
            }
            else
            {
                mobileJumpRequested = false; 
                yVelocity += gravity * Time.deltaTime;
            }

            velocity.y = yVelocity;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}
