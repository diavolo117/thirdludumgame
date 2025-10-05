using TMPro;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [Header("Astral Visual")]
    [SerializeField] private GameObject astralOverlay;
    private Collider2D playerCollider;

    [Header("Attack Settings")]
    [SerializeField] private GameObject[] attackHitboxes; // 3 вида атак
    [SerializeField] private float attackDuration = 0.2f;
    private int attackType = 0; // 0,1,2 — какой хитбокс используется
    [SerializeField] public int damagedeal = 1;
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private TMP_Text deathMessage;
    public int upgradeStage = 1; // от 1 до 6

    private int currentHealth;
    private bool isDead;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Camera mainCamera;
    private bool isAttacking;
    [SerializeField] private float invulnerabilityAfterHit = 0.5f;
    private float lastHitTime = -999f;

    // ====== ПРОГРЕССИЯ / СПОСОБНОСТИ ======
    [Header("Abilities")]
    public bool dashUnlocked = false;
    public bool astralUnlocked = false;
    public bool regenUnlocked = false;
    public bool vampirismUnlocked = false;

    [Header("Dash Settings")]
    [SerializeField] private float dashDistance = 3f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float dashDuration = 0.15f;
    private float lastDashTime = -999f;
    private bool isDashing = false;

    [Header("Astral Settings")]
    [SerializeField] private KeyCode astralKey = KeyCode.Space;
    [SerializeField] private float astralDuration = 2f;
    private bool isAstral = false;
    [SerializeField] private float astralCooldown = 6f; // ⏳ время перезарядки астрала
    private float lastAstralTime = -999f;



    [Header("Regen Settings")]
    [SerializeField] private float regenPerSecond = 1f;
    private Coroutine regenRoutine;

    [Header("Vampirism Settings")]
    [SerializeField] private int lifestealAmount = 1;
    [Header("UI References")]
    [SerializeField] private UnityEngine.UI.Slider healthSlider;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private UnityEngine.UI.Slider dashCooldownSlider;
    [SerializeField] private UnityEngine.UI.Slider astralCooldownSlider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        foreach (var hb in attackHitboxes)
            if (hb != null) hb.SetActive(false);
        playerCollider = GetComponent<Collider2D>();

    }

    private void Start()
    {
        currentHealth = maxHealth;
        if (deathMessage != null)
            deathMessage.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isDead) return;

        HandleInput();
        HandleRotation();

        // --- атака ---
        if (Input.GetMouseButtonDown(0) && !isAttacking && !isAstral && !isDashing)
            StartCoroutine(PerformAttack());

        // --- дэш ---
        if (dashUnlocked && Input.GetMouseButtonDown(1) && !isAstral && Time.time - lastDashTime >= dashCooldown)
            StartCoroutine(PerformDash());

        // --- астрал ---
        if (astralUnlocked && Input.GetKeyDown(astralKey) && !isAstral && !isDashing && !isAttacking)
        {
            if (Time.time - lastAstralTime >= astralCooldown)
                StartCoroutine(AstralForm());
        }

        UpdateUI();
    }

    private void FixedUpdate()
    {
        if (isDead || isAstral || isDashing) return;
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

    // ======================= АТАКА =======================
    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        if (attackHitboxes.Length > 0 && attackHitboxes[attackType] != null)
            attackHitboxes[attackType].SetActive(true);

        yield return new WaitForSeconds(attackDuration);

        if (attackHitboxes.Length > 0 && attackHitboxes[attackType] != null)
            attackHitboxes[attackType].SetActive(false);
        isAttacking = false;
    }

    // ======================= ДЭШ =======================
    private IEnumerator PerformDash()
    {
        isDashing = true;
        lastDashTime = Time.time;

        if (playerCollider != null)
            playerCollider.enabled = false; // 🚫 выключаем коллайдер


        // направление по курсору
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dashDir = ((Vector2)mousePos - (Vector2)transform.position).normalized;

        // временно делаем неуязвимым
        float originalInvuln = invulnerabilityAfterHit;
        invulnerabilityAfterHit = dashDuration + 0.1f;

        float elapsed = 0f;
        Vector2 startPos = transform.position;
        Vector2 targetPos = startPos + dashDir * dashDistance;

        while (elapsed < dashDuration)
        {
            rb.MovePosition(Vector2.Lerp(startPos, targetPos, elapsed / dashDuration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        rb.MovePosition(targetPos);
        if (playerCollider != null)
            playerCollider.enabled = true; // ✅ возвращаем коллайдер

        invulnerabilityAfterHit = originalInvuln;
        isDashing = false;
    }

    // ======================= АСТРАЛ =======================
    private IEnumerator AstralForm()
    {
        isAstral = true;
        lastAstralTime = Time.time; // 🕒 запускаем кулдаун

        float originalSpeed = moveSpeed;
        rb.linearVelocity = Vector2.zero;
        moveSpeed = 0;

        if (astralOverlay != null)
            astralOverlay.SetActive(true); // 🔵 включаем PNG

        Debug.Log("Astral form activated");

        yield return new WaitForSeconds(astralDuration);

        moveSpeed = originalSpeed;
        isAstral = false;

        if (astralOverlay != null)
            astralOverlay.SetActive(false); // 🔵 выключаем PNG

        Debug.Log("Astral form ended");
    }


    // ======================= РЕГЕН =======================
    public void EnableRegen(bool state)
    {
        regenUnlocked = state;

        if (regenRoutine != null)
        {
            StopCoroutine(regenRoutine);
            regenRoutine = null;
        }

        if (regenUnlocked)
            regenRoutine = StartCoroutine(RegenLoop());
    }


    private IEnumerator RegenLoop()
    {
        while (regenUnlocked && !isDead)
        {
            yield return new WaitForSeconds(1f);
            Heal((int)regenPerSecond);
        }
    }


    // ======================= ВАМПИРИЗМ =======================
    public void OnEnemyHit()
    {
        if (vampirismUnlocked)
            Heal(lifestealAmount);
    }

    // ======================= ХП =======================
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        if (isAstral || isDashing) return; // неуязвим в этих состояниях
        if (Time.time - lastHitTime < invulnerabilityAfterHit) return;

        lastHitTime = Time.time;
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"Player HP: {currentHealth} ( -{damage} )");
        if (currentHealth <= 0) Die();
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        Debug.Log($"+{amount} HP → {currentHealth}/{maxHealth}");
    }

    private void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        if (deathMessage != null)
        {
            deathMessage.text = "YOU DIED";
            deathMessage.gameObject.SetActive(true);
        }
        Debug.Log("Player is dead!");
        enabled = false;
    }

    // ======================= ПРОГРЕССИЯ =======================
    // вызов из наград / базы
    public void ApplyUpgrade(int stage)
    {
        switch (stage)
        {
            case 1: Debug.Log("firstuply"); break;
            case 2: Debug.Log("secstuply"); break;
            case 3: Debug.Log("thirdstuply"); break;
            case 4: vampirismUnlocked = true; break;
            case 5: attackType = 1; break;
            case 6: attackType = 2; break;
        }
        Debug.Log($"Upgrade {stage} applied!");
    }
    // ======================= АГР ДЛЯ ВРАГОВ =======================
    public bool IsAstralActive()
    {
        return isAstral;
    }
    // ======================= UI ОБНОВЛЕНИЕ =======================
    private void UpdateUI()
    {
        // Astral Cooldown
        if (astralUnlocked && astralCooldownSlider != null)
        {
            float t = Mathf.Clamp01((Time.time - lastAstralTime) / astralCooldown);
            astralCooldownSlider.value = t;
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthText != null)
            healthText.text = $"{currentHealth} / {maxHealth}";

        // Dash Cooldown
        if (dashUnlocked && dashCooldownSlider != null)
        {
            float t = Mathf.Clamp01((Time.time - lastDashTime) / dashCooldown);
            dashCooldownSlider.value = t;
        }
    }

}
