using System.Collections;
using UnityEngine;
using XianxiaSurvivor.Combat;
using XianxiaSurvivor.Player;

namespace XianxiaSurvivor.Enemies
{
    /// <summary>
    /// Purpose: drives a single-target melee attack for enemies without changing movement AI.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyMeleeAttack : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Animator animator;
        [SerializeField] private MonoBehaviour movementController;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private float attackRange = 1.1f;
        [SerializeField] private float attackCooldown = 1.2f;
        [SerializeField] private float hitDelay = 0.25f;
        [SerializeField] private int damage = 0;
        [SerializeField] private string attackTriggerName = "Attack";
        [SerializeField] private bool debugLog = false;
        [SerializeField] private bool stopMovementInAttackRange = true;
        [SerializeField] private float resumeMoveRange = 1.45f;

        private EnemyStats stats;
        private Coroutine attackRoutine;
        private float nextAttackTime;
        private bool isAttacking;
        private bool hasWarnedMissingTarget;
        private bool hasWarnedMissingAnimator;
        private bool hasWarnedMissingStats;
        private bool hasWarnedMissingDamageable;

        private void Awake()
        {
            stats = GetComponent<EnemyStats>();
            CacheAnimator();
            CacheMovementReferences();
            EnsureValidResumeMoveRange();
        }

        private void Start()
        {
            if (target == null)
            {
                FindPlayerByTagOnce();
            }
        }

        private void OnDisable()
        {
            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
                attackRoutine = null;
            }

            isAttacking = false;
            EnableMovementController();
        }

        private void OnValidate()
        {
            attackRange = Mathf.Max(0f, attackRange);
            attackCooldown = Mathf.Max(0f, attackCooldown);
            hitDelay = Mathf.Max(0f, hitDelay);
            EnsureValidResumeMoveRange();
        }

        private void Update()
        {
            if (Time.timeScale <= 0f)
            {
                return;
            }

            if (target == null)
            {
                WarnMissingTargetOnce();
                return;
            }

            float distance = Vector2.Distance(transform.position, target.position);
            UpdateMovementStop(distance);

            if (isAttacking || Time.time < nextAttackTime)
            {
                return;
            }

            if (distance > attackRange)
            {
                return;
            }

            attackRoutine = StartCoroutine(AttackRoutine());
        }

        private IEnumerator AttackRoutine()
        {
            isAttacking = true;

            if (animator != null && !string.IsNullOrEmpty(attackTriggerName))
            {
                animator.SetTrigger(attackTriggerName);
            }
            else if (animator == null)
            {
                WarnMissingAnimatorOnce();
            }

            if (debugLog)
            {
                Debug.Log("EnemyMeleeAttack started.", this);
            }

            if (hitDelay > 0f)
            {
                yield return new WaitForSeconds(hitDelay);
            }

            if (isActiveAndEnabled && gameObject.activeInHierarchy && target != null && Time.timeScale > 0f)
            {
                TryApplyHit();
            }

            nextAttackTime = Time.time + attackCooldown;
            isAttacking = false;
            attackRoutine = null;
        }

        private void UpdateMovementStop(float distance)
        {
            if (!stopMovementInAttackRange)
            {
                EnableMovementController();
                return;
            }

            if (distance <= attackRange)
            {
                DisableMovementController();
                StopRigidbodyMovement();
                return;
            }

            if (distance >= resumeMoveRange)
            {
                EnableMovementController();
            }
        }

        private void TryApplyHit()
        {
            if (Vector2.Distance(transform.position, target.position) > attackRange)
            {
                if (debugLog)
                {
                    Debug.Log("EnemyMeleeAttack missed because target left range.", this);
                }

                return;
            }

            IDamageable damageable = target.GetComponent<IDamageable>();

            if (damageable == null)
            {
                damageable = target.GetComponentInParent<IDamageable>();
            }

            if (damageable == null)
            {
                PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();

                if (playerHealth == null)
                {
                    playerHealth = target.GetComponentInParent<PlayerHealth>();
                }

                damageable = playerHealth;
            }

            if (damageable == null)
            {
                WarnMissingDamageableOnce();
                return;
            }

            if (!damageable.IsAlive)
            {
                return;
            }

            int damageAmount = GetDamageAmount();
            damageable.TakeDamage(new DamageInfo(damageAmount, gameObject, target.position));

            if (debugLog)
            {
                Debug.Log($"EnemyMeleeAttack hit target for {damageAmount} damage.", this);
            }
        }

        private int GetDamageAmount()
        {
            if (damage > 0)
            {
                return damage;
            }

            if (stats != null)
            {
                return stats.Damage;
            }

            WarnMissingStatsOnce();
            return 1;
        }

        private void CacheAnimator()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (animator == null)
            {
                WarnMissingAnimatorOnce();
            }
        }

        private void CacheMovementReferences()
        {
            if (movementController == null)
            {
                movementController = GetComponent<EnemyController>();
            }

            if (rb == null)
            {
                rb = GetComponent<Rigidbody2D>();
            }
        }

        private void DisableMovementController()
        {
            if (movementController == null || movementController == this || !movementController.enabled)
            {
                return;
            }

            movementController.enabled = false;
        }

        private void EnableMovementController()
        {
            if (movementController == null || movementController == this || movementController.enabled)
            {
                return;
            }

            movementController.enabled = true;
        }

        private void StopRigidbodyMovement()
        {
            if (rb == null)
            {
                return;
            }

            rb.velocity = Vector2.zero;
        }

        private void EnsureValidResumeMoveRange()
        {
            if (resumeMoveRange <= attackRange)
            {
                resumeMoveRange = attackRange + 0.25f;
            }
        }

        private void FindPlayerByTagOnce()
        {
            GameObject playerObject = null;

            try
            {
                playerObject = GameObject.FindGameObjectWithTag("Player");
            }
            catch (UnityException)
            {
                WarnMissingTargetOnce();
                return;
            }

            if (playerObject != null)
            {
                target = playerObject.transform;
                return;
            }

            WarnMissingTargetOnce();
        }

        private void WarnMissingTargetOnce()
        {
            if (hasWarnedMissingTarget)
            {
                return;
            }

            hasWarnedMissingTarget = true;
            Debug.LogWarning("EnemyMeleeAttack cannot find Player target. Set target manually or ensure Player tag exists.", this);
        }

        private void WarnMissingAnimatorOnce()
        {
            if (hasWarnedMissingAnimator)
            {
                return;
            }

            hasWarnedMissingAnimator = true;
            Debug.LogWarning("EnemyMeleeAttack cannot find Animator. Damage still works, but attack animation will not play.", this);
        }

        private void WarnMissingStatsOnce()
        {
            if (hasWarnedMissingStats)
            {
                return;
            }

            hasWarnedMissingStats = true;
            Debug.LogWarning("EnemyMeleeAttack cannot find EnemyStats. Using fallback damage 1.", this);
        }

        private void WarnMissingDamageableOnce()
        {
            if (hasWarnedMissingDamageable)
            {
                return;
            }

            hasWarnedMissingDamageable = true;
            Debug.LogWarning("EnemyMeleeAttack target has no IDamageable or PlayerHealth component.", this);
        }
    }
}
