using UnityEngine;
using XianxiaSurvivor.Core;

namespace XianxiaSurvivor.Player
{
    /// <summary>
    /// 用途：处理玩家吸收灵气、经验增长和升级事件。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerExperience : MonoBehaviour
    {
        [SerializeField] private float expGrowthMultiplier = 1.25f;
        [SerializeField] private int extraExpPerLevel = 5;

        private PlayerStats stats;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
            expGrowthMultiplier = Mathf.Max(1f, expGrowthMultiplier);
            extraExpPerLevel = Mathf.Max(0, extraExpPerLevel);
        }

        public void AddExp(int amount)
        {
            if (stats == null || !stats.IsAlive || amount <= 0)
            {
                return;
            }

            stats.AddExp(amount);
            EventBus.Raise(new PlayerExpChangedEvent(gameObject, stats.CurrentExp, stats.ExpToNextLevel, amount));

            while (stats.CurrentExp >= stats.ExpToNextLevel)
            {
                LevelUpOnce();
            }
        }

        private void LevelUpOnce()
        {
            int previousLevel = stats.Level;
            int nextExpToNextLevel = CalculateNextExpToNextLevel();

            stats.LevelUp(nextExpToNextLevel);

            EventBus.Raise(new PlayerLevelUpEvent(gameObject, previousLevel, stats.Level));
            EventBus.Raise(new PlayerExpChangedEvent(gameObject, stats.CurrentExp, stats.ExpToNextLevel, 0));
        }

        private int CalculateNextExpToNextLevel()
        {
            float nextExp = stats.ExpToNextLevel * expGrowthMultiplier + extraExpPerLevel;
            return Mathf.Max(1, Mathf.CeilToInt(nextExp));
        }
    }
}
