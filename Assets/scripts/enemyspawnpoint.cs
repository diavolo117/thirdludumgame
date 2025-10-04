using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private EnemyData[] enemies;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform baseTarget;

    [Header("Wave Settings")]
    [SerializeField] private int wavePoints = 20;
    [SerializeField] private float spawnDelay = 0.5f;
    [SerializeField] private float waveInterval = 5f;

    private bool spawningWave;

    private void Start()
    {
        StartCoroutine(WaveRoutine());
    }

    private IEnumerator WaveRoutine()
    {
        while (true)
        {
            yield return StartCoroutine(SpawnWave());
            yield return new WaitForSeconds(waveInterval);
        }
    }

    private IEnumerator SpawnWave()
    {
        spawningWave = true;
        int pointsLeft = wavePoints;

        Debug.Log("Wave started! Points: " + wavePoints);

        while (pointsLeft > 0)
        {
            // 1. �������� ���������� �����, �������� ����� ���� "���������"
            EnemyData chosen = enemies[Random.Range(0, enemies.Length)];

            if (chosen.cost > pointsLeft)
            {
                // ���� ���� ������� ������� � ������� �������
                bool found = false;
                foreach (var e in enemies)
                {
                    if (e.cost <= pointsLeft)
                    {
                        chosen = e;
                        found = true;
                        break;
                    }
                }

                if (!found)
                    break; // �� ������ �� ����� ����������
            }

            // 2. �������� ��������� ����� ������
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // 3. ������� �����
            GameObject enemy = Instantiate(chosen.prefab, spawnPoint.position, Quaternion.identity);

            // 4. ���� ���� ����� EnemyController � ������ ����
            EnemyController controller = enemy.GetComponent<EnemyController>();
            if (controller != null && baseTarget != null)
                controller.SetDefaultTarget(baseTarget);

            pointsLeft -= chosen.cost;

            yield return new WaitForSeconds(spawnDelay);
        }

        Debug.Log("Wave finished!");
        spawningWave = false;
    }
}
