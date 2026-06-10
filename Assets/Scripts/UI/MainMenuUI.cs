using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XianxiaSurvivor.UI
{
    /// <summary>
    /// 用途：控制主菜单第一版的标题显示、开始历练和退出游戏按钮。
    /// </summary>
    [DisallowMultipleComponent]
    public class MainMenuUI : MonoBehaviour
    {
        private const string BattleSceneName = "Battle";

        [Header("Text")]
        [SerializeField] private Text titleText;
        [SerializeField] private string gameTitle = "修仙幸存者";

        [Header("Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button quitButton;

        private bool areButtonsBound;
        private bool warnedMissingTitleText;
        private bool warnedMissingStartButton;
        private bool warnedMissingQuitButton;

        private void OnEnable()
        {
            RefreshTitle();
            BindButtons();
        }

        private void OnDisable()
        {
            UnbindButtons();
        }

        private void RefreshTitle()
        {
            if (titleText == null)
            {
                WarnOnce(ref warnedMissingTitleText, "MainMenuUI 缺少 TitleText 引用，无法显示游戏标题。");
                return;
            }

            titleText.text = string.IsNullOrEmpty(gameTitle) ? "修仙幸存者" : gameTitle;
        }

        private void BindButtons()
        {
            if (areButtonsBound)
            {
                return;
            }

            if (startButton != null)
            {
                startButton.onClick.AddListener(StartBattle);
            }
            else
            {
                WarnOnce(ref warnedMissingStartButton, "MainMenuUI 缺少 StartButton 引用，无法点击开始历练。");
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(QuitGame);
            }
            else
            {
                WarnOnce(ref warnedMissingQuitButton, "MainMenuUI 缺少 QuitButton 引用，无法点击退出游戏。");
            }

            areButtonsBound = true;
        }

        private void UnbindButtons()
        {
            if (!areButtonsBound)
            {
                return;
            }

            if (startButton != null)
            {
                startButton.onClick.RemoveListener(StartBattle);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(QuitGame);
            }

            areButtonsBound = false;
        }

        private void StartBattle()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(BattleSceneName);
        }

        private void QuitGame()
        {
            Time.timeScale = 1f;

#if UNITY_EDITOR
            Debug.Log("MainMenuUI 收到退出游戏请求：编辑器中停止 Play Mode。", this);
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
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
