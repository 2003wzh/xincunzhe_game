using UnityEngine;
using XianxiaSurvivor.Player;

namespace XianxiaSurvivor.Combat
{
    /// <summary>
    /// 用途：表示怪物死亡后掉落的灵气球，玩家触发碰撞后吸收并获得经验。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CircleCollider2D))]
    public class ExpOrb : MonoBehaviour
    {
        [SerializeField] private int expAmount = 1;

        private bool isCollected;

        public int ExpAmount => expAmount;

        private void Reset()
        {
            Collider2D triggerCollider = GetComponent<Collider2D>();
            if (triggerCollider != null)
            {
                triggerCollider.isTrigger = true;
            }
        }

        private void OnValidate()
        {
            expAmount = Mathf.Max(1, expAmount);
        }

        private void OnEnable()
        {
            isCollected = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected)
            {
                return;
            }

            PlayerExperience playerExperience = other.GetComponent<PlayerExperience>();
            if (playerExperience == null)
            {
                playerExperience = other.GetComponentInParent<PlayerExperience>();
            }

            if (playerExperience == null)
            {
                return;
            }

            isCollected = true;
            playerExperience.AddExp(expAmount);
            gameObject.SetActive(false);
        }

        public void SetExpAmount(int amount)
        {
            expAmount = Mathf.Max(1, amount);
        }
    }
}
