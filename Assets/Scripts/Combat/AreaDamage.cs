using System.Collections.Generic;
using UnityEngine;

namespace XianxiaSurvivor.Combat
{
    /// <summary>
    /// 用途：在指定圆形范围内查找可受伤敌人，并通过 IDamageable 造成范围伤害。
    /// </summary>
    public class AreaDamage
    {
        private readonly Collider2D[] results;
        private readonly List<IDamageable> damagedTargets = new List<IDamageable>(16);

        public AreaDamage(int maxResults = 32)
        {
            results = new Collider2D[Mathf.Max(1, maxResults)];
        }

        public void ApplyDamage(Vector2 center, float radius, int damage, GameObject source, LayerMask targetLayerMask)
        {
            if (radius <= 0f || damage <= 0 || targetLayerMask.value == 0)
            {
                return;
            }

            damagedTargets.Clear();
            int hitCount = Physics2D.OverlapCircleNonAlloc(center, radius, results, targetLayerMask);

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D candidate = results[i];

                if (candidate == null)
                {
                    continue;
                }

                IDamageable damageable = candidate.GetComponent<IDamageable>();

                if (damageable == null)
                {
                    damageable = candidate.GetComponentInParent<IDamageable>();
                }

                if (damageable == null || !damageable.IsAlive || damagedTargets.Contains(damageable))
                {
                    continue;
                }

                damagedTargets.Add(damageable);
                damageable.TakeDamage(new DamageInfo(damage, source, center));
            }
        }
    }
}
