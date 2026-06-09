using UnityEngine;
using XianxiaSurvivor.Player;

namespace XianxiaSurvivor.Enemies
{
    /// <summary>
    /// 用途：控制怪物朝玩家移动，目标优先由 EnemySpawner 在生成时注入。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyStats))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private Transform target;

        private EnemyStats stats;
        private Rigidbody2D body;

        private void Awake()
        {
            stats = GetComponent<EnemyStats>();
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
        }

        private void Start()
        {
            if (target == null)
            {
                FindPlayerOnce();
            }
        }

        private void FixedUpdate()
        {
            if (target == null || stats == null || !stats.IsAlive)
            {
                return;
            }

            Vector2 currentPosition = body.position;
            Vector2 targetPosition = target.position;
            Vector2 direction = targetPosition - currentPosition;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Vector2 nextPosition = currentPosition + direction.normalized * stats.MoveSpeed * Time.fixedDeltaTime;
            body.MovePosition(nextPosition);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void FindPlayerOnce()
        {
            PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();

            if (playerHealth != null)
            {
                target = playerHealth.transform;
            }
        }

        private void Reset()
        {
            Rigidbody2D attachedBody = GetComponent<Rigidbody2D>();

            if (attachedBody != null)
            {
                attachedBody.gravityScale = 0f;
                attachedBody.freezeRotation = true;
            }
        }
    }
}
