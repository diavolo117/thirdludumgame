using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class EnemyType
    {
        public GameObject prefab;
        public int cost;
    }

    public List<EnemyType> enemyTypes;
    public Transform[] spawnPoints;
    public Transform baseTarget;

    public int wavePoints = 10;
    public float spawnDelay = 0.5f;
    public GameObject rewardPrefab;

    private List<GameObject> aliveEnemies = new List<GameObject>();
    private bool waveActive = false;
    private bool waitingForPickup = false;

    void Start()
    {
        StartCoroutine(StartWave());
    }

    IEnumerator StartWave()
    {
        waveActive = true;
        waitingForPickup = false;

        int points = wavePoints;
        while (points > 0)
        {
            EnemyType e = enemyTypes[Random.Range(0, enemyTypes.Count)];
            if (e.cost <= points)
            {
                Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
                GameObject enemy = Instantiate(e.prefab, sp.position, Quaternion.identity);
                enemy.GetComponent<EnemyController>().SetDefaultTarget(baseTarget);
                aliveEnemies.Add(enemy);
                points -= e.cost;
                yield return new WaitForSeconds(spawnDelay);
            }
            else break;
        }

        // ждём, пока враги умрут
        yield return new WaitUntil(() => aliveEnemies.TrueForAll(e => e == null));

        waveActive = false;
        SpawnReward();
    }

    void SpawnReward()
    {
        if (rewardPrefab != null)
        {
            Instantiate(rewardPrefab, Vector3.zero, Quaternion.identity);
            waitingForPickup = true;
        }
    }

    public void OnRewardDelivered()
    {
        if (!waitingForPickup) return;

        waitingForPickup = false;
        wavePoints += 5; // пример роста сложности
        StartCoroutine(StartWave());
    }

    public void UnregisterEnemy(GameObject enemy)
    {
        if (aliveEnemies.Contains(enemy))
            aliveEnemies.Remove(enemy);
    }
}
