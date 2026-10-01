using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Настройки движения")]
    public float speed = 6f;
    public float jumpForce = 12f;
    public float climbSpeed = 4f;

    [Header("Проверка земли")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float checkRadius = 0.2f;

    [Header("Настройки стены (Wall Jump)")]
    public float wallSlideSpeed = 2f;
    public float wallStickTime = 1f;
    public Vector2 wallJumpForce = new Vector2(8f, 12f);
    public float wallCheckDistance = 0.4f;

    [Header("Здоровье и урон")]
    public int maxHealth = 3;
    public float invincibilityDuration = 1.5f; // Время неуязвимости после удара
    public float knockbackForceX = 8f;        // Сила отскока по горизонтали
    public float knockbackForceY = 6f;        // Сила отскока по вертикали

    [Header("Звуки игрока")]
    public AudioClip jumpSound;       // Звук прыжка
    public AudioClip hurtSound;       // Звук получения урона
    private AudioSource audioSource;

    private int currentHealth;
    private float invincibilityTimer;
    private float knockbackTimer; // Блокировка ввода при получении урона


    public int CurrentHealth => currentHealth;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private float horizontalInput;
    private float verticalInput;
    private bool isGrounded;

    private bool isTouchingLadder;
    private bool isClimbing;
    private float originalGravity;

    private bool isTouchingWall;
    private bool isWallSliding;
    private float wallStickTimer;
    private float wallJumpTimer;
    private float lastWallDir;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        originalGravity = rb.gravityScale;
        wallStickTimer = wallStickTime;
        currentHealth = maxHealth;
    }

    void Update()
    {
        // Таймеры блокировок
        if (wallJumpTimer > 0) wallJumpTimer -= Time.deltaTime;
        if (knockbackTimer > 0) knockbackTimer -= Time.deltaTime;

        // Обработка неуязвимости
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;

            // Мерцание
            float alpha = Mathf.PingPong(Time.time * 15f, 1f) > 0.5f ? 1f : 0.2f;
            spriteRenderer.color = new Color(1f, 1f, 1f, alpha);
        }
        else
        {
            spriteRenderer.color = Color.white;
        }

        // Считывание ввода (блокируется при отскоке от стены или получении урона)
        if (Keyboard.current != null)
        {
            if (wallJumpTimer <= 0 && knockbackTimer <= 0)
            {
                horizontalInput = 0f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                    horizontalInput = 1f;
                else if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                    horizontalInput = -1f;
            }

            verticalInput = 0f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                verticalInput = 1f;
            else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                verticalInput = -1f;
        }

        if (wallJumpTimer <= 0 && knockbackTimer <= 0)
        {
            // Если пальцем двигают джойстик по горизонтали — используем его значение
            if (Mathf.Abs(VirtualJoystick.HorizontalInput) > 0.1f)
            {
                horizontalInput = VirtualJoystick.HorizontalInput;
            }
        }

        // Если пальцем двигают джойстик по вертикали — переводим в W/S для лестниц
        if (Mathf.Abs(VirtualJoystick.VerticalInput) > 0.2f)
        {
            verticalInput = VirtualJoystick.VerticalInput > 0 ? 1f : -1f;
        }

        // Проверка земли
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        if (isGrounded)
        {
            lastWallDir = 0f;
        }

        // Проверка стены
        float direction = transform.localScale.x;
        Vector2 rayDirection = new Vector2(direction, 0);
        isTouchingWall = Physics2D.Raycast(transform.position, rayDirection, wallCheckDistance, groundLayer);

        // ПРЫЖОК (Пробел на клавиатуре ИЛИ мобильная кнопка на экране)
        bool jumpPressed = (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) || MobileJumpButton.WasPressed;

        if (jumpPressed && isGrounded && !isClimbing && !isWallSliding && knockbackTimer <= 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (jumpSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(jumpSound);
            }
        }

        // Прыжок от стены (Wall Jump)
        if (jumpPressed && isWallSliding && knockbackTimer <= 0)
        {
            lastWallDir = direction;
            rb.linearVelocity = new Vector2(-direction * wallJumpForce.x, wallJumpForce.y);
            transform.localScale = new Vector3(-direction, 1, 1);
            wallJumpTimer = 0.15f;
            isWallSliding = false;
        }

        // Логика лестницы
        if (isTouchingLadder && Mathf.Abs(verticalInput) > 0f && knockbackTimer <= 0)
        {
            isClimbing = true;
        }
        if (!isTouchingLadder)
        {
            isClimbing = false;
        }

        // Логика скольжения на стене
        bool isSameWall = (direction == lastWallDir);

        if (isTouchingWall && !isGrounded && rb.linearVelocity.y <= 0.1f && !isClimbing && !isSameWall && knockbackTimer <= 0)
        {
            isWallSliding = true;
            lastWallDir = 0f;
        }
        else
        {
            isWallSliding = false;
            if (!isTouchingWall || isGrounded)
            {
                wallStickTimer = wallStickTime;
            }
        }

        // Поворот персонажа
        if (!isClimbing && !isWallSliding && wallJumpTimer <= 0 && knockbackTimer <= 0)
        {
            if (horizontalInput > 0)
                transform.localScale = new Vector3(1, 1, 1);
            else if (horizontalInput < 0)
                transform.localScale = new Vector3(-1, 1, 1);
        }

        // Аниматор
        if (anim != null)
        {
            anim.SetBool("isRunning", horizontalInput != 0 && isGrounded);
            anim.SetBool("isGrounded", isGrounded);
            anim.SetBool("isClimbing", isClimbing);
            anim.SetFloat("yVelocity", rb.linearVelocity.y);
            anim.SetBool("isHurt", knockbackTimer > 0);

            if (isClimbing)
            {
                anim.SetFloat("climbSpeed", Mathf.Abs(verticalInput));
            }
        }
    }

    void FixedUpdate()
    {
        if (isClimbing)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(horizontalInput * speed * 0.5f, verticalInput * climbSpeed);
        }
        else if (isWallSliding)
        {
            rb.gravityScale = 0f;

            if (wallStickTimer > 0)
            {
                rb.linearVelocity = new Vector2(0f, 0f);
                wallStickTimer -= Time.fixedDeltaTime;
            }
            else
            {
                rb.linearVelocity = new Vector2(0f, -wallSlideSpeed);
            }
        }
        else
        {
            rb.gravityScale = originalGravity;

            // Физика обычного бега (блокируется при получении урона)
            if (wallJumpTimer <= 0 && knockbackTimer <= 0)
            {
                rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);
            }
        }
    }

    // ПОЛУЧЕНИЕ УРОНА И ОТБРАСЫВАНИЕ
    public void TakeDamage(int damage, float damageSourceX)
    {
        if (invincibilityTimer > 0) return; // Пропускаем урон, если мы неуязвимы

        currentHealth -= damage;

        // Включаем анимацию урона
        if (anim != null)
        {
            anim.SetBool("isHurt", true);
        }

        // Если здоровье кончилось — смерть (перезапуск)
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // Рассчитываем направление отскока (в противоположную сторону от источника урона)
        float bounceDir = Mathf.Sign(transform.position.x - damageSourceX);

        // Сбрасываем лестницу и стену
        isClimbing = false;
        isWallSliding = false;

        // Применяем силу отскока
        rb.linearVelocity = new Vector2(bounceDir * knockbackForceX, knockbackForceY);

        if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }
        // Включаем таймеры блокировки и неуязвимости
        knockbackTimer = 0.3f;
        invincibilityTimer = invincibilityDuration;
    }

    private void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Столкновение с ШИПАМИ
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //  Смертельная яма с шипами (мгновенная смерть)
        if (collision.gameObject.CompareTag("DeadlySpikes"))
        {
            Die(); // Вызываем метод смерти (перезапуск уровня)
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            isTouchingLadder = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            isTouchingLadder = false;
            isClimbing = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }

        Gizmos.color = Color.blue;
        float direction = transform.localScale.x;
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(direction * wallCheckDistance, 0, 0));
    }
}