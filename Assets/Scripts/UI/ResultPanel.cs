using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using XianxiaSurvivor.Core;
using XianxiaSurvivor.Enemies;
using XianxiaSurvivor.Player;

namespace XianxiaSurvivor.UI
{
    /// <summary>
    /// 用途：显示单局失败或通关结算面板，并提供重新开始和返回主菜单按钮。
    /// </summary>
    [DisallowMultipleComponent]
    public class ResultPanel : MonoBehaviour
    {
        private const string FailedTitle = "历练失败";
        private const string VictoryTitle = "渡劫成功";
        private const string DefaultBattleSceneName = "Battle";
        private const string DefaultMainMenuSceneName = "MainMenu";

        [Header("Data Sources")]
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private RunTimer runTimer;
        [SerializeField] private GameManager gameManager;

        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;

        [Header("Text")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text timeText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text expText;

        [Header("Buttons")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        private bool isSubscribed;
        private bool areButtonsBound;
        private bool isShowing;
        private bool warnedMissingPanelRoot;
        private bool warnedMissingPlayerStats;
        private bool warnedMissingRunTimer;
        private bool warnedMissingTitleText;
        private bool warnedMissingTimeText;
        private bool warnedMissingLevelText;
        private bool warnedMissingExpText;
        private bool warnedMissingRestartButton;
        private bool warnedMissingMainMenuButton;

        private void Awake()
        {
            HidePanelRoot();
        }

        private void OnEnable()
        {
            SubscribeEvents();
            BindButtons();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
            UnbindButtons();

            if (isShowing)
            {
                Time.timeScale = 1f;
            }
        }

        private void SubscribeEvents()
        {
            if (isSubscribed)
            {
                return;
            }

            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Subscribe<BossDiedEvent>(OnBossDied);
            isSubscribed = true;
        }

        private void UnsubscribeEvents()
        {
            if (!isSubscribed)
            {
                return;
            }

            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Unsubscribe<BossDiedEvent>(OnBossDied);
            isSubscribed = false;
        }

        private void BindButtons()
        {
            if (areButtonsBound)
            {
                return;
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(RestartBattle);
            }
            else
            {
                WarnOnce(ref warnedMissingRestartButton, "ResultPanel 缺少 RestartButton 引用，无法点击重新开始。");
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            }
            else
            {
                WarnOnce(ref warnedMissingMainMenuButton, "ResultPanel 缺少 MainMenuButton 引用，无法点击返回主菜单。");
            }

            areButtonsBound = true;
        }

        private void UnbindButtons()
        {
            if (!areButtonsBound)
            {
                return;
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(RestartBattle);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
            }

            areButtonsBound = false;
        }

        private void OnPlayerDied(PlayerDiedEvent eventData)
        {
            if (isShowing)
            {
                return;
            }

            if (playerStats != null && eventData.Player != playerStats.gameObject)
            {
                return;
            }

            if (gameManager != null)
            {
                gameManager.FailRun();
            }

            ShowResult(FailedTitle);
        }

        private void OnBossDied(BossDiedEvent eventData)
        {
            if (isShowing)
            {
                return;
            }

            if (gameManager != null)
            {
                gameManager.WinRun();
            }

            ShowResult(VictoryTitle);
        }

        private void ShowResult(string title)
        {
            if (isShowing)
            {
                return;
            }

            isShowing = true;

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
            else
            {
                WarnOnce(ref warnedMissingPanelRoot, "ResultPanel 缺少 PanelRoot 引用，无法显示结算面板。");
            }

            SetText(titleText, title, ref warnedMissingTitleText, "TitleText");
            SetText(timeText, $"本局存活时间：{GetSurvivalTimeText()}", ref warnedMissingTimeText, "TimeText");
            SetText(levelText, $"最终等级：{GetFinalLevelText()}", ref warnedMissingLevelText, "LevelText");
            SetText(expText, $"当前灵气经验：{GetCurrentExpText()}", ref warnedMissingExpText, "ExpText");

            Time.timeScale = 0f;
        }

        private void RestartBattle()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(DefaultBattleSceneName);
        }

        private void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(DefaultMainMenuSceneName);
        }

        private string GetSurvivalTimeText()
        {
            if (runTimer == null)
            {
                WarnOnce(ref warnedMissingRunTimer, "ResultPanel 缺少 RunTimer 引用，无法显示本局存活时间。");
                return "00:00";
            }

            return runTimer.GetFormattedTime();
        }

        private string GetFinalLevelText()
        {
            if (playerStats == null)
            {
                WarnOnce(ref warnedMissingPlayerStats, "ResultPanel 缺少 PlayerStats 引用，无法显示最终等级和当前灵气经验。");
                return "-";
            }

            return Mathf.Max(1, playerStats.Level).ToString();
        }

        private string GetCurrentExpText()
        {
            if (playerStats == null)
            {
                WarnOnce(ref warnedMissingPlayerStats, "ResultPanel 缺少 PlayerStats 引用，无法显示最终等级和当前灵气经验。");
                return "-";
            }

            return Mathf.Max(0, playerStats.CurrentExp).ToString();
        }

        private void HidePanelRoot()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void SetText(Text targetText, string value, ref bool warned, string fieldName)
        {
            if (targetText == null)
            {
                WarnOnce(ref warned, $"ResultPanel 缺少 {fieldName} 引用。");
                return;
            }

            targetText.text = value;
        }

        private void WarnOnce(ref bool warned, string message)
        {
            if (warned)
            {
                return;
            }

            warned = true;
            Debug.LogWarning(message, this);
        }
    }
}
