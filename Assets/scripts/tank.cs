using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyControllertank : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float separationRadius = 1f;
    [SerializeField] private Transform defaultTarget;
    private Vector2 currentDirection;
    private Transform currentTarget;
    private bool isKnockedBack = false;
    private float knockbackTimer = 0f;
    [SerializeField] private float knockbackDuration = 0.3f;


    [Header("Combat")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int damageToPlayer = 1;
    [SerializeField] private float damageCooldown = 0.5f; // сек между укусами/контактами
    [SerializeField] private float knockstr = 3;
    private Rigidbody2D rb;
    private Transform player;
    private int currentHealth;
    private float lastDamageTime = -999f;
    [Header("Tank Settings")]
    [SerializeField] private float aoeRadius = 2.5f;
    [SerializeField] private int aoeDamage = 1;
    [SerializeField] private float aoeInterval = 4f;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private GameObject aoeEffect; // визуальный круг, если хочешь
    private bool recentlyHit = false;
    [SerializeField] private float hitCooldown = 0.2f; // задержка между получениями урона

    private bool canAoe = true;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        currentTarget = defaultTarget;

    }
    private IEnumerator AoeAttack()
    {
        canAoe = false;

        if (aoeEffect != null)
        {
            aoeEffect.SetActive(true);
            yield return new WaitForSeconds(0.3f);
            aoeEffect.SetActive(false);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aoeRadius, playerMask);
        foreach (var h in hits)
        {
            var ph = h.GetComponent<PlayerController>();
            if (ph != null)
                ph.TakeDamage(aoeDamage);
        }

        yield return new WaitForSeconds(aoeInterval);
        canAoe = true;
    }

    private IEnumerator HitCooldown()
    {
        yield return new WaitForSeconds(hitCooldown);
        recentlyHit = false;
    }

    public void SetDefaultTarget(Transform target)
    {
        defaultTarget = target;
    }
    private void Update()
    {
        // 🔵 Проверяем возможность АОЕ
        if (canAoe)
            StartCoroutine(AoeAttack());

        // 🔴 Обновляем агро-цель
        if (player == null) return;

        var playerCtrl = player.GetComponent<PlayerController>();
        if (playerCtrl != null && !playerCtrl.IsAstralActive())
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist <= detectionRadius)
            {
                // Агримся на игрока
                currentTarget = player;
            }
            else
            {
                // Возвращаемся к базе
                currentTarget = defaultTarget;
            }
        }
        else
        {
            // Игрок в астрале → агро сбрасывается
            currentTarget = defaultTarget;
        }
    }

    private void FixedUpdate()
    {
        if (isKnockedBack)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            if (knockbackTimer <= 0f)
            {
                isKnockedBack = false;
                rb.linearVelocity = Vector2.zero;
            }
            return;
        }

        if (currentTarget == null) return;

        // направление к цели
        Vector2 dir = ((Vector2)currentTarget.position - (Vector2)transform.position).normalized;
        dir += CalculateSeparation();
        dir = dir.normalized;

        // движение
        rb.linearVelocity = dir * moveSpeed;

        // поворот
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            rb.rotation = angle;
        }
    }


    private Vector2 CalculateSeparation()
    {
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, separationRadius);
        Vector2 separation = Vector2.zero;

        foreach (var col in nearby)
        {
            if (col == null) continue;
            // Сравнивай тег врагов — убедись, что у врагов стоит тот же тег
            if (col.gameObject == gameObject) continue;
            if (!col.CompareTag("Enemyfirst")) continue;

            Vector2 away = (Vector2)(transform.position - col.transform.position);
            float distance = Mathf.Max(away.magnitude, 0.01f);
            separation += away.normalized / distance;
        }

        return separation * 0.5f;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
            Die();

    }

    private void Die()
    {
        FindObjectOfType<enemyspawnpoint>().UnregisterEnemy(this.gameObject);
        Destroy(gameObject);
    }
    private IEnumerator slowburn()
    {
        moveSpeed *= 0.1f;
        yield return new WaitForSeconds(0.8f);
        moveSpeed /= 0.1f;
    }
    private void Knockback(Vector2 sourcePosition, float strength)
    {
        Vector2 dir = (transform.position - (Vector3)sourcePosition).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dir * strength, ForceMode2D.Impulse);
        isKnockedBack = true;
        knockbackTimer = knockbackDuration;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Лог для отладки — что именно коснулось
        //Debug.Log($"Enemy '{name}' OnTriggerEnter2D with '{other.gameObject.name}' (tag={other.gameObject.tag})");

        // Если попались под удар игрока
        if (other.CompareTag("PlayerAttack") && !recentlyHit)
        {
            recentlyHit = true;

            PlayerController player = FindObjectOfType<PlayerController>();
            int attackdamag = player.damagedeal;

            if (player != null)
            {
                TakeDamage(attackdamag);
                player.OnEnemyHit();
            }

            StartCoroutine(HitCooldown());
            return;
        }



        // Наносим урон игроку при контакте (с кулдауном)
        if (other.CompareTag("Player") || other.CompareTag("base"))
        {
            Knockback(other.transform.position, knockstr);
            StartCoroutine(slowburn());

            PlayerController ph = other.GetComponent<PlayerController>();
            if (ph == null && other.attachedRigidbody != null)
                ph = other.attachedRigidbody.GetComponent<PlayerController>();

            if (ph != null)
            {
                if (Time.time - lastDamageTime >= damageCooldown)
                {
                    Debug.Log("Enemy deals damage to player: " + damageToPlayer);
                    ph.TakeDamage(damageToPlayer);
                    lastDamageTime = Time.time;
                }
                else
                {
                    Debug.Log("Damage on cooldown");
                }
            }
            else
            {
                Debug.LogWarning("PlayerHealth not found on Player object. Ensure Player has PlayerHealth component.");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}
