using UnityEngine;

namespace XianxiaSurvivor.Player
{
    /// <summary>
    /// 用途：保存玩家第一版基础属性，供移动、血量和经验组件共用。
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerStats : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private int maxHp = 100;
        [SerializeField] private int currentHp = 100;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Experience")]
        [SerializeField] private int level = 1;
        [SerializeField] private int currentExp = 0;
        [SerializeField] private int expToNextLevel = 10;

        public int MaxHp => maxHp;
        public int CurrentHp => currentHp;
        public float MoveSpeed => moveSpeed;
        public int Level => level;
        public int CurrentExp => currentExp;
        public int ExpToNextLevel => expToNextLevel;
        public bool IsAlive => currentHp > 0;

        private void Awake()
        {
            maxHp = Mathf.Max(1, maxHp);
            currentHp = Mathf.Clamp(currentHp, 0, maxHp);
            moveSpeed = Mathf.Max(0f, moveSpeed);
            level = Mathf.Max(1, level);
            currentExp = Mathf.Max(0, currentExp);
            expToNextLevel = Mathf.Max(1, expToNextLevel);
        }

        public void SetCurrentHp(int value)
        {
            currentHp = Mathf.Clamp(value, 0, maxHp);
        }

        public void AddExp(int amount)
        {
            currentExp = Mathf.Max(0, currentExp + Mathf.Max(0, amount));
        }

        public void LevelUp(int nextExpToNextLevel)
        {
            currentExp = Mathf.Max(0, currentExp - expToNextLevel);
            level += 1;
            expToNextLevel = Mathf.Max(1, nextExpToNextLevel);
        }
    }
}
