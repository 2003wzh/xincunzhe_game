using System.Collections.Generic;
using UnityEngine;
using XianxiaSurvivor.Core;
using XianxiaSurvivor.Skills;

namespace XianxiaSurvivor.UI
{
    /// <summary>
    /// 用途：显示升级三选一面板，暂停战斗，并在玩家完成选择后恢复游戏。
    /// </summary>
    [DisallowMultipleComponent]
    public class LevelUpPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private SkillUpgradeService skillUpgradeService;
        [SerializeField] private GameManager gameManager;

        [Header("Option Buttons")]
        [SerializeField] private LevelUpOptionButton[] optionButtons = new LevelUpOptionButton[3];

        private bool isPausedByPanel;

        private void Awake()
        {
            HidePanelRoot();
            ClearOptionButtons();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<SkillUpgradeOptionsReadyEvent>(OnSkillUpgradeOptionsReady);
            EventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<SkillUpgradeOptionsReadyEvent>(OnSkillUpgradeOptionsReady);
            EventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
            ClosePanel();
        }

        private void OnSkillUpgradeOptionsReady(SkillUpgradeOptionsReadyEvent eventData)
        {
            if (!CanShowUpgradePanel())
            {
                return;
            }

            if (skillUpgradeService == null || eventData.Player != skillUpgradeService.gameObject)
            {
                return;
            }

            ShowCurrentOptions();
        }

        private void OnGameStateChanged(GameStateChangedEvent eventData)
        {
            if (eventData.CurrentState == GameState.Failed || eventData.CurrentState == GameState.Victory)
            {
                ClosePanelWithoutResuming();
            }
        }

        private void ShowCurrentOptions()
        {
            if (!CanShowUpgradePanel())
            {
                return;
            }

            if (skillUpgradeService == null)
            {
                Debug.LogWarning("LevelUpPanel is missing SkillUpgradeService.", this);
                return;
            }

            IReadOnlyList<SkillUpgradeOption> upgradeOptions = skillUpgradeService.GetUpgradeOptions();
            if (upgradeOptions == null || upgradeOptions.Count == 0)
            {
                Debug.LogWarning("LevelUpPanel received a ready event, but there are no upgrade options to show.", this);
                return;
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            BindOptionButtons(upgradeOptions);
            PauseGameIfNeeded();
        }

        private void BindOptionButtons(IReadOnlyList<SkillUpgradeOption> upgradeOptions)
        {
            if (optionButtons == null)
            {
                return;
            }

            for (int i = 0; i < optionButtons.Length; i++)
            {
                LevelUpOptionButton optionButton = optionButtons[i];
                if (optionButton == null)
                {
                    continue;
                }

                if (i < upgradeOptions.Count)
                {
                    optionButton.Bind(upgradeOptions[i], OnOptionSelected);
                }
                else
                {
                    optionButton.Clear();
                }
            }
        }

        private void OnOptionSelected(SkillUpgradeOption option)
        {
            if (IsRunEnded())
            {
                ClosePanelWithoutResuming();
                return;
            }

            if (skillUpgradeService == null)
            {
                Debug.LogWarning("LevelUpPanel cannot apply an upgrade because SkillUpgradeService is missing.", this);
                return;
            }

            bool applied = skillUpgradeService.ApplyUpgradeOption(option);
            if (!applied)
            {
                Debug.LogWarning("Failed to apply the selected upgrade option.", this);
                return;
            }

            if (!skillUpgradeService.HasCurrentOptions)
            {
                ClosePanel();
            }
        }

        private void PauseGameIfNeeded()
        {
            if (IsRunEnded())
            {
                return;
            }

            if (isPausedByPanel)
            {
                return;
            }

            if (gameManager != null)
            {
                if (gameManager.IsGameplayRunning)
                {
                    gameManager.PauseGame();
                    isPausedByPanel = true;
                }

                return;
            }

            Time.timeScale = 0f;
            isPausedByPanel = true;
        }

        private void ClosePanel()
        {
            HidePanelRoot();
            ClearOptionButtons();
            ResumeGameIfNeeded();
        }

        private void ClosePanelWithoutResuming()
        {
            HidePanelRoot();
            ClearOptionButtons();
            isPausedByPanel = false;
        }

        private void HidePanelRoot()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void ClearOptionButtons()
        {
            if (optionButtons == null)
            {
                return;
            }

            for (int i = 0; i < optionButtons.Length; i++)
            {
                LevelUpOptionButton optionButton = optionButtons[i];
                if (optionButton != null)
                {
                    optionButton.Clear();
                }
            }
        }

        private void ResumeGameIfNeeded()
        {
            if (!isPausedByPanel)
            {
                return;
            }

            if (gameManager != null)
            {
                if (gameManager.CurrentState == GameState.Paused)
                {
                    gameManager.ResumeBattle();
                }
            }
            else
            {
                Time.timeScale = 1f;
            }

            isPausedByPanel = false;
        }

        private bool CanShowUpgradePanel()
        {
            if (IsRunEnded())
            {
                return false;
            }

            if (gameManager != null)
            {
                return gameManager.IsGameplayRunning || isPausedByPanel;
            }

            return Time.timeScale > 0f || isPausedByPanel;
        }

        private bool IsRunEnded()
        {
            return gameManager != null && gameManager.IsRunEnded;
        }
    }
}
