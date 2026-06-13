using UnityEngine;
using XianxiaSurvivor.Combat;
using XianxiaSurvivor.Core;

namespace XianxiaSurvivor.Enemies
{
    /// <summary>
    /// 用途：处理怪物受伤、死亡、掉落灵气球，并通过事件通知其他系统。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyStats))]
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private ExpOrb expOrbPrefab;

        private EnemyStats stats;
        private HitFlashFeedback hitFlashFeedback;
        private bool hasDied;

        public bool IsAlive => stats != null && stats.IsAlive && !hasDied;

        private void Awake()
        {
            stats = GetComponent<EnemyStats>();
            hitFlashFeedback = GetComponent<HitFlashFeedback>();

            if (hitFlashFeedback == null)
            {
                hitFlashFeedback = GetComponentInChildren<HitFlashFeedback>();
            }
        }

        private void OnEnable()
        {
            if (stats == null)
            {
                stats = GetComponent<EnemyStats>();
            }

            if (hitFlashFeedback == null)
            {
                hitFlashFeedback = GetComponent<HitFlashFeedback>();
            }

            if (hitFlashFeedback == null)
            {
                hitFlashFeedback = GetComponentInChildren<HitFlashFeedback>();
            }

            hasDied = false;

            if (stats != null)
            {
                stats.ResetHp();
            }
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (!IsAlive)
            {
                return;
            }

            int damageAmount = Mathf.Max(0, damageInfo.Amount);

            if (damageAmount <= 0)
            {
                return;
            }

            int previousHp = stats.CurrentHp;
            stats.TakeDamage(damageAmount);
            hitFlashFeedback?.PlayFeedback();

            int delta = stats.CurrentHp - previousHp;
            EventBus.Raise(new EnemyHealthChangedEvent(gameObject, stats.CurrentHp, stats.MaxHp, delta));

            if (stats.CurrentHp <= 0)
            {
                Die();
            }
        }

        [ContextMenu("Debug Kill Enemy")]
        private void DebugKillEnemy()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Debug Kill Enemy 只用于 Play Mode 测试。", this);
                return;
            }

            if (stats == null)
            {
                stats = GetComponent<EnemyStats>();
            }

            if (stats != null)
            {
                stats.SetCurrentHp(0);
            }

            Die();
        }

        private void Die()
        {
            if (hasDied)
            {
                return;
            }

            hasDied = true;

            bool isBoss = GetComponent<BossMarker>() != null;
            int expDrop = stats != null ? stats.ExpDrop : 0;
            Vector3 deathPosition = transform.position;

            EventBus.Raise(new EnemyDiedEvent(gameObject, isBoss, expDrop, deathPosition));

            if (isBoss)
            {
                EventBus.Raise(new BossDiedEvent(gameObject));
            }

            DropExpOrb(deathPosition, expDrop);
            gameObject.SetActive(false);
        }

        private void DropExpOrb(Vector3 position, int expAmount)
        {
            if (expOrbPrefab == null || expAmount <= 0)
            {
                return;
            }

            ExpOrb expOrb = Instantiate(expOrbPrefab, position, Quaternion.identity);
            expOrb.SetExpAmount(expAmount);
        }
    }
}
