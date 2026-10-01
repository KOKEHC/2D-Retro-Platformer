using UnityEngine;
using UnityEngine.SceneManagement;

public enum EnemyType { GroundPatrol, FlyingWaypoints }

public class EnemyController : MonoBehaviour
{
    [Header("Основные настройки типа врага")]
    public EnemyType enemyType; 
    public bool spriteFacesLeftByDefault = true;
    public float patrolSpeed = 2f;
    public float bounceForce = 10f;

    [Header("Настройки патруля ЗЕМЛИ (только для GroundPatrol)")]
    public float checkDistance = 0.7f;
    public LayerMask groundLayer;

    [Header("Настройки ПОГОНИ (только для GroundPatrol)")]
    public float chaseSpeed = 4.5f;
    public float detectionRange = 6f;
    public float loseRange = 9f;
    public LayerMask playerLayer;
    public Transform player;

    [Header("Настройки ПОЛЕТА (только для FlyingWaypoints)")]
    public Transform[] waypoints; // Точки полета в воздухе

    [Header("Звуки врага")]
    public AudioClip stompSound; 

    private Rigidbody2D rb;
    private Collider2D col;
    private int movingDirection = 1;
    private bool isChasing;
    private float scaleXMultiplier = 1f;

    private float chaseCooldownTimer;
    private float turnCooldownTimer;
    private int targetIndex = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

            // Если это летящий враг — отключаем для него гравитацию
            if (enemyType == EnemyType.FlyingWaypoints)
            {
                rb.gravityScale = 0f;
            }
        }

        scaleXMultiplier = spriteFacesLeftByDefault ? -1f : 1f;

        // Настройка путей для летающего врага при старте
        if (enemyType == EnemyType.FlyingWaypoints)
        {
            foreach (Transform wp in waypoints)
            {
                if (wp != null) wp.SetParent(null);
            }
            if (waypoints.Length > 0 && waypoints[0] != null)
            {
                transform.position = waypoints[0].position;
            }
        }
    }

    void Update()
    {
        if (enemyType == EnemyType.GroundPatrol)
        {
            UpdateGroundEnemy();
        }
        else if (enemyType == EnemyType.FlyingWaypoints)
        {
            UpdateFlyingEnemy();
        }
    }

    void FixedUpdate()
    {
        if (enemyType == EnemyType.GroundPatrol)
        {
            FixedUpdateGroundEnemy();
        }
    }

    // ЛОГИКА НАЗЕМНОГО ВРАГА (УЛИТКА) 
    void UpdateGroundEnemy()
    {
        if (player == null) return;

        if (chaseCooldownTimer > 0)
        {
            chaseCooldownTimer -= Time.deltaTime;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        bool lineOfSightBlocked = false;
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        RaycastHit2D sightHit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, groundLayer);

        if (sightHit.collider != null)
        {
            lineOfSightBlocked = true;
        }

        if (!isChasing)
        {
            if (chaseCooldownTimer <= 0)
            {
                Vector2 rayDir = new Vector2(movingDirection, 0);
                RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDir, detectionRange, playerLayer);

                if (hit.collider != null && hit.collider.CompareTag("Player") && !lineOfSightBlocked)
                {
                    isChasing = true;
                }
            }
        }
        else
        {
            bool playerTooFar = distanceToPlayer > loseRange;
            bool playerTooHigh = Mathf.Abs(player.position.y - transform.position.y) > 2.5f;

            float chaseDir = Mathf.Sign(player.position.x - transform.position.x);
            bool isWallBlockingChase = Physics2D.Raycast(transform.position, new Vector2(chaseDir, 0), checkDistance, groundLayer);

            if (playerTooFar || playerTooHigh || lineOfSightBlocked || isWallBlockingChase)
            {
                isChasing = false;
                chaseCooldownTimer = 1.5f;

                float lastKnownDir = Mathf.Sign(player.position.x - transform.position.x);
                movingDirection = (int)lastKnownDir;

                transform.localScale = new Vector3(movingDirection * scaleXMultiplier, 1, 1);
            }
        }
    }

    void FixedUpdateGroundEnemy()
    {
        if (turnCooldownTimer > 0)
        {
            turnCooldownTimer -= Time.fixedDeltaTime;
        }

        if (!isChasing)
        {
            rb.linearVelocity = new Vector2(movingDirection * patrolSpeed, rb.linearVelocity.y);

            float bottomY = (col != null) ? col.bounds.min.y : transform.position.y;
            Vector2 checkPos = new Vector2(transform.position.x + movingDirection * checkDistance, bottomY + 0.1f);

            bool isGroundAhead = Physics2D.Raycast(checkPos, Vector2.down, 1.2f, groundLayer);
            bool isWallAhead = Physics2D.Raycast(transform.position, new Vector2(movingDirection, 0), checkDistance, groundLayer);

            if ((!isGroundAhead || isWallAhead) && turnCooldownTimer <= 0)
            {
                movingDirection *= -1;
                transform.localScale = new Vector3(movingDirection * scaleXMultiplier, 1, 1);
                turnCooldownTimer = 0.5f;
            }
        }
        else
        {
            if (player != null)
            {
                float xDiff = player.position.x - transform.position.x;
                float chaseDirection = movingDirection;

                if (Mathf.Abs(xDiff) > 1.2f)
                {
                    chaseDirection = Mathf.Sign(xDiff);
                    movingDirection = (int)chaseDirection;
                }

                rb.linearVelocity = new Vector2(chaseDirection * chaseSpeed, rb.linearVelocity.y);
                transform.localScale = new Vector3(chaseDirection * scaleXMultiplier, 1, 1);
            }
        }
    }

    //  ЛОГИКА ЛЕТАЮЩЕГО ВРАГА (ПЧЕЛА) 
    void UpdateFlyingEnemy()
    {
        if (waypoints.Length == 0) return;

        // Движение к текущей точке пути
        transform.position = Vector3.MoveTowards(transform.position, waypoints[targetIndex].position, patrolSpeed * Time.deltaTime);

        // Разворот спрайта в воздухе
        float direction = Mathf.Sign(waypoints[targetIndex].position.x - transform.position.x);
        transform.localScale = new Vector3(direction * scaleXMultiplier, 1, 1);

        if (Vector3.Distance(transform.position, waypoints[targetIndex].position) < 0.05f)
        {
            targetIndex = (targetIndex + 1) % waypoints.Length;
        }
    }

    // ОБЩЕЕ СТОЛКНОВЕНИЕ 
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Прыгнули сверху на голову
            if (collision.contacts[0].normal.y < -0.5f)
            {
                Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, bounceForce);
                }
                if (stompSound != null)
                {
                    // Воспроизводим звук в координатах камеры, чтобы он звучал на 100% громкости
                    AudioSource.PlayClipAtPoint(stompSound, Camera.main.transform.position, 1f);
                }
                Destroy(gameObject);
            }
            else
            {
                // Нанесение урона игроку
                PlayerController playerScript = collision.gameObject.GetComponent<PlayerController>();
                if (playerScript != null)
                {
                    playerScript.TakeDamage(1, transform.position.x);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (enemyType == EnemyType.GroundPatrol)
        {
            float dir = isChasing ? Mathf.Sign(transform.localScale.x * scaleXMultiplier) : movingDirection;

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(dir * detectionRange, 0, 0));

            Gizmos.color = Color.red;
            float bottomY = (col != null) ? col.bounds.min.y : transform.position.y;
            Vector2 checkPos = new Vector2(transform.position.x + dir * checkDistance, bottomY + 0.1f);
            Gizmos.DrawLine(checkPos, checkPos + Vector2.down * 1.2f);
        }
    }
}