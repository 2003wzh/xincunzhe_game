using UnityEngine;

namespace XianxiaSurvivor.Combat
{
    /// <summary>
    /// 用途：在指定范围和图层内寻找最近的可受伤目标，供自动技能索敌使用。
    /// </summary>
    public class TargetFinder
    {
        private readonly Collider2D[] results;

        public TargetFinder(int maxResults = 32)
        {
            results = new Collider2D[Mathf.Max(1, maxResults)];
        }

        public Transform FindNearestDamageable(Vector2 center, float radius, LayerMask targetLayerMask)
        {
            if (radius <= 0f || targetLayerMask.value == 0)
            {
                return null;
            }

            int hitCount = Physics2D.OverlapCircleNonAlloc(center, radius, results, targetLayerMask);
            Transform nearestTarget = null;
            float nearestSqrDistance = float.MaxValue;

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

                if (damageable == null || !damageable.IsAlive)
                {
                    continue;
                }

                Transform candidateTransform = GetDamageableTransform(damageable, candidate.transform);
                float sqrDistance = ((Vector2)candidateTransform.position - center).sqrMagnitude;

                if (sqrDistance < nearestSqrDistance)
                {
                    nearestSqrDistance = sqrDistance;
                    nearestTarget = candidateTransform;
                }
            }

            return nearestTarget;
        }

        private static Transform GetDamageableTransform(IDamageable damageable, Transform fallback)
        {
            Component component = damageable as Component;
            return component != null ? component.transform : fallback;
        }
    }
}
