using UnityEngine;

public class RewardItem : MonoBehaviour
{
    private bool pickedUp = false;
    private Transform followTarget;
    private float followSpeed = 5f;

    void Update()
    {
        if (pickedUp && followTarget != null)
        {
            transform.position = Vector3.Lerp(transform.position, followTarget.position + Vector3.up * 1f, Time.deltaTime * followSpeed);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!pickedUp && other.CompareTag("Player"))
        {
            followTarget = other.transform;
            pickedUp = true;
        }
    }

    public bool IsPickedUp() => pickedUp;
}
