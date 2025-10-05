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
    public int weight = 50; // для взвешенного выбора
}

[System.Serializable]
public class WaveDefinition
{
    public int pointBudget = 10;
    public float spawnDelay = 0.5f;
    public bool requireAllDead = true; // если false — волна заканчивается по таймеру ниже
    public float maxDuration = 999f; // если requireAllDead == false, то maxDuration завершит волну
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
    public Transform rewardSpawnPoint; // куда поместить награду

    [Header("General")]
    [Header("Reward Variants")]
    public GameObject[] rewardPrefabs;

    public GameObject poolParent;
    public UnityEvent<int> OnWaveStarted; // передаём индекс волны
    public UnityEvent<int> OnWaveFinished; // передаём индекс волны

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

        // Пока есть очки — спавним.
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

            // защитный выход если длительность ограничена
            if (!def.requireAllDead && timer >= def.maxDuration) break;
        }

        // Ждём окончания волны:
        if (def.requireAllDead)
        {
            // Ждём пока список живых врагов пуст
            yield return new WaitUntil(() => aliveEnemies.Count == 0);
        }
        else
        {
            // Ждём либо пока все мертвы, либо пока не прошёл maxDuration
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

    // Взвешенный выбор врага, учитывая текущий бюджет
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
        // Если список пуст — заполняем его всеми префабами
        if (unusedRewards.Count == 0)
        {
            unusedRewards = new List<GameObject>(rewardPrefabs);
            // Если не хочешь повторов вообще, можешь закомментировать эту строку,
            // тогда после 6-й награды больше ничего не выпадет.
        }

        // Берём случайный индекс из оставшихся наград
        int index = Random.Range(0, unusedRewards.Count);
        GameObject prefab = unusedRewards[index];

        // Удаляем выбранную награду из списка, чтобы не повторялась
        unusedRewards.RemoveAt(index);

        // Спавним награду
        Vector3 spawnPos = rewardSpawnPoint ? rewardSpawnPoint.position : Vector3.zero;
        Instantiate(prefab, spawnPos, Quaternion.identity);
        waitingForPickup = true;

        Debug.Log($"Spawned unique reward: {prefab.name}");
    }



    // Вызывается врагом в OnDestroy/OnDeath, чтобы корректно удалить из списка
    public void UnregisterEnemy(GameObject enemy)
    {
        if (aliveEnemies.Contains(enemy))
            aliveEnemies.Remove(enemy);
    }

    // Вызывать при доставке награды (например, игрок поднимает и приносит)
    public void OnRewardDelivered()
    {
        if (!waitingForPickup) return;
        waitingForPickup = false;

        // увеличиваем индекс волны, но не выходим за границы
        currentWave++;
        if (currentWave >= waves.Count)
        {
            // можно зацикливать или увеличивать сложность и повторять
            currentWave = waves.Count - 1; // пока держим последнюю волной
        }
        StartNextWave();
    }
}
