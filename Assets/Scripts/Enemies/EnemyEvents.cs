using UnityEngine;

namespace XianxiaSurvivor.Enemies
{
    /// <summary>
    /// 用途：表示怪物血量变化，后续 UI 或调试工具可以监听这个事件。
    /// </summary>
    public readonly struct EnemyHealthChangedEvent
    {
        public EnemyHealthChangedEvent(GameObject enemy, int currentHp, int maxHp, int delta)
        {
            Enemy = enemy;
            CurrentHp = currentHp;
            MaxHp = maxHp;
            Delta = delta;
        }

        public GameObject Enemy { get; }
        public int CurrentHp { get; }
        public int MaxHp { get; }
        public int Delta { get; }
    }

    /// <summary>
    /// 用途：表示一个怪物死亡，刷怪器、统计系统或后续通关逻辑可以监听这个事件。
    /// </summary>
    public readonly struct EnemyDiedEvent
    {
        public EnemyDiedEvent(GameObject enemy, bool isBoss, int expDrop, Vector3 position)
        {
            Enemy = enemy;
            IsBoss = isBoss;
            ExpDrop = expDrop;
            Position = position;
        }

        public GameObject Enemy { get; }
        public bool IsBoss { get; }
        public int ExpDrop { get; }
        public Vector3 Position { get; }
    }

    /// <summary>
    /// 用途：表示 Boss 死亡，后续 GameManager 可以监听这个事件并切换到通关状态。
    /// </summary>
    public readonly struct BossDiedEvent
    {
        public BossDiedEvent(GameObject boss)
        {
            Boss = boss;
        }

        public GameObject Boss { get; }
    }
}
