using UnityEngine;

public class RewardItem : MonoBehaviour
{
    private bool pickedUp = false;
    private Transform followTarget;
    private float followSpeed = 5f;
    public int upgradeStage = 1; // от 1 до 6

    void Update()
    {
        if (pickedUp && followTarget != null)
        {
            transform.position = Vector3.Lerp(transform.position, followTarget.position + Vector3.up * 1f, Time.deltaTime * followSpeed);
        }
    }

    private void Start()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (pickedUp) return;

        if (other.CompareTag("Player"))
        {
            followTarget = other.transform;
            pickedUp = true;

            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.ApplyUpgrade(upgradeStage);
            }

            // Можно оставить коллайдер включенным — не мешает доставке
            Debug.Log($"Reward {upgradeStage} picked up and applied upgrade.");
        }
    }



    public bool IsPickedUp() => pickedUp;
}
