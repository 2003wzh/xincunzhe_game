using UnityEngine;

namespace XianxiaSurvivor.Data
{
    /// <summary>
    /// 用途：保存技能的基础配置数据，第一版用于飞剑术，后续可复用到其他技能。
    /// </summary>
    [CreateAssetMenu(fileName = "FlyingSwordSkillData", menuName = "Xianxia Survivor/Skill Data")]
    public class SkillData : ScriptableObject
    {
        [Header("Basic")]
        [SerializeField] private string skillName = "飞剑术";

        [Header("Combat")]
        [SerializeField] private int damage = 10;
        [SerializeField] private float cooldown = 1f;
        [SerializeField] private float range = 8f;

        [Header("Projectile")]
        [SerializeField] private float projectileSpeed = 12f;
        [SerializeField] private float projectileMaxDistance = 12f;

        [Header("Area Effect")]
        [SerializeField] private float explosionRadius = 1.5f;

        public string SkillName => skillName;
        public int Damage => damage;
        public float Cooldown => cooldown;
        public float Range => range;
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileMaxDistance => projectileMaxDistance;
        public float ExplosionRadius => explosionRadius;

        private void OnValidate()
        {
            damage = Mathf.Max(0, damage);
            cooldown = Mathf.Max(0.05f, cooldown);
            range = Mathf.Max(0.1f, range);
            projectileSpeed = Mathf.Max(0.1f, projectileSpeed);
            projectileMaxDistance = Mathf.Max(0.1f, projectileMaxDistance);
            explosionRadius = Mathf.Max(0.1f, explosionRadius);
        }
    }
}
