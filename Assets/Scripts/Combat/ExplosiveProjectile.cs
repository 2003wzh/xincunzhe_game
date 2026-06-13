using UnityEngine;

namespace XianxiaSurvivor.Combat
{
    /// <summary>
    /// 用途：控制火球投射物移动、直接命中伤害和命中点爆炸范围伤害。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public class ExplosiveProjectile : MonoBehaviour
    {
        private const float MinimumMoveSpeed = 0.1f;
        private const float MinimumMaxDistance = 0.1f;
        private const float MinimumExplosionRadius = 0.1f;

        [SerializeField] private int maxExplosionTargets = 32;

        [Header("Visual Effects")]
        [SerializeField] private GameObject explosionEffectPrefab;
        [SerializeField] private bool spawnExplosionEffect = true;

        [Header("Debug")]
        [SerializeField] private bool debugCollisionLog = true;

        private AreaDamage areaDamage;
        private Rigidbody2D body;
        private Vector2 direction = Vector2.right;
        private int damage;
        private float speed = 8f;
        private float maxDistance = 10f;
        private float explosionRadius = 1.5f;
        private float traveledDistance;
        private GameObject source;
        private LayerMask targetLayerMask;
        private bool isInitialized;
        private bool hasHit;
        private bool hasExploded;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            areaDamage = new AreaDamage(maxExplosionTargets);

            Collider2D triggerCollider = GetComponent<Collider2D>();
            if (triggerCollider != null)
            {
                triggerCollider.isTrigger = true;
            }

            if (body != null)
            {
                body.gravityScale = 0f;
                body.freezeRotation = true;
                body.bodyType = RigidbodyType2D.Kinematic;
            }
        }

        private void OnEnable()
        {
            traveledDistance = 0f;
            hasHit = false;
            hasExploded = false;
            isInitialized = false;
        }

        private void OnValidate()
        {
            maxExplosionTargets = Mathf.Max(1, maxExplosionTargets);
        }

        private void FixedUpdate()
        {
            if (!isInitialized || hasHit)
            {
                return;
            }

            float moveStep = speed * Time.fixedDeltaTime;
            Vector2 currentPosition = body != null ? body.position : (Vector2)transform.position;
            Vector2 nextPosition = currentPosition + direction * moveStep;

            if (body != null)
            {
                body.MovePosition(nextPosition);
            }
            else
            {
                transform.position = nextPosition;
            }

            traveledDistance += moveStep;

            if (traveledDistance >= maxDistance)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            LogCollisionDiagnostics(other);

            if (!isInitialized || hasHit || other == null)
            {
                return;
            }

            if (!IsLayerInMask(other.gameObject.layer, targetLayerMask))
            {
                return;
            }

            if (ShouldIgnoreCollision(other))
            {
                return;
            }

            IDamageable damageable = other.GetComponent<IDamageable>();

            if (damageable == null)
            {
                damageable = other.GetComponentInParent<IDamageable>();
            }

            if (damageable == null || !damageable.IsAlive)
            {
                return;
            }

            hasHit = true;
            Vector2 hitPoint = transform.position;
            damageable.TakeDamage(new DamageInfo(damage, source, hitPoint));
            Explode(hitPoint);
            Destroy(gameObject);
        }

        public void Initialize(
            Vector2 moveDirection,
            int projectileDamage,
            float projectileSpeed,
            float projectileMaxDistance,
            float projectileExplosionRadius,
            GameObject damageSource,
            LayerMask damageTargetLayerMask)
        {
            if (moveDirection.sqrMagnitude <= 0.0001f)
            {
                Destroy(gameObject);
                return;
            }

            direction = moveDirection.normalized;
            damage = Mathf.Max(0, projectileDamage);
            speed = Mathf.Max(MinimumMoveSpeed, projectileSpeed);
            maxDistance = Mathf.Max(MinimumMaxDistance, projectileMaxDistance);
            explosionRadius = Mathf.Max(MinimumExplosionRadius, projectileExplosionRadius);
            source = damageSource;
            targetLayerMask = damageTargetLayerMask;
            traveledDistance = 0f;
            hasHit = false;
            hasExploded = false;
            isInitialized = true;
            RotateToDirection();
        }

        private void RotateToDirection()
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void Explode(Vector2 hitPoint)
        {
            if (hasExploded)
            {
                return;
            }

            hasExploded = true;
            areaDamage.ApplyDamage(hitPoint, explosionRadius, damage, source, targetLayerMask);
            SpawnExplosionEffect(hitPoint);
        }

        private void SpawnExplosionEffect(Vector2 hitPoint)
        {
            if (!spawnExplosionEffect || explosionEffectPrefab == null)
            {
                return;
            }

            Instantiate(explosionEffectPrefab, hitPoint, Quaternion.identity);
        }

        private bool ShouldIgnoreCollision(Collider2D other)
        {
            Transform otherTransform = other.transform;

            if (other.gameObject == gameObject || otherTransform.IsChildOf(transform))
            {
                return true;
            }

            if (source == null)
            {
                return false;
            }

            Transform sourceTransform = source.transform;

            if (other.gameObject == source)
            {
                return true;
            }

            if (otherTransform.IsChildOf(sourceTransform))
            {
                return true;
            }

            return sourceTransform.IsChildOf(otherTransform);
        }

        private void LogCollisionDiagnostics(Collider2D other)
        {
            if (!debugCollisionLog)
            {
                return;
            }

            string projectileName = gameObject != null ? gameObject.name : "null";
            string otherName = other != null ? other.name : "null";
            int otherLayer = other != null ? other.gameObject.layer : -1;
            string otherLayerName = other != null ? LayerMask.LayerToName(otherLayer) : "null";
            string otherRootName = other != null ? other.transform.root.name : "null";
            string sourceName = source != null ? source.name : "null";
            bool passesLayerMask = other != null && IsLayerInMask(other.gameObject.layer, targetLayerMask);
            bool ignoredBySourceOrProjectile = other != null && ShouldIgnoreCollision(other);
            IDamageable damageable = null;

            if (other != null)
            {
                damageable = other.GetComponent<IDamageable>();

                if (damageable == null)
                {
                    damageable = other.GetComponentInParent<IDamageable>();
                }
            }

            bool hasValidDamageable = damageable != null && damageable.IsAlive;
            bool willExplode = isInitialized && !hasHit && other != null && passesLayerMask && !ignoredBySourceOrProjectile && hasValidDamageable;
            bool willSpawnExplosionEffect = willExplode && !hasExploded && spawnExplosionEffect && explosionEffectPrefab != null;
            bool willDestroyProjectile = willExplode;

            Debug.Log(
                "[ExplosiveProjectile] Collision diagnostic"
                + $" | projectile={projectileName}"
                + $" | other={otherName}"
                + $" | otherLayer={otherLayer}"
                + $" | otherLayerName={otherLayerName}"
                + $" | otherRoot={otherRootName}"
                + $" | source={sourceName}"
                + $" | targetLayerMask={targetLayerMask.value}"
                + $" | isInitialized={isInitialized}"
                + $" | hasHit={hasHit}"
                + $" | hasExploded={hasExploded}"
                + $" | passesLayerMask={passesLayerMask}"
                + $" | ignoredBySourceOrProjectile={ignoredBySourceOrProjectile}"
                + $" | hasIDamageable={damageable != null}"
                + $" | damageableAlive={hasValidDamageable}"
                + $" | willSpawnExplosionEffect={willSpawnExplosionEffect}"
                + $" | willDestroyProjectile={willDestroyProjectile}");
        }

        private static bool IsLayerInMask(int layer, LayerMask layerMask)
        {
            return (layerMask.value & (1 << layer)) != 0;
        }
    }
}
