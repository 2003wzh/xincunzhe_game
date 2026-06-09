using UnityEngine;

namespace XianxiaSurvivor.Enemies
{
    /// <summary>
    /// 用途：保存怪物的基础战斗属性，供移动、受伤、接触伤害和掉落逻辑读取。
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyStats : MonoBehaviour
    {
        [SerializeField] private int maxHp = 10;
        [SerializeField] private int currentHp = 10;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private int damage = 5;
        [SerializeField] private int expDrop = 1;

        public int MaxHp => maxHp;
        public int CurrentHp => currentHp;
        public float MoveSpeed => moveSpeed;
        public int Damage => damage;
        public int ExpDrop => expDrop;
        public bool IsAlive => currentHp > 0;

        private void OnValidate()
        {
            maxHp = Mathf.Max(1, maxHp);
            currentHp = Mathf.Clamp(currentHp, 0, maxHp);
            moveSpeed = Mathf.Max(0f, moveSpeed);
            damage = Mathf.Max(0, damage);
            expDrop = Mathf.Max(0, expDrop);
        }

        public void ResetHp()
        {
            currentHp = maxHp;
        }

        public void SetCurrentHp(int value)
        {
            currentHp = Mathf.Clamp(value, 0, maxHp);
        }

        public void TakeDamage(int amount)
        {
            int damageAmount = Mathf.Max(0, amount);

            if (damageAmount <= 0)
            {
                return;
            }

            SetCurrentHp(currentHp - damageAmount);
        }
    }
}
