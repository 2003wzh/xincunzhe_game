using UnityEngine;
using XianxiaSurvivor.Core;
using XianxiaSurvivor.Player;

namespace XianxiaSurvivor.Skills
{
    /// <summary>
    /// 用途：表示可以由 PlayerSkillController 按帧驱动的玩家技能。
    /// </summary>
    public interface IPlayerSkill
    {
        void Tick(float deltaTime);
    }

    /// <summary>
    /// 用途：驱动挂在玩家身上的技能组件，保留飞剑术旧列表，并支持额外的通用技能组件。
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerSkillController : MonoBehaviour
    {
        [SerializeField] private FlyingSwordSkill[] skills;
        [SerializeField] private MonoBehaviour[] additionalSkills;
        [SerializeField] private GameManager gameManager;

        private PlayerStats playerStats;

        private void Awake()
        {
            playerStats = GetComponent<PlayerStats>();

            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }

            if (skills == null || skills.Length == 0)
            {
                skills = GetComponents<FlyingSwordSkill>();
            }

            if (additionalSkills == null || additionalSkills.Length == 0)
            {
                additionalSkills = GetComponents<MonoBehaviour>();
            }
        }

        private void Update()
        {
            if (!CanDriveSkills())
            {
                return;
            }

            float deltaTime = Time.deltaTime;

            TickFlyingSwordSkills(deltaTime);
            TickAdditionalSkills(deltaTime);
        }

        private void TickFlyingSwordSkills(float deltaTime)
        {
            if (skills == null || skills.Length == 0)
            {
                return;
            }

            for (int i = 0; i < skills.Length; i++)
            {
                FlyingSwordSkill skill = skills[i];

                if (skill == null || !skill.isActiveAndEnabled)
                {
                    continue;
                }

                skill.Tick(deltaTime);
            }
        }

        private void TickAdditionalSkills(float deltaTime)
        {
            if (additionalSkills == null || additionalSkills.Length == 0)
            {
                return;
            }

            for (int i = 0; i < additionalSkills.Length; i++)
            {
                MonoBehaviour skillBehaviour = additionalSkills[i];

                if (skillBehaviour == null || skillBehaviour == this || !skillBehaviour.isActiveAndEnabled)
                {
                    continue;
                }

                IPlayerSkill playerSkill = skillBehaviour as IPlayerSkill;

                if (playerSkill == null)
                {
                    continue;
                }

                playerSkill.Tick(deltaTime);
            }
        }

        private bool CanDriveSkills()
        {
            if (Time.timeScale <= 0f)
            {
                return false;
            }

            if (playerStats != null && !playerStats.IsAlive)
            {
                return false;
            }

            if (gameManager != null && gameManager.IsRunEnded)
            {
                return false;
            }

            return true;
        }
    }
}
