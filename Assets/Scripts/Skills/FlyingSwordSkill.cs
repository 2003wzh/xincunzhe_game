using UnityEngine;
using XianxiaSurvivor.Combat;
using XianxiaSurvivor.Data;
using XianxiaSurvivor.Utils;

namespace XianxiaSurvivor.Skills
{
    /// <summary>
    /// 用途：实现飞剑术的自动冷却、索敌和发射逻辑。
    /// </summary>
    [DisallowMultipleComponent]
    public class FlyingSwordSkill : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private SkillData skillData;
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform firePoint;

        [Header("Target")]
        [SerializeField] private LayerMask enemyLayerMask;
        [SerializeField] private int maxTargetResults = 32;
        [SerializeField] private float readySearchInterval = 0.1f;

        [Header("Optional Pool")]
        [SerializeField] private ObjectPool projectilePool;

        private TargetFinder targetFinder;
        private float cooldownTimer;
        private float searchTimer;
        private bool warnedMissingSkillData;
        private bool warnedMissingProjectile;
        private bool warnedMissingLayerMask;
        private bool warnedPoolMissingProjectile;

        private void Awake()
        {
            if (firePoint == null)
            {
                firePoint = transform;
            }

            targetFinder = new TargetFinder(maxTargetResults);
        }

        private void OnEnable()
        {
            cooldownTimer = 0f;
            searchTimer = 0f;
        }

        private void OnValidate()
        {
            maxTargetResults = Mathf.Max(1, maxTargetResults);
            readySearchInterval = Mathf.Max(0.02f, readySearchInterval);
        }

        public void Tick(float deltaTime)
        {
            if (!isActiveAndEnabled || !HasRequiredSetup())
            {
                return;
            }

            cooldownTimer -= deltaTime;
            if (cooldownTimer > 0f)
            {
                return;
            }

            searchTimer -= deltaTime;
            if (searchTimer > 0f)
            {
                return;
            }

            Vector2 origin = firePoint != null ? firePoint.position : transform.position;
            Transform target = targetFinder.FindNearestDamageable(origin, skillData.Range, enemyLayerMask);

            if (target == null)
            {
                searchTimer = readySearchInterval;
                return;
            }

            if (TryFireAt(target))
            {
                cooldownTimer = skillData.Cooldown;
                searchTimer = 0f;
            }
        }

        private bool TryFireAt(Transform target)
        {
            if (target == null)
            {
                return false;
            }

            Vector3 origin = firePoint != null ? firePoint.position : transform.position;
            Vector2 direction = target.position - origin;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            Quaternion rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            Projectile projectile = SpawnProjectile(origin, rotation, out ObjectPool ownerPool);

            if (projectile == null)
            {
                return false;
            }

            projectile.Initialize(
                direction,
                skillData.Damage,
                skillData.ProjectileSpeed,
                skillData.ProjectileMaxDistance,
                gameObject,
                enemyLayerMask,
                ownerPool);

            return true;
        }

        private Projectile SpawnProjectile(Vector3 position, Quaternion rotation, out ObjectPool ownerPool)
        {
            ownerPool = null;

            if (projectilePool != null)
            {
                GameObject pooledObject = projectilePool.Get(position, rotation);

                if (pooledObject != null)
                {
                    Projectile pooledProjectile = pooledObject.GetComponent<Projectile>();

                    if (pooledProjectile != null)
                    {
                        ownerPool = projectilePool;
                        return pooledProjectile;
                    }

                    LogWarningOnce(ref warnedPoolMissingProjectile, "Projectile Pool 取出的对象缺少 Projectile 组件。");
                    projectilePool.Release(pooledObject);
                }
            }

            if (projectilePrefab == null)
            {
                LogWarningOnce(ref warnedMissingProjectile, "FlyingSwordSkill 缺少 Projectile Prefab，无法发射飞剑。");
                return null;
            }

            return Instantiate(projectilePrefab, position, rotation);
        }

        private bool HasRequiredSetup()
        {
            if (skillData == null)
            {
                LogWarningOnce(ref warnedMissingSkillData, "FlyingSwordSkill 缺少 Skill Data。");
                return false;
            }

            if (projectilePrefab == null && projectilePool == null)
            {
                LogWarningOnce(ref warnedMissingProjectile, "FlyingSwordSkill 需要 Projectile Prefab，或可选的 Projectile Pool。");
                return false;
            }

            if (enemyLayerMask.value == 0)
            {
                LogWarningOnce(ref warnedMissingLayerMask, "FlyingSwordSkill 缺少 Enemy Layer Mask。");
                return false;
            }

            return true;
        }

        private void LogWarningOnce(ref bool warned, string message)
        {
            if (warned)
            {
                return;
            }

            warned = true;
            Debug.LogWarning(message, this);
        }
    }
}
