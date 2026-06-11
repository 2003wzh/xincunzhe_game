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
    /// 用途：生成、缓存并应用玩家升级时的三选一技能候选项。
    /// </summary>
    [DisallowMultipleComponent]
    public class SkillUpgradeService : MonoBehaviour
    {
        [Header("Target Skill")]
        [SerializeField] private FlyingSwordSkill flyingSwordSkill;
        [SerializeField] private FireballSkill fireballSkill;

        [Header("Flying Sword Options")]
        [SerializeField] private float damageIncrease = 5f;
        [SerializeField] private float cooldownReduction = 0.15f;
        [SerializeField] private float rangeIncrease = 1.5f;

        [Header("Fireball Options")]
        [SerializeField] private float fireballDamageIncrease = 3f;
        [SerializeField] private float fireballCooldownReduction = 0.2f;
        [SerializeField] private float fireballExplosionRadiusIncrease = 0.3f;

        [Header("Debug")]
        [SerializeField] private bool logDebugMessages = true;

        private readonly List<SkillUpgradeOption> currentOptions = new List<SkillUpgradeOption>(3);
        private readonly Queue<List<SkillUpgradeOption>> pendingOptionSets = new Queue<List<SkillUpgradeOption>>();
        private readonly List<SkillUpgradeOption> upgradePool = new List<SkillUpgradeOption>(6);
        private bool isSubscribed;
        private bool warnedMissingFlyingSwordSkill;

        private void Awake()
        {
            TryResolveFlyingSwordSkill();
            TryResolveFireballSkill();
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
            fireballDamageIncrease = Mathf.Max(0f, fireballDamageIncrease);
            fireballCooldownReduction = Mathf.Max(0f, fireballCooldownReduction);
            fireballExplosionRadiusIncrease = Mathf.Max(0f, fireballExplosionRadiusIncrease);
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

            bool applied = ApplyUpgrade(option);

            if (!applied)
            {
                LogDebug("Unsupported upgrade option type.");
                return false;
            }

            currentOptions.Clear();
            PromoteNextPendingOptions();
            LogSkillValues($"Applied upgrade: {option.Title}");
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
            BuildUpgradePool();
            List<SkillUpgradeOption> generatedOptions = new List<SkillUpgradeOption>(3);

            while (generatedOptions.Count < 3 && upgradePool.Count > 0)
            {
                int randomIndex = Random.Range(0, upgradePool.Count);
                generatedOptions.Add(upgradePool[randomIndex]);
                upgradePool.RemoveAt(randomIndex);
            }

            return generatedOptions;
        }

        private void BuildUpgradePool()
        {
            upgradePool.Clear();

            if (TryResolveFlyingSwordSkill())
            {
                upgradePool.Add(new SkillUpgradeOption(
                    "强化飞剑伤害",
                    $"飞剑伤害 +{damageIncrease:0.#}",
                    SkillUpgradeOptionType.FlyingSwordDamage,
                    damageIncrease));

                upgradePool.Add(new SkillUpgradeOption(
                    "缩短飞剑冷却",
                    $"飞剑冷却 -{cooldownReduction:0.##} 秒",
                    SkillUpgradeOptionType.FlyingSwordCooldown,
                    cooldownReduction));

                upgradePool.Add(new SkillUpgradeOption(
                    "提升飞剑索敌范围",
                    $"飞剑索敌范围 +{rangeIncrease:0.#}",
                    SkillUpgradeOptionType.FlyingSwordRange,
                    rangeIncrease));
            }

            if (TryResolveFireballSkill())
            {
                upgradePool.Add(new SkillUpgradeOption(
                    "强化离火诀伤害",
                    $"离火诀伤害 +{fireballDamageIncrease:0.#}",
                    SkillUpgradeOptionType.FireballDamage,
                    fireballDamageIncrease));

                upgradePool.Add(new SkillUpgradeOption(
                    "缩短离火诀冷却",
                    $"离火诀冷却 -{fireballCooldownReduction:0.##} 秒",
                    SkillUpgradeOptionType.FireballCooldown,
                    fireballCooldownReduction));

                upgradePool.Add(new SkillUpgradeOption(
                    "扩大离火诀爆炸范围",
                    $"离火诀爆炸范围 +{fireballExplosionRadiusIncrease:0.#}",
                    SkillUpgradeOptionType.FireballExplosionRadius,
                    fireballExplosionRadiusIncrease));
            }
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

        private bool ApplyUpgrade(SkillUpgradeOption option)
        {
            switch (option.OptionType)
            {
                case SkillUpgradeOptionType.FlyingSwordDamage:
                    if (!TryResolveFlyingSwordSkill())
                    {
                        return false;
                    }

                    flyingSwordSkill.AddDamage(option.ValueChange);
                    return true;

                case SkillUpgradeOptionType.FlyingSwordCooldown:
                    if (!TryResolveFlyingSwordSkill())
                    {
                        return false;
                    }

                    flyingSwordSkill.ReduceCooldown(option.ValueChange);
                    return true;

                case SkillUpgradeOptionType.FlyingSwordRange:
                    if (!TryResolveFlyingSwordSkill())
                    {
                        return false;
                    }

                    flyingSwordSkill.AddRange(option.ValueChange);
                    return true;

                case SkillUpgradeOptionType.FireballDamage:
                    if (!TryResolveFireballSkill())
                    {
                        return false;
                    }

                    fireballSkill.AddDamage(option.ValueChange);
                    return true;

                case SkillUpgradeOptionType.FireballCooldown:
                    if (!TryResolveFireballSkill())
                    {
                        return false;
                    }

                    fireballSkill.ReduceCooldown(option.ValueChange);
                    return true;

                case SkillUpgradeOptionType.FireballExplosionRadius:
                    if (!TryResolveFireballSkill())
                    {
                        return false;
                    }

                    fireballSkill.AddExplosionRadius(option.ValueChange);
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

        private bool TryResolveFireballSkill()
        {
            if (fireballSkill != null)
            {
                return true;
            }

            fireballSkill = GetComponent<FireballSkill>();
            return fireballSkill != null;
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

        private void LogSkillValues(string prefix)
        {
            if (!logDebugMessages)
            {
                return;
            }

            if (flyingSwordSkill != null)
            {
                Debug.Log(
                    $"{prefix}. Current flying sword values: damage {flyingSwordSkill.CurrentDamage}, cooldown {flyingSwordSkill.CurrentCooldown:0.##}, range {flyingSwordSkill.CurrentRange:0.#}.",
                    this);
            }

            if (fireballSkill != null)
            {
                Debug.Log(
                    $"{prefix}. Current fireball values: damage {fireballSkill.CurrentDamage}, cooldown {fireballSkill.CurrentCooldown:0.##}, explosion radius {fireballSkill.CurrentExplosionRadius:0.#}.",
                    this);
            }
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
