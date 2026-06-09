using UnityEngine;
using XianxiaSurvivor.Combat;
using XianxiaSurvivor.Core;

namespace XianxiaSurvivor.Player
{
    /// <summary>
    /// 用途：处理玩家受伤和死亡，并通过事件通知其他系统。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        private PlayerStats stats;
        private bool hasDied;

        public bool IsAlive => stats != null && stats.IsAlive && !hasDied;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
            hasDied = stats != null && !stats.IsAlive;
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
            stats.SetCurrentHp(previousHp - damageAmount);

            int delta = stats.CurrentHp - previousHp;
            EventBus.Raise(new PlayerHealthChangedEvent(gameObject, stats.CurrentHp, stats.MaxHp, delta));

            if (stats.CurrentHp <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            if (hasDied)
            {
                return;
            }

            hasDied = true;
            EventBus.Raise(new PlayerDiedEvent(gameObject));
        }
    }
}
