using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class MainBase : MonoBehaviour
{
    [Header("Base Health Settings")]
    [SerializeField] private int maxHealth = 20;
    [SerializeField] private TMP_Text baseHealthText;
    [SerializeField] private TMP_Text destroyedMessage;

    private int currentHealth;
    private bool isDestroyed = false;
    private float lastDamageTime = -999f;
    [SerializeField] private float damageCooldown = 0.5f;

    private enemyspawnpoint waveManager;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();

        waveManager = FindObjectOfType<enemyspawnpoint>();
        if (destroyedMessage != null)
            destroyedMessage.gameObject.SetActive(false);
    }

    private void UpdateUI()
    {
        if (baseHealthText != null)
            baseHealthText.text = $"BASE HP: {currentHealth}";
    }

    public void TakeDamage(int damage)
    {
        if (isDestroyed) return;
        if (Time.time - lastDamageTime < damageCooldown) return;

        lastDamageTime = Time.time;
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();

        Debug.Log($"Base takes {damage} damage → HP: {currentHealth}");

        if (currentHealth <= 0)
            Die();
    }
    private IEnumerator RestartLevel()
    {
        yield return new WaitForSeconds(2f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }
    private void Die()
    {
        isDestroyed = true;
        Debug.Log("Main base destroyed!");

        if (destroyedMessage != null)
        {
            destroyedMessage.text = "BASE DESTROYED!";
            destroyedMessage.gameObject.SetActive(true);
        }
        StartCoroutine(RestartLevel());
        // Тут можешь вызвать GameOver или перезагрузку
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDestroyed) return;

        // 🟦 Если артефакт касается базы → следующая волна
        if (other.CompareTag("Artifact"))
        {
            Debug.Log("Артефакт доставлен на базу — запускаем новую волну");

            if (waveManager != null)
                waveManager.OnRewardDelivered();

            Destroy(other.gameObject);
            return;
        }

        // 🔴 Если враг касается базы → урон
        if (other.CompareTag("Enemyfirst"))
        {
            TakeDamage(1);
        }
    }
}
