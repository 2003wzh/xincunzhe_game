using UnityEngine;
using XianxiaSurvivor.Core;
using XianxiaSurvivor.Player;

namespace XianxiaSurvivor.Skills
{
    /// <summary>
    /// 用途：驱动挂在玩家身上的技能组件，第一版只用于飞剑术。
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerSkillController : MonoBehaviour
    {
        [SerializeField] private FlyingSwordSkill[] skills;
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
        }

        private void Update()
        {
            if (!CanDriveSkills())
            {
                return;
            }

            if (skills == null || skills.Length == 0)
            {
                return;
            }

            float deltaTime = Time.deltaTime;

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
