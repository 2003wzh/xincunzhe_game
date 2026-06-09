using System.Collections.Generic;
using UnityEngine;
using XianxiaSurvivor.Core;
using XianxiaSurvivor.Player;

namespace XianxiaSurvivor.Enemies
{
    /// <summary>
    /// 用途：根据战斗时间在玩家周围随机刷怪，并在指定时间生成一次 Boss。
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private RunTimer runTimer;
        [SerializeField] private SpawnWaveConfig[] waves = new SpawnWaveConfig[0];
        [SerializeField] private float spawnDistance = 12f;
        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private float bossSpawnSeconds = 900f;
        [SerializeField] private bool stopNormalSpawnsAfterBoss = true;

        private float localElapsedSeconds;
        private float nextSpawnTime;
        private int aliveEnemyCount;
        private bool bossSpawned;
        private bool warnedMissingPlayer;
        private bool warnedMissingBossPrefab;
        private readonly HashSet<GameObject> spawnedEnemies = new HashSet<GameObject>();

        private void OnEnable()
        {
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
        }

        private void Start()
        {
            FindSceneReferencesOnce();
        }

        private void Update()
        {
            if (runTimer == null)
            {
                localElapsedSeconds += Time.deltaTime;
            }

            float elapsedSeconds = GetElapsedSeconds();

            TrySpawnBoss(elapsedSeconds);
            TrySpawnNormalEnemies(elapsedSeconds);
        }

        private void OnValidate()
        {
            spawnDistance = Mathf.Max(0f, spawnDistance);
            bossSpawnSeconds = Mathf.Max(0f, bossSpawnSeconds);
        }

        private void FindSceneReferencesOnce()
        {
            if (player == null)
            {
                PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();

                if (playerHealth != null)
                {
                    player = playerHealth.transform;
                }
            }

            if (runTimer == null)
            {
                runTimer = FindObjectOfType<RunTimer>();
            }
        }

        private float GetElapsedSeconds()
        {
            if (runTimer != null)
            {
                return runTimer.ElapsedSeconds;
            }

            return localElapsedSeconds;
        }

        private void TrySpawnNormalEnemies(float elapsedSeconds)
        {
            if (stopNormalSpawnsAfterBoss && bossSpawned)
            {
                return;
            }

            SpawnWaveConfig activeWave = GetActiveWave(elapsedSeconds);

            if (activeWave == null || Time.time < nextSpawnTime)
            {
                return;
            }

            if (aliveEnemyCount >= activeWave.MaxAlive)
            {
                return;
            }

            int availableSlots = activeWave.MaxAlive - aliveEnemyCount;
            int spawnTotal = Mathf.Min(activeWave.SpawnCount, availableSlots);

            for (int i = 0; i < spawnTotal; i++)
            {
                SpawnEnemy(activeWave.EnemyPrefab, false);
            }

            nextSpawnTime = Time.time + activeWave.SpawnInterval;
        }

        private SpawnWaveConfig GetActiveWave(float elapsedSeconds)
        {
            if (waves == null)
            {
                return null;
            }

            for (int i = 0; i < waves.Length; i++)
            {
                SpawnWaveConfig wave = waves[i];

                if (wave != null && wave.IsActive(elapsedSeconds))
                {
                    return wave;
                }
            }

            return null;
        }

        private void TrySpawnBoss(float elapsedSeconds)
        {
            if (bossSpawned || elapsedSeconds < bossSpawnSeconds)
            {
                return;
            }

            if (bossPrefab == null)
            {
                if (!warnedMissingBossPrefab)
                {
                    Debug.LogWarning("EnemySpawner 缺少 Boss Prefab，无法生成 Boss。", this);
                    warnedMissingBossPrefab = true;
                }

                return;
            }

            bossSpawned = SpawnEnemy(bossPrefab, true);
        }

        private bool SpawnEnemy(GameObject prefab, bool isBoss)
        {
            if (prefab == null)
            {
                return false;
            }

            if (player == null)
            {
                if (!warnedMissingPlayer)
                {
                    Debug.LogWarning("EnemySpawner 缺少玩家 Transform，无法刷怪。", this);
                    warnedMissingPlayer = true;
                }

                return false;
            }

            Vector3 spawnPosition = GetRandomSpawnPosition();
            GameObject enemy = Instantiate(prefab, spawnPosition, Quaternion.identity);

            EnemyController controller = enemy.GetComponent<EnemyController>();

            if (controller != null)
            {
                controller.SetTarget(player);
            }

            if (isBoss && enemy.GetComponent<BossMarker>() == null)
            {
                enemy.AddComponent<BossMarker>();
            }

            spawnedEnemies.Add(enemy);
            aliveEnemyCount++;
            return true;
        }

        private Vector3 GetRandomSpawnPosition()
        {
            Vector2 direction = Random.insideUnitCircle;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector2.right;
            }

            direction.Normalize();
            return player.position + (Vector3)(direction * spawnDistance);
        }

        private void OnEnemyDied(EnemyDiedEvent eventData)
        {
            if (spawnedEnemies.Remove(eventData.Enemy) && aliveEnemyCount > 0)
            {
                aliveEnemyCount--;
            }
        }
    }
}
