using UnityEngine;
using XianxiaSurvivor.Utils;

namespace XianxiaSurvivor.Combat
{
    /// <summary>
    /// 用途：控制飞剑投射物的移动、碰撞命中和伤害结算。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        private const float MinimumMoveSpeed = 0.1f;
        private const float MinimumMaxDistance = 0.1f;

        [Header("Visual Effects")]
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private bool spawnHitEffect = true;

        [Header("Debug")]
        [SerializeField] private bool debugCollisionLog = true;

        private Vector2 direction = Vector2.right;
        private int damage;
        private float speed = 10f;
        private float maxDistance = 10f;
        private float traveledDistance;
        private GameObject source;
        private LayerMask targetLayerMask;
        private Rigidbody2D body;
        private ObjectPool ownerPool;
        private bool isInitialized;
        private bool hasHit;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();

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
            isInitialized = false;
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
                DisableProjectile();
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
            damageable.TakeDamage(new DamageInfo(damage, source, transform.position));
            SpawnHitEffect();
            DisableProjectile();
        }

        public void Initialize(
            Vector2 moveDirection,
            int projectileDamage,
            float projectileSpeed,
            float projectileMaxDistance,
            GameObject damageSource,
            LayerMask damageTargetLayerMask,
            ObjectPool pool = null)
        {
            ownerPool = pool;

            if (moveDirection.sqrMagnitude <= 0.0001f)
            {
                DisableProjectile();
                return;
            }

            direction = moveDirection.normalized;
            damage = Mathf.Max(0, projectileDamage);
            speed = Mathf.Max(MinimumMoveSpeed, projectileSpeed);
            maxDistance = Mathf.Max(MinimumMaxDistance, projectileMaxDistance);
            source = damageSource;
            targetLayerMask = damageTargetLayerMask;
            traveledDistance = 0f;
            hasHit = false;
            isInitialized = true;
            RotateToDirection();
        }

        private void RotateToDirection()
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void SpawnHitEffect()
        {
            if (!spawnHitEffect || hitEffectPrefab == null)
            {
                return;
            }

            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
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

        private void DisableProjectile()
        {
            isInitialized = false;

            if (ownerPool != null)
            {
                ownerPool.Release(gameObject);
                return;
            }

            Destroy(gameObject);
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
            bool willHit = isInitialized && !hasHit && other != null && passesLayerMask && !ignoredBySourceOrProjectile && hasValidDamageable;
            bool willSpawnHitEffect = willHit && spawnHitEffect && hitEffectPrefab != null;
            bool willDestroyProjectile = willHit;

            Debug.Log(
                "[Projectile] Collision diagnostic"
                + $" | projectile={projectileName}"
                + $" | other={otherName}"
                + $" | otherLayer={otherLayer}"
                + $" | otherLayerName={otherLayerName}"
                + $" | otherRoot={otherRootName}"
                + $" | source={sourceName}"
                + $" | targetLayerMask={targetLayerMask.value}"
                + $" | isInitialized={isInitialized}"
                + $" | hasHit={hasHit}"
                + $" | passesLayerMask={passesLayerMask}"
                + $" | ignoredBySourceOrProjectile={ignoredBySourceOrProjectile}"
                + $" | hasIDamageable={damageable != null}"
                + $" | damageableAlive={hasValidDamageable}"
                + $" | willSpawnHitEffect={willSpawnHitEffect}"
                + $" | willDestroyProjectile={willDestroyProjectile}");
        }

        private static bool IsLayerInMask(int layer, LayerMask layerMask)
        {
            return (layerMask.value & (1 << layer)) != 0;
        }
    }
}
