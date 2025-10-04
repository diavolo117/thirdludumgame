using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float separationRadius = 1f;
    [SerializeField] private Transform defaultTarget;

    [Header("Combat")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int damageToPlayer = 1;
    [SerializeField] private float damageCooldown = 0.5f; // сек между укусами/контактами

    private Rigidbody2D rb;
    private Transform player;
    private int currentHealth;
    private float lastDamageTime = -999f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    public void SetDefaultTarget(Transform target)
    {
        defaultTarget = target;
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        Vector2 targetPos = defaultTarget != null ? (Vector2)defaultTarget.position : (Vector2)transform.position;
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer <= detectionRadius)
            targetPos = player.position;

        Vector2 moveDir = (targetPos - (Vector2)transform.position).normalized;
        moveDir += CalculateSeparation();
        moveDir = moveDir.normalized;

        rb.linearVelocity = moveDir * moveSpeed; // <- исправлено: velocity, не linearVelocity

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
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Лог для отладки — что именно коснулось
        Debug.Log($"Enemy '{name}' OnTriggerEnter2D with '{other.gameObject.name}' (tag={other.gameObject.tag})");

        // Если попались под удар игрока
        if (other.CompareTag("PlayerAttack"))
        {
            Debug.Log("Enemy hit by PlayerAttack");
            TakeDamage(1);
            return;
        }

        // Наносим урон игроку при контакте (с кулдауном)
        if (other.CompareTag("Player"))
        {
            // пытаемся достать компонент здоровья — сначала с самого collider'а, затем с attachedRigidbody (root)
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
    }
}
