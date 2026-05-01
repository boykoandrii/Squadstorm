using UnityEngine;

namespace Squadstorm.Core
{
    public class PlayerMovement : MonoBehaviour
    {
        public float moveSpeed = 5f;
        public float jumpForce = 5f;
        public float gravity = -15f;
        
        [Header("Керування (Twin-Stick)")]
        public SimpleJoystick moveJoystick; 
        public SimpleJoystick aimJoystick;  
        
        [Header("Приціл")]
        public Transform aimCursor; 
        public float aimDistance = 8f; 
        public float aimSmoothSpeed = 15f; // Наскільки швидко приціл літає

        [Header("Стрільба")]
        public GameObject bulletPrefab; // Префаб кулі для командира
        public float fireRate = 0.3f; // Швидкість стрільби
        private float nextFireTime = 0f;

        private float yVelocity = 0f;
        private CharacterController controller;
        private bool mobileJumpRequested = false; 
        private LineRenderer laserLine; // Наш лазерний приціл

        void Start()
        {
            controller = GetComponent<CharacterController>();
            if (aimCursor != null) aimCursor.gameObject.SetActive(false); 
            
            // Створюємо лазер прямо з коду, щоб вам не довелось нічого налаштовувати в Unity!
            laserLine = GetComponent<LineRenderer>();
            if (laserLine == null)
            {
                laserLine = gameObject.AddComponent<LineRenderer>();
                laserLine.startWidth = 0.15f;
                laserLine.endWidth = 0.15f;
                
                // Даємо йому стандартний яскравий матеріал
                Material laserMat = new Material(Shader.Find("Sprites/Default"));
                laserLine.material = laserMat;
                
                // Робимо його червоним і трохи напівпрозорим
                laserLine.startColor = new Color(1f, 0f, 0f, 0.6f);
                laserLine.endColor = new Color(1f, 0f, 0f, 0.6f);
                
                laserLine.positionCount = 2;
                laserLine.enabled = false;
            }
        }

        public void MobileJump()
        {
            mobileJumpRequested = true;
        }

        void Update()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            if (moveJoystick != null && moveJoystick.inputVector.magnitude > 0f)
            {
                horizontal = moveJoystick.inputVector.x;
                vertical = moveJoystick.inputVector.y;
            }

            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
            Vector3 velocity = direction * moveSpeed;

            // --- ПРИЦІЛ І СТРІЛЬБА ---
            bool isAiming = false;
            if (aimJoystick != null && aimJoystick.inputVector.magnitude > 0.1f)
            {
                isAiming = true;
                
                // Отримуємо "сире" відхилення джойстика (чим сильніше тягнете, тим довший вектор)
                Vector3 aimInput = new Vector3(aimJoystick.inputVector.x, 0f, aimJoystick.inputVector.y);
                Vector3 aimDir = aimInput.normalized; // Напрямок (тільки для того, щоб гравець туди повертався)
                
                if (aimCursor != null)
                {
                    // Точка, куди вказує ваш палець
                    Vector3 rawTargetPos = transform.position + aimInput * aimDistance;
                    rawTargetPos.y = 0.1f; 

                    // --- АВТОПРИЦІЛ (МАГНІТ) ---
                    Vector3 targetCursorPos = rawTargetPos;
                    float magnetismRadius = 4f; // Наскільки сильно приціл "липне" до ворогів
                    Enemy bestTarget = null;
                    float minDistance = magnetismRadius;

                    // Шукаємо ворогів біля того місця, куди ви цілитесь
                    foreach (Enemy e in FindObjectsOfType<Enemy>())
                    {
                        float distToCursor = Vector3.Distance(rawTargetPos, e.transform.position);
                        if (distToCursor < minDistance)
                        {
                            minDistance = distToCursor;
                            bestTarget = e;
                        }
                    }

                    // Якщо ворог близько до курсора — магнітимо приціл прямо на нього!
                    if (bestTarget != null)
                    {
                        targetCursorPos = bestTarget.transform.position;
                        targetCursorPos.y = 0.1f; // тримаємо на землі
                        
                        // Коригуємо напрямок, щоб гравець і солдати дивилися ТОЧНО на цього ворога
                        Vector3 dirToEnemy = (bestTarget.transform.position - transform.position);
                        dirToEnemy.y = 0;
                        aimDir = dirToEnemy.normalized;
                    }

                    if (!aimCursor.gameObject.activeSelf)
                    {
                        // Тільки-но почали цілитись — з'являємося біля гравця
                        Vector3 startPos = transform.position;
                        startPos.y = 0.1f;
                        aimCursor.position = startPos;
                        aimCursor.gameObject.SetActive(true); 
                    }
                    
                    // Плавно підтягуємо курсор до потрібної точки
                    aimCursor.position = Vector3.Lerp(aimCursor.position, targetCursorPos, aimSmoothSpeed * Time.deltaTime);
                    
                    // Гарантуємо, що висота завжди залишається 0.1
                    Vector3 finalPos = aimCursor.position;
                    finalPos.y = 0.1f;
                    aimCursor.position = finalPos;
                }

                // --- МАЛЮЄМО ЛАЗЕР ---
                if (laserLine != null && aimCursor != null)
                {
                    laserLine.enabled = true;
                    // Початок лазера (від грудей гравця)
                    laserLine.SetPosition(0, transform.position + Vector3.up * 1f); 
                    // Кінець лазера (там, де курсор на землі)
                    laserLine.SetPosition(1, aimCursor.position);
                }

                Vector3 lookPos = transform.position + aimDir;
                lookPos.y = transform.position.y;
                transform.LookAt(lookPos);

                // Командир стріляє сам!
                if (Time.time >= nextFireTime && bulletPrefab != null)
                {
                    nextFireTime = Time.time + fireRate;
                    Vector3 spawnPos = transform.position + Vector3.up * 0.5f + transform.forward * 1f;
                    Instantiate(bulletPrefab, spawnPos, transform.rotation);
                }

                // Наказуємо загону теж стріляти
                SquadMember[] squad = FindObjectsOfType<SquadMember>();
                foreach (var soldier in squad)
                {
                    if (aimCursor != null)
                        soldier.AimAt(aimCursor.position);
                    else
                        soldier.AimAt(transform.position + aimDir * aimDistance);
                        
                    soldier.TryShoot(); 
                }
            }
            else
            {
                if (aimCursor != null) aimCursor.gameObject.SetActive(false); 
                if (laserLine != null) laserLine.enabled = false; // Вимикаємо лазер

                if (direction.magnitude >= 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.deltaTime);
                }

                SquadMember[] squad = FindObjectsOfType<SquadMember>();
                foreach (var soldier in squad)
                {
                    soldier.StopAiming();
                }
            }

            // --- СТРИБОК ---
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
