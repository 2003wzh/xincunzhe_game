using UnityEngine;

namespace XianxiaSurvivor.Player
{
    /// <summary>
    /// 用途：根据玩家实际移动速度驱动 Idle/Run 动画，并处理角色左右朝向。
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Rigidbody2D body;

        [Header("Animator")]
        [SerializeField] private string speedParameter = "Speed";

        [Header("Movement")]
        [SerializeField] private float moveThreshold = 0.1f;
        [SerializeField] private float flipThreshold = 0.01f;

        private Vector3 lastPosition;
        private int speedHash;
        private bool warnedMissingAnimator;
        private bool warnedMissingSpriteRenderer;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (body == null)
            {
                body = GetComponent<Rigidbody2D>();
            }

            speedHash = Animator.StringToHash(speedParameter);
            lastPosition = GetCurrentPosition();
        }

        private void FixedUpdate()
        {
            Vector3 currentPosition = GetCurrentPosition();
            Vector3 delta = currentPosition - lastPosition;
            float speed = Time.fixedDeltaTime > 0f ? delta.magnitude / Time.fixedDeltaTime : 0f;

            UpdateAnimator(speed);
            UpdateFacing(delta.x);

            lastPosition = currentPosition;
        }

        private Vector3 GetCurrentPosition()
        {
            if (body != null)
            {
                return body.position;
            }

            return transform.position;
        }

        private void UpdateAnimator(float speed)
        {
            if (animator == null)
            {
                WarnOnce(ref warnedMissingAnimator, "PlayerAnimationController 缺少 Animator 引用。");
                return;
            }

            animator.SetFloat(speedHash, speed > moveThreshold ? speed : 0f);
        }

        private void UpdateFacing(float horizontalDelta)
        {
            if (spriteRenderer == null)
            {
                WarnOnce(ref warnedMissingSpriteRenderer, "PlayerAnimationController 缺少 SpriteRenderer 引用。");
                return;
            }

            if (Mathf.Abs(horizontalDelta) <= flipThreshold)
            {
                return;
            }

            spriteRenderer.flipX = horizontalDelta < 0f;
        }

        private void WarnOnce(ref bool hasWarned, string message)
        {
            if (hasWarned)
            {
                return;
            }

            Debug.LogWarning(message, this);
            hasWarned = true;
        }
    }
}
