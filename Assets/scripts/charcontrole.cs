using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Attack Settings")]
    [SerializeField] private GameObject attackHitbox; // ������ ��� �������� ������
    [SerializeField] private float attackDuration = 0.2f;
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private TMP_Text deathMessage; // UI ����� ��� ������� ��� ������

    private int currentHealth;
    private bool isDead;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Camera mainCamera;
    private bool isAttacking;
    [SerializeField] private float invulnerabilityAfterHit = 0.5f; // сек неуязвимости после получения урона

    private float lastHitTime = -999f;
    private void Start()
    {
        currentHealth = maxHealth;
        if (deathMessage != null)
            deathMessage.gameObject.SetActive(false);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        if (Time.time - lastHitTime < invulnerabilityAfterHit) return;

        lastHitTime = Time.time;
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"Player HP: {currentHealth} ( -{damage} )");

        if (currentHealth <= 0)
            Die();
    }



    private void Die()
    {
        isDead = true;
        Debug.Log("Player is dead!");

        if (deathMessage != null)
        {
            deathMessage.text = "YOU DIED";
            deathMessage.gameObject.SetActive(true);
        }

        // ����� ���������� ������
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        GetComponent<PlayerController>().enabled = false;
    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        if (attackHitbox != null)
            attackHitbox.SetActive(false);
    }

    private void Update()
    {
        HandleInput();
        HandleRotation();

        if (Input.GetMouseButtonDown(0) && !isAttacking)
            StartCoroutine(PerformAttack());
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void HandleInput()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;
    }

    private void Move()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void HandleRotation()
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = (mousePos - transform.position);
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        rb.rotation = angle;
    }
    
    private System.Collections.IEnumerator PerformAttack()
    {
        isAttacking = true;
        attackHitbox.SetActive(true);
        yield return new WaitForSeconds(attackDuration);
        attackHitbox.SetActive(false);
        isAttacking = false;
    }
}
