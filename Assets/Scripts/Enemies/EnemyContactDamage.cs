using UnityEngine;
using XianxiaSurvivor.Combat;
using XianxiaSurvivor.Player;

namespace XianxiaSurvivor.Enemies
{
    /// <summary>
    /// 用途：怪物通过 Trigger 接触玩家时，按固定间隔对玩家造成伤害。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(EnemyStats))]
    public class EnemyContactDamage : MonoBehaviour
    {
        [SerializeField] private float damageInterval = 0.75f;

        private EnemyStats stats;
        private float nextDamageTime;

        private void Awake()
        {
            stats = GetComponent<EnemyStats>();
        }

        private void OnEnable()
        {
            nextDamageTime = 0f;
        }

        private void OnValidate()
        {
            damageInterval = Mathf.Max(0.05f, damageInterval);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryDamagePlayer(other);
        }

        private void TryDamagePlayer(Collider2D other)
        {
            if (stats == null || !stats.IsAlive || Time.time < nextDamageTime)
            {
                return;
            }

            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth == null)
            {
                playerHealth = other.GetComponentInParent<PlayerHealth>();
            }

            if (playerHealth == null || !playerHealth.IsAlive)
            {
                return;
            }

            Vector2 hitPoint = other.ClosestPoint(transform.position);
            playerHealth.TakeDamage(new DamageInfo(stats.Damage, gameObject, hitPoint));
            nextDamageTime = Time.time + damageInterval;
        }
    }
}
