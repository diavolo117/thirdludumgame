using UnityEngine;

public class hitbox : MonoBehaviour
{
    [SerializeField] private int damageToPlayer = 1;
    private float lastDamageTime = -999f;
    [SerializeField] private float damageCooldown = 0.5f; // ��� ����� �������/����������
    private void OnTriggerEnter2D(Collider2D other)
    {
        // ��� ��� ������� � ��� ������ ���������
        //Debug.Log($"Enemy '{name}' OnTriggerEnter2D with '{other.gameObject.name}' (tag={other.gameObject.tag})");

        


        // ������� ���� ������ ��� �������� (� ���������)
        if (other.CompareTag("Player") || other.CompareTag("base"))
        {
            

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
}
