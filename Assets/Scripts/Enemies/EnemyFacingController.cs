using UnityEngine;

namespace XianxiaSurvivor.Enemies
{
    /// <summary>
    /// 用途：根据玩家相对位置翻转怪物 SpriteRenderer 朝向，不影响移动和战斗逻辑。
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyFacingController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Transform target;
        [SerializeField] private float deadZone = 0.05f;
        [SerializeField] private bool defaultFacesRight = true;

        private bool hasWarnedMissingSpriteRenderer;
        private bool hasWarnedMissingTarget;

        private void Awake()
        {
            CacheSpriteRenderer();
        }

        private void Start()
        {
            if (target == null)
            {
                FindPlayerByTagOnce();
            }
        }

        private void Update()
        {
            if (spriteRenderer == null)
            {
                WarnMissingSpriteRendererOnce();
                return;
            }

            if (target == null)
            {
                WarnMissingTargetOnce();
                return;
            }

            float xDelta = target.position.x - transform.position.x;

            if (Mathf.Abs(xDelta) < deadZone)
            {
                return;
            }

            bool shouldFaceRight = xDelta > 0f;
            spriteRenderer.flipX = defaultFacesRight ? !shouldFaceRight : shouldFaceRight;
        }

        private void OnValidate()
        {
            deadZone = Mathf.Max(0f, deadZone);
        }

        private void CacheSpriteRenderer()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (spriteRenderer == null)
            {
                WarnMissingSpriteRendererOnce();
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

        private void WarnMissingSpriteRendererOnce()
        {
            if (hasWarnedMissingSpriteRenderer)
            {
                return;
            }

            hasWarnedMissingSpriteRenderer = true;
            Debug.LogWarning("EnemyFacingController 找不到 SpriteRenderer，怪物朝向不会翻转。", this);
        }

        private void WarnMissingTargetOnce()
        {
            if (hasWarnedMissingTarget)
            {
                return;
            }

            hasWarnedMissingTarget = true;
            Debug.LogWarning("EnemyFacingController 找不到 Player 目标，请确认 Player Tag 或手动绑定 target。", this);
        }
    }
}
