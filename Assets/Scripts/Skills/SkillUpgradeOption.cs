namespace XianxiaSurvivor.Skills
{
    /// <summary>
    /// 用途：表示第一版技能升级候选项的类型。
    /// </summary>
    public enum SkillUpgradeOptionType
    {
        None = 0,
        FlyingSwordDamage = 1,
        FlyingSwordCooldown = 2,
        FlyingSwordRange = 3,
        FireballDamage = 4,
        FireballCooldown = 5,
        FireballExplosionRadius = 6
    }

    /// <summary>
    /// 用途：描述一次升级三选一中的单个候选项，供后续 UI 展示和回传选择结果。
    /// </summary>
    public class SkillUpgradeOption
    {
        public SkillUpgradeOption(
            string title,
            string description,
            SkillUpgradeOptionType optionType,
            float valueChange)
        {
            Title = title ?? string.Empty;
            Description = description ?? string.Empty;
            OptionType = optionType;
            ValueChange = valueChange;
        }

        public string Title { get; }
        public string Description { get; }
        public SkillUpgradeOptionType OptionType { get; }
        public float ValueChange { get; }
        public bool IsValid => OptionType != SkillUpgradeOptionType.None
            && !string.IsNullOrEmpty(Title)
            && ValueChange != 0f;
    }
}
