using System.Collections.Generic;
using UnityEngine;
using XianxiaSurvivor.Core;
using XianxiaSurvivor.Player;

namespace XianxiaSurvivor.Skills
{
    /// <summary>
    /// 用途：生成、缓存和应用玩家升级时的技能三选一候选项，第一版只服务于飞剑术。
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

        public bool ApplyUpgradeOption(SkillUpgradeOption option)
        {
            if (option == null || !option.IsValid)
            {
                LogDebug("升级选项为空或无效，无法应用。");
                return false;
            }

            if (currentOptions.Count == 0 || !currentOptions.Contains(option))
            {
                LogDebug("传入的升级选项不属于当前缓存选项，无法应用。");
                return false;
            }

            if (!TryResolveFlyingSwordSkill())
            {
                return false;
            }

            bool applied = ApplyToFlyingSword(option);

            if (!applied)
            {
                LogDebug("升级选项类型暂不支持，无法应用。");
                return false;
            }

            currentOptions.Clear();
            LogFlyingSwordValues($"已应用升级：{option.Title}");
            return true;
        }

        [ContextMenu("Debug Generate Upgrade Options")]
        private void DebugGenerateUpgradeOptions()
        {
            GenerateUpgradeOptions();
            LogCurrentOptions();
        }

        [ContextMenu("Debug Apply First Upgrade Option")]
        private void DebugApplyFirstUpgradeOption()
        {
            if (currentOptions.Count == 0)
            {
                Debug.LogWarning("当前没有缓存的升级选项，请先执行 Debug Generate Upgrade Options。", this);
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

            GenerateUpgradeOptions();
            LogDebug($"玩家升级，已生成 {currentOptions.Count} 个技能升级候选项。");
        }

        private void GenerateUpgradeOptions()
        {
            currentOptions.Clear();

            if (!TryResolveFlyingSwordSkill())
            {
                return;
            }

            currentOptions.Add(new SkillUpgradeOption(
                "强化飞剑伤害",
                $"飞剑伤害 +{damageIncrease:0.#}",
                SkillUpgradeOptionType.FlyingSwordDamage,
                damageIncrease));

            currentOptions.Add(new SkillUpgradeOption(
                "缩短飞剑冷却",
                $"飞剑冷却 -{cooldownReduction:0.##} 秒",
                SkillUpgradeOptionType.FlyingSwordCooldown,
                cooldownReduction));

            currentOptions.Add(new SkillUpgradeOption(
                "提升飞剑索敌范围",
                $"飞剑索敌范围 +{rangeIncrease:0.#}",
                SkillUpgradeOptionType.FlyingSwordRange,
                rangeIncrease));
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
                Debug.LogWarning("SkillUpgradeService 缺少 FlyingSwordSkill 引用，无法生成飞剑术升级选项。", this);
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
                Debug.Log("当前没有可用的技能升级候选项。", this);
                return;
            }

            for (int i = 0; i < currentOptions.Count; i++)
            {
                SkillUpgradeOption option = currentOptions[i];
                Debug.Log($"{i + 1}. {option.Title}：{option.Description}", this);
            }
        }

        private void LogFlyingSwordValues(string prefix)
        {
            if (!logDebugMessages || flyingSwordSkill == null)
            {
                return;
            }

            Debug.Log(
                $"{prefix}。当前飞剑数值：伤害 {flyingSwordSkill.CurrentDamage}，冷却 {flyingSwordSkill.CurrentCooldown:0.##}，范围 {flyingSwordSkill.CurrentRange:0.#}",
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
