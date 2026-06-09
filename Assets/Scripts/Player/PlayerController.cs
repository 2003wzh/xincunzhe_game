using UnityEngine;

namespace XianxiaSurvivor.Player
{
    /// <summary>
    /// 用途：读取玩家方向输入，并通过 Rigidbody2D 控制玩家在 2D 平面移动。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerController : MonoBehaviour
    {
        private Rigidbody2D body;
        private PlayerStats stats;
        private Vector2 moveInput;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            stats = GetComponent<PlayerStats>();
        }

        private void Update()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            moveInput = new Vector2(horizontal, vertical);

            if (moveInput.sqrMagnitude > 1f)
            {
                moveInput.Normalize();
            }
        }

        private void FixedUpdate()
        {
            if (stats == null || !stats.IsAlive)
            {
                return;
            }

            Vector2 nextPosition = body.position + moveInput * stats.MoveSpeed * Time.fixedDeltaTime;
            body.MovePosition(nextPosition);
        }
    }
}
