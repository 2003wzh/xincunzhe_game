using UnityEngine;

namespace XianxiaSurvivor.Player
{
    /// <summary>
    /// 用途：表示玩家血量发生变化，供 UI 或其他系统监听。
    /// </summary>
    public readonly struct PlayerHealthChangedEvent
    {
        public PlayerHealthChangedEvent(GameObject player, int currentHp, int maxHp, int delta)
        {
            Player = player;
            CurrentHp = currentHp;
            MaxHp = maxHp;
            Delta = delta;
        }

        public GameObject Player { get; }
        public int CurrentHp { get; }
        public int MaxHp { get; }
        public int Delta { get; }
    }

    /// <summary>
    /// 用途：表示玩家灵气经验发生变化，供 UI 或其他系统监听。
    /// </summary>
    public readonly struct PlayerExpChangedEvent
    {
        public PlayerExpChangedEvent(GameObject player, int currentExp, int expToNextLevel, int addedExp)
        {
            Player = player;
            CurrentExp = currentExp;
            ExpToNextLevel = expToNextLevel;
            AddedExp = addedExp;
        }

        public GameObject Player { get; }
        public int CurrentExp { get; }
        public int ExpToNextLevel { get; }
        public int AddedExp { get; }
    }

    /// <summary>
    /// 用途：表示玩家升级，后续三选一功法 UI 会监听这个事件。
    /// </summary>
    public readonly struct PlayerLevelUpEvent
    {
        public PlayerLevelUpEvent(GameObject player, int previousLevel, int currentLevel)
        {
            Player = player;
            PreviousLevel = previousLevel;
            CurrentLevel = currentLevel;
        }

        public GameObject Player { get; }
        public int PreviousLevel { get; }
        public int CurrentLevel { get; }
    }

    /// <summary>
    /// 用途：表示玩家死亡，后续 GameManager 会监听这个事件并切换失败状态。
    /// </summary>
    public readonly struct PlayerDiedEvent
    {
        public PlayerDiedEvent(GameObject player)
        {
            Player = player;
        }

        public GameObject Player { get; }
    }
}
