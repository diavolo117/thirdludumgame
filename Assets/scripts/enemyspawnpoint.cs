using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

[System.Serializable]
public class EnemyType
{
    public GameObject prefab;
    public int cost;
    [Range(0, 100)]
    public int weight = 50; // ��� ����������� ������
}

[System.Serializable]
public class WaveDefinition
{
    public int pointBudget = 10;
    public float spawnDelay = 0.5f;
    public bool requireAllDead = true; // ���� false � ����� ������������� �� ������� ����
    public float maxDuration = 999f; // ���� requireAllDead == false, �� maxDuration �������� �����
    public bool isBossWave = false;
}

public class enemyspawnpoint : MonoBehaviour
{
    [Header("Enemies")]
    public List<EnemyType> enemyTypes;

    [Header("Wave config")]
    public List<WaveDefinition> waves;
    public int startingWaveIndex = 0;

    [Header("Spawn")]
    public Transform[] spawnPoints;
    public Transform baseTarget;
    public Transform rewardSpawnPoint; // ���� ��������� �������

    [Header("General")]
    [Header("Reward Variants")]
    public GameObject[] rewardPrefabs;

    public GameObject poolParent;
    public UnityEvent<int> OnWaveStarted; // ������� ������ �����
    public UnityEvent<int> OnWaveFinished; // ������� ������ �����

    private List<GameObject> aliveEnemies = new List<GameObject>();
    private int currentWave = 0;
    private Coroutine waveRoutine;
    private bool waitingForPickup = false;

    void Start()
    {
        currentWave = Mathf.Clamp(startingWaveIndex, 0, waves.Count - 1);
        StartNextWave();
    }

    void StartNextWave()
    {
        if (waveRoutine != null) StopCoroutine(waveRoutine);
        waveRoutine = StartCoroutine(WaveCoroutine(currentWave));
    }

    IEnumerator WaveCoroutine(int waveIndex)
    {
        if (waveIndex < 0 || waveIndex >= waves.Count) yield break;
        WaveDefinition def = waves[waveIndex];

        OnWaveStarted?.Invoke(waveIndex);

        int points = def.pointBudget;
        float timer = 0f;

        // ���� ���� ���� � �������.
        while (points > 0)
        {
            EnemyType e = PickWeightedEnemy(points);
            if (e == null) break;

            Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemy = Instantiate(e.prefab, sp.position, Quaternion.identity);
            var ec = enemy.GetComponent<EnemyController>();
            if (ec != null) ec.SetDefaultTarget(baseTarget);

            aliveEnemies.Add(enemy);
            points -= e.cost;

            yield return new WaitForSeconds(def.spawnDelay);
            timer += def.spawnDelay;

            // �������� ����� ���� ������������ ����������
            if (!def.requireAllDead && timer >= def.maxDuration) break;
        }

        // ��� ��������� �����:
        if (def.requireAllDead)
        {
            // ��� ���� ������ ����� ������ ����
            yield return new WaitUntil(() => aliveEnemies.Count == 0);
        }
        else
        {
            // ��� ���� ���� ��� ������, ���� ���� �� ������ maxDuration
            float elapsed = 0f;
            while (elapsed < def.maxDuration && aliveEnemies.Count > 0)
            {
                yield return null;
                elapsed += Time.deltaTime;
            }
        }

        SpawnReward();

        OnWaveFinished?.Invoke(waveIndex);
    }

    // ���������� ����� �����, �������� ������� ������
    EnemyType PickWeightedEnemy(int budget)
    {
        var candidates = enemyTypes.Where(e => e.cost <= budget).ToList();
        if (candidates.Count == 0) return null;

        int total = candidates.Sum(c => Mathf.Max(1, c.weight));
        int r = Random.Range(0, total);
        int acc = 0;
        foreach (var c in candidates)
        {
            acc += Mathf.Max(1, c.weight);
            if (r < acc) return c;
        }
        return candidates[0];
    }

    private List<GameObject> unusedRewards = new List<GameObject>();

    void SpawnReward()
    {
        // ���� ������ ���� � ��������� ��� ����� ���������
        if (unusedRewards.Count == 0)
        {
            unusedRewards = new List<GameObject>(rewardPrefabs);
            // ���� �� ������ �������� ������, ������ ���������������� ��� ������,
            // ����� ����� 6-� ������� ������ ������ �� �������.
        }

        // ���� ��������� ������ �� ���������� ������
        int index = Random.Range(0, unusedRewards.Count);
        GameObject prefab = unusedRewards[index];

        // ������� ��������� ������� �� ������, ����� �� �����������
        unusedRewards.RemoveAt(index);

        // ������� �������
        Vector3 spawnPos = rewardSpawnPoint ? rewardSpawnPoint.position : Vector3.zero;
        Instantiate(prefab, spawnPos, Quaternion.identity);
        waitingForPickup = true;

        Debug.Log($"Spawned unique reward: {prefab.name}");
    }



    // ���������� ������ � OnDestroy/OnDeath, ����� ��������� ������� �� ������
    public void UnregisterEnemy(GameObject enemy)
    {
        if (aliveEnemies.Contains(enemy))
            aliveEnemies.Remove(enemy);
    }

    // �������� ��� �������� ������� (��������, ����� ��������� � ��������)
    public void OnRewardDelivered()
    {
        if (!waitingForPickup) return;
        waitingForPickup = false;

        // ����������� ������ �����, �� �� ������� �� �������
        currentWave++;
        if (currentWave >= waves.Count)
        {
            // ����� ����������� ��� ����������� ��������� � ���������
            currentWave = waves.Count - 1; // ���� ������ ��������� ������
        }
        StartNextWave();
    }
}
