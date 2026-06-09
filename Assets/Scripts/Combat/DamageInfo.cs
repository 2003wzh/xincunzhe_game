using UnityEngine;

namespace XianxiaSurvivor.Combat
{
    /// <summary>
    /// 用途：描述一次伤害的基础数据，后续可扩展暴击、元素、击退等信息。
    /// </summary>
    public readonly struct DamageInfo
    {
        public DamageInfo(int amount)
            : this(amount, null, Vector2.zero)
        {
        }

        public DamageInfo(int amount, GameObject source)
            : this(amount, source, Vector2.zero)
        {
        }

        public DamageInfo(int amount, GameObject source, Vector2 hitPoint)
        {
            Amount = Mathf.Max(0, amount);
            Source = source;
            HitPoint = hitPoint;
        }

        public int Amount { get; }
        public GameObject Source { get; }
        public Vector2 HitPoint { get; }
    }
}
