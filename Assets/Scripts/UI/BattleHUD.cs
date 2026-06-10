using UnityEngine;
using UnityEngine.UI;
using XianxiaSurvivor.Core;
using XianxiaSurvivor.Player;

namespace XianxiaSurvivor.UI
{
    /// <summary>
    /// 用途：显示战斗中的玩家血量、等级、灵气经验和局内时间，只负责 UI 展示。
    /// </summary>
    [DisallowMultipleComponent]
    public class BattleHUD : MonoBehaviour
    {
        [Header("Data Sources")]
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private RunTimer runTimer;

        [Header("Text")]
        [SerializeField] private Text healthText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text expText;
        [SerializeField] private Text timeText;

        [Header("Optional Slider")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Slider expSlider;

        private bool isSubscribed;
        private int lastDisplayedTimeSeconds = -1;
        private bool warnedMissingPlayerStats;
        private bool warnedMissingRunTimer;
        private bool warnedMissingHealthText;
        private bool warnedMissingLevelText;
        private bool warnedMissingExpText;
        private bool warnedMissingTimeText;

        private void OnEnable()
        {
            SubscribeEvents();
            RefreshAll();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void Update()
        {
            RefreshTime(false);
        }

        private void SubscribeEvents()
        {
            if (isSubscribed)
            {
                return;
            }

            EventBus.Subscribe<PlayerHealthChangedEvent>(OnPlayerHealthChanged);
            EventBus.Subscribe<PlayerExpChangedEvent>(OnPlayerExpChanged);
            EventBus.Subscribe<PlayerLevelUpEvent>(OnPlayerLevelUp);
            isSubscribed = true;
        }

        private void UnsubscribeEvents()
        {
            if (!isSubscribed)
            {
                return;
            }

            EventBus.Unsubscribe<PlayerHealthChangedEvent>(OnPlayerHealthChanged);
            EventBus.Unsubscribe<PlayerExpChangedEvent>(OnPlayerExpChanged);
            EventBus.Unsubscribe<PlayerLevelUpEvent>(OnPlayerLevelUp);
            isSubscribed = false;
        }

        private void RefreshAll()
        {
            if (playerStats == null)
            {
                WarnOnce(ref warnedMissingPlayerStats, "BattleHUD 缺少 PlayerStats 引用，无法显示玩家血量、等级和灵气经验。");
            }
            else
            {
                RefreshHealth(playerStats.CurrentHp, playerStats.MaxHp);
                RefreshLevel(playerStats.Level);
                RefreshExp(playerStats.CurrentExp, playerStats.ExpToNextLevel);
            }

            RefreshTime(true);
        }

        private void OnPlayerHealthChanged(PlayerHealthChangedEvent eventData)
        {
            if (!IsTargetPlayer(eventData.Player))
            {
                return;
            }

            RefreshHealth(eventData.CurrentHp, eventData.MaxHp);
        }

        private void OnPlayerExpChanged(PlayerExpChangedEvent eventData)
        {
            if (!IsTargetPlayer(eventData.Player))
            {
                return;
            }

            RefreshExp(eventData.CurrentExp, eventData.ExpToNextLevel);
        }

        private void OnPlayerLevelUp(PlayerLevelUpEvent eventData)
        {
            if (!IsTargetPlayer(eventData.Player))
            {
                return;
            }

            RefreshLevel(eventData.CurrentLevel);

            if (playerStats != null)
            {
                RefreshExp(playerStats.CurrentExp, playerStats.ExpToNextLevel);
            }
        }

        private bool IsTargetPlayer(GameObject player)
        {
            return playerStats != null && player == playerStats.gameObject;
        }

        private void RefreshHealth(int currentHp, int maxHp)
        {
            int safeMaxHp = Mathf.Max(1, maxHp);
            int safeCurrentHp = Mathf.Clamp(currentHp, 0, safeMaxHp);

            SetText(healthText, $"血量：{safeCurrentHp}/{safeMaxHp}", ref warnedMissingHealthText, "HealthText");

            if (healthSlider != null)
            {
                healthSlider.minValue = 0f;
                healthSlider.maxValue = safeMaxHp;
                healthSlider.value = safeCurrentHp;
            }
        }

        private void RefreshLevel(int level)
        {
            SetText(levelText, $"等级：{Mathf.Max(1, level)}", ref warnedMissingLevelText, "LevelText");
        }

        private void RefreshExp(int currentExp, int expToNextLevel)
        {
            int safeExpToNextLevel = Mathf.Max(1, expToNextLevel);
            int safeCurrentExp = Mathf.Clamp(currentExp, 0, safeExpToNextLevel);

            SetText(expText, $"灵气：{safeCurrentExp}/{safeExpToNextLevel}", ref warnedMissingExpText, "ExpText");

            if (expSlider != null)
            {
                expSlider.minValue = 0f;
                expSlider.maxValue = safeExpToNextLevel;
                expSlider.value = safeCurrentExp;
            }
        }

        private void RefreshTime(bool forceRefresh)
        {
            if (runTimer == null)
            {
                WarnOnce(ref warnedMissingRunTimer, "BattleHUD 缺少 RunTimer 引用，无法显示局内时间。");
                SetText(timeText, "时间：00:00", ref warnedMissingTimeText, "TimeText");
                return;
            }

            int totalSeconds = Mathf.FloorToInt(runTimer.ElapsedSeconds);
            if (!forceRefresh && totalSeconds == lastDisplayedTimeSeconds)
            {
                return;
            }

            lastDisplayedTimeSeconds = totalSeconds;
            SetText(timeText, $"时间：{runTimer.GetFormattedTime()}", ref warnedMissingTimeText, "TimeText");
        }

        private void SetText(Text targetText, string value, ref bool warned, string fieldName)
        {
            if (targetText == null)
            {
                WarnOnce(ref warned, $"BattleHUD 缺少 {fieldName} 引用。");
                return;
            }

            targetText.text = value;
        }

        private void WarnOnce(ref bool warned, string message)
        {
            if (warned)
            {
                return;
            }

            warned = true;
            Debug.LogWarning(message, this);
        }
    }
}
