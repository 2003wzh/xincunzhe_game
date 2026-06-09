namespace XianxiaSurvivor.Combat
{
    /// <summary>
    /// 用途：表示对象可以受到伤害，玩家、怪物、Boss 后续都可以实现这个接口。
    /// </summary>
    public interface IDamageable
    {
        bool IsAlive { get; }

        void TakeDamage(DamageInfo damageInfo);
    }
}
