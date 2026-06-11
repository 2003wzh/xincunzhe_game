using UnityEngine;
using XianxiaSurvivor.Combat;
using XianxiaSurvivor.Data;

namespace XianxiaSurvivor.Skills
{
    /// <summary>
    /// 用途：实现离火诀的自动冷却、索敌和火球发射逻辑。
    /// </summary>
    [DisallowMultipleComponent]
    public class FireballSkill : MonoBehaviour, IPlayerSkill
    {
        private const float MinimumDamage = 1f;
        private const float MinimumCooldown = 0.3f;
        private const float MinimumRange = 0.1f;
        private const float MinimumExplosionRadius = 0.1f;
        private const float MaximumExplosionRadius = 4f;

        [Header("Config")]
        [SerializeField] private SkillData skillData;
        [SerializeField] private ExplosiveProjectile projectilePrefab;
        [SerializeField] private Transform firePoint;

        [Header("Target")]
        [SerializeField] private LayerMask enemyLayerMask;
        [SerializeField] private int maxTargetResults = 32;
        [SerializeField] private float readySearchInterval = 0.1f;

        private TargetFinder targetFinder;
        private float cooldownTimer;
        private float searchTimer;
        private bool hasRuntimeValues;
        private bool warnedMissingSkillData;
        private bool warnedMissingProjectile;
        private bool warnedMissingLayerMask;
        private float runtimeDamage;
        private float runtimeCooldown;
        private float runtimeRange;
        private float runtimeProjectileSpeed;
        private float runtimeProjectileMaxDistance;
        private float runtimeExplosionRadius;

        public int CurrentDamage => Mathf.Max(1, Mathf.RoundToInt(runtimeDamage));
        public float CurrentCooldown => Mathf.Max(MinimumCooldown, runtimeCooldown);
        public float CurrentRange => Mathf.Max(MinimumRange, runtimeRange);
        public float CurrentExplosionRadius => Mathf.Clamp(runtimeExplosionRadius, MinimumExplosionRadius, MaximumExplosionRadius);

        private void Awake()
        {
            if (firePoint == null)
            {
                firePoint = transform;
            }

            targetFinder = new TargetFinder(maxTargetResults);
            InitializeRuntimeValues();
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

            EnsureTargetFinder();

            Vector2 origin = firePoint != null ? firePoint.position : transform.position;
            Transform target = targetFinder.FindNearestDamageable(origin, CurrentRange, enemyLayerMask);

            if (target == null)
            {
                searchTimer = readySearchInterval;
                return;
            }

            if (TryFireAt(target))
            {
                cooldownTimer = CurrentCooldown;
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
            ExplosiveProjectile projectile = Instantiate(projectilePrefab, origin, rotation);

            if (projectile == null)
            {
                return false;
            }

            projectile.Initialize(
                direction,
                CurrentDamage,
                runtimeProjectileSpeed,
                runtimeProjectileMaxDistance,
                CurrentExplosionRadius,
                gameObject,
                enemyLayerMask);

            return true;
        }

        private bool HasRequiredSetup()
        {
            if (skillData == null)
            {
                LogWarningOnce(ref warnedMissingSkillData, "FireballSkill 缺少 Skill Data。");
                return false;
            }

            if (!hasRuntimeValues)
            {
                InitializeRuntimeValues();
            }

            if (projectilePrefab == null)
            {
                LogWarningOnce(ref warnedMissingProjectile, "FireballSkill 缺少 Projectile Prefab，无法发射火球。");
                return false;
            }

            if (enemyLayerMask.value == 0)
            {
                LogWarningOnce(ref warnedMissingLayerMask, "FireballSkill 缺少 Enemy Layer Mask。");
                return false;
            }

            return true;
        }

        private void EnsureTargetFinder()
        {
            if (targetFinder == null)
            {
                targetFinder = new TargetFinder(maxTargetResults);
            }
        }

        public void AddDamage(float value)
        {
            if (!EnsureRuntimeValues())
            {
                return;
            }

            runtimeDamage = Mathf.Max(MinimumDamage, runtimeDamage + Mathf.Max(0f, value));
        }

        public void ReduceCooldown(float value)
        {
            if (!EnsureRuntimeValues())
            {
                return;
            }

            runtimeCooldown = Mathf.Max(MinimumCooldown, runtimeCooldown - Mathf.Max(0f, value));

            if (cooldownTimer > runtimeCooldown)
            {
                cooldownTimer = runtimeCooldown;
            }
        }

        public void AddExplosionRadius(float value)
        {
            if (!EnsureRuntimeValues())
            {
                return;
            }

            runtimeExplosionRadius = Mathf.Clamp(
                runtimeExplosionRadius + Mathf.Max(0f, value),
                MinimumExplosionRadius,
                MaximumExplosionRadius);
        }

        private bool EnsureRuntimeValues()
        {
            if (!hasRuntimeValues)
            {
                InitializeRuntimeValues();
            }

            return hasRuntimeValues;
        }

        private void InitializeRuntimeValues()
        {
            if (skillData == null)
            {
                hasRuntimeValues = false;
                return;
            }

            runtimeDamage = skillData.Damage;
            runtimeCooldown = skillData.Cooldown;
            runtimeRange = skillData.Range;
            runtimeProjectileSpeed = skillData.ProjectileSpeed;
            runtimeProjectileMaxDistance = skillData.ProjectileMaxDistance;
            runtimeExplosionRadius = Mathf.Clamp(skillData.ExplosionRadius, MinimumExplosionRadius, MaximumExplosionRadius);
            hasRuntimeValues = true;
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
