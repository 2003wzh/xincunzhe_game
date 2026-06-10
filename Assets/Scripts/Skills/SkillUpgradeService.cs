using System.Collections.Generic;
using UnityEngine;
using XianxiaSurvivor.Core;
using XianxiaSurvivor.Player;

namespace XianxiaSurvivor.Skills
{
    /// <summary>
    /// 通知 UI：当前有一组可展示的升级选项已经准备好。
    /// </summary>
    public readonly struct SkillUpgradeOptionsReadyEvent
    {
        public SkillUpgradeOptionsReadyEvent(GameObject player)
        {
            Player = player;
        }

        public GameObject Player { get; }
    }

    /// <summary>
    /// 用途：生成、缓存并应用玩家升级时的三选一技能候选项，第一版只服务于飞剑术。
    /// </summary>
    [DisallowMultipleComponent]
    public class SkillUpgradeService : MonoBehaviour
    {
        [Header("Target Skill")]
        [SerializeField] private FlyingSwordSkill flyingSwordSkill;

        [Header("Flying Sword Options")]
        [SerializeField] private float damageIncrease = 5f;
        [SerializeField] private float cooldownReduction = 0.15f;
        [SerializeField] private float rangeIncrease = 1.5f;

        [Header("Debug")]
        [SerializeField] private bool logDebugMessages = true;

        private readonly List<SkillUpgradeOption> currentOptions = new List<SkillUpgradeOption>(3);
        private readonly Queue<List<SkillUpgradeOption>> pendingOptionSets = new Queue<List<SkillUpgradeOption>>();
        private bool isSubscribed;
        private bool warnedMissingFlyingSwordSkill;

        private void Awake()
        {
            TryResolveFlyingSwordSkill();
        }

        private void OnEnable()
        {
            if (isSubscribed)
            {
                return;
            }

            EventBus.Subscribe<PlayerLevelUpEvent>(OnPlayerLevelUp);
            isSubscribed = true;
        }

        private void OnDisable()
        {
            if (!isSubscribed)
            {
                return;
            }

            EventBus.Unsubscribe<PlayerLevelUpEvent>(OnPlayerLevelUp);
            isSubscribed = false;
        }

        private void OnValidate()
        {
            damageIncrease = Mathf.Max(0f, damageIncrease);
            cooldownReduction = Mathf.Max(0f, cooldownReduction);
            rangeIncrease = Mathf.Max(0f, rangeIncrease);
        }

        public IReadOnlyList<SkillUpgradeOption> GetUpgradeOptions()
        {
            return currentOptions;
        }

        public bool HasCurrentOptions => currentOptions.Count > 0;

        public bool ApplyUpgradeOption(SkillUpgradeOption option)
        {
            if (option == null || !option.IsValid)
            {
                LogDebug("Upgrade option is null or invalid.");
                return false;
            }

            if (currentOptions.Count == 0 || !currentOptions.Contains(option))
            {
                LogDebug("The option does not belong to the current active set.");
                return false;
            }

            if (!TryResolveFlyingSwordSkill())
            {
                return false;
            }

            bool applied = ApplyToFlyingSword(option);

            if (!applied)
            {
                LogDebug("Unsupported upgrade option type.");
                return false;
            }

            currentOptions.Clear();
            PromoteNextPendingOptions();
            LogFlyingSwordValues($"Applied upgrade: {option.Title}");
            return true;
        }

        [ContextMenu("Debug Generate Upgrade Options")]
        private void DebugGenerateUpgradeOptions()
        {
            pendingOptionSets.Clear();
            SetCurrentOptions(BuildUpgradeOptions());
            RaiseOptionsReadyEvent();
            LogCurrentOptions();
        }

        [ContextMenu("Debug Apply First Upgrade Option")]
        private void DebugApplyFirstUpgradeOption()
        {
            if (currentOptions.Count == 0)
            {
                Debug.LogWarning("No active upgrade option set exists. Run Debug Generate Upgrade Options first.", this);
                return;
            }

            ApplyUpgradeOption(currentOptions[0]);
        }

        private void OnPlayerLevelUp(PlayerLevelUpEvent eventData)
        {
            if (eventData.Player != gameObject)
            {
                return;
            }

            List<SkillUpgradeOption> generatedOptions = BuildUpgradeOptions();
            if (generatedOptions.Count == 0)
            {
                return;
            }

            if (!HasCurrentOptions)
            {
                SetCurrentOptions(generatedOptions);
                RaiseOptionsReadyEvent();
            }
            else
            {
                pendingOptionSets.Enqueue(generatedOptions);
            }

            LogDebug($"Player leveled up. Active options: {currentOptions.Count}, pending sets: {pendingOptionSets.Count}.");
        }

        private List<SkillUpgradeOption> BuildUpgradeOptions()
        {
            List<SkillUpgradeOption> generatedOptions = new List<SkillUpgradeOption>(3);

            if (!TryResolveFlyingSwordSkill())
            {
                return generatedOptions;
            }

            generatedOptions.Add(new SkillUpgradeOption(
                "强化飞剑伤害",
                $"飞剑伤害 +{damageIncrease:0.#}",
                SkillUpgradeOptionType.FlyingSwordDamage,
                damageIncrease));

            generatedOptions.Add(new SkillUpgradeOption(
                "缩短飞剑冷却",
                $"飞剑冷却 -{cooldownReduction:0.##} 秒",
                SkillUpgradeOptionType.FlyingSwordCooldown,
                cooldownReduction));

            generatedOptions.Add(new SkillUpgradeOption(
                "提升飞剑索敌范围",
                $"飞剑索敌范围 +{rangeIncrease:0.#}",
                SkillUpgradeOptionType.FlyingSwordRange,
                rangeIncrease));

            return generatedOptions;
        }

        private void SetCurrentOptions(List<SkillUpgradeOption> generatedOptions)
        {
            currentOptions.Clear();
            if (generatedOptions == null || generatedOptions.Count == 0)
            {
                return;
            }

            currentOptions.AddRange(generatedOptions);
        }

        private void PromoteNextPendingOptions()
        {
            if (pendingOptionSets.Count == 0)
            {
                return;
            }

            SetCurrentOptions(pendingOptionSets.Dequeue());
            RaiseOptionsReadyEvent();
        }

        private void RaiseOptionsReadyEvent()
        {
            EventBus.Raise(new SkillUpgradeOptionsReadyEvent(gameObject));
        }

        private bool ApplyToFlyingSword(SkillUpgradeOption option)
        {
            switch (option.OptionType)
            {
                case SkillUpgradeOptionType.FlyingSwordDamage:
                    flyingSwordSkill.AddDamage(option.ValueChange);
                    return true;

                case SkillUpgradeOptionType.FlyingSwordCooldown:
                    flyingSwordSkill.ReduceCooldown(option.ValueChange);
                    return true;

                case SkillUpgradeOptionType.FlyingSwordRange:
                    flyingSwordSkill.AddRange(option.ValueChange);
                    return true;

                default:
                    return false;
            }
        }

        private bool TryResolveFlyingSwordSkill()
        {
            if (flyingSwordSkill != null)
            {
                return true;
            }

            flyingSwordSkill = GetComponent<FlyingSwordSkill>();

            if (flyingSwordSkill != null)
            {
                return true;
            }

            if (!warnedMissingFlyingSwordSkill)
            {
                warnedMissingFlyingSwordSkill = true;
                Debug.LogWarning("SkillUpgradeService is missing FlyingSwordSkill, so upgrade options cannot be generated.", this);
            }

            return false;
        }

        private void LogCurrentOptions()
        {
            if (!logDebugMessages)
            {
                return;
            }

            if (currentOptions.Count == 0)
            {
                Debug.Log("No active upgrade options are cached.", this);
                return;
            }

            for (int i = 0; i < currentOptions.Count; i++)
            {
                SkillUpgradeOption option = currentOptions[i];
                Debug.Log($"{i + 1}. {option.Title}: {option.Description}", this);
            }
        }

        private void LogFlyingSwordValues(string prefix)
        {
            if (!logDebugMessages || flyingSwordSkill == null)
            {
                return;
            }

            Debug.Log(
                $"{prefix}. Current flying sword values: damage {flyingSwordSkill.CurrentDamage}, cooldown {flyingSwordSkill.CurrentCooldown:0.##}, range {flyingSwordSkill.CurrentRange:0.#}.",
                this);
        }

        private void LogDebug(string message)
        {
            if (!logDebugMessages)
            {
                return;
            }

            Debug.Log(message, this);
        }
    }
}
