using System;
using UnityEngine;

namespace XianxiaSurvivor.Enemies
{
    /// <summary>
    /// 用途：描述一段基础刷怪波次，包括时间范围、刷怪间隔、数量上限和怪物 prefab。
    /// </summary>
    [Serializable]
    public class SpawnWaveConfig
    {
        [SerializeField] private string waveName = "低级妖兽";
        [SerializeField] private float startSeconds;
        [SerializeField] private float endSeconds = 300f;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private float spawnInterval = 2f;
        [SerializeField] private int spawnCount = 1;
        [SerializeField] private int maxAlive = 30;

        public string WaveName => waveName;
        public float StartSeconds => Mathf.Max(0f, startSeconds);
        public float EndSeconds => Mathf.Max(StartSeconds, endSeconds);
        public GameObject EnemyPrefab => enemyPrefab;
        public float SpawnInterval => Mathf.Max(0.1f, spawnInterval);
        public int SpawnCount => Mathf.Max(1, spawnCount);
        public int MaxAlive => Mathf.Max(1, maxAlive);

        public bool IsActive(float elapsedSeconds)
        {
            return enemyPrefab != null
                && elapsedSeconds >= StartSeconds
                && elapsedSeconds < EndSeconds;
        }
    }
}
