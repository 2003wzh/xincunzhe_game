using UnityEngine;

namespace XianxiaSurvivor.Core
{
    /// <summary>
    /// 用途：描述游戏当前处于哪个基础状态。
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Battle,
        Paused,
        Failed,
        Victory
    }

    /// <summary>
    /// 用途：描述一次游戏状态变化，供 UI 或其他系统监听。
    /// </summary>
    public readonly struct GameStateChangedEvent
    {
        public GameStateChangedEvent(GameState previousState, GameState currentState)
        {
            PreviousState = previousState;
            CurrentState = currentState;
        }

        public GameState PreviousState { get; }
        public GameState CurrentState { get; }
    }

    /// <summary>
    /// 用途：管理游戏状态切换，只负责菜单、战斗、暂停、失败和通关状态。
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameState startState = GameState.MainMenu;

        public GameState CurrentState { get; private set; }

        private void Awake()
        {
            CurrentState = startState;
            ApplyPauseState(CurrentState);
        }

        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState)
            {
                return;
            }

            GameState previousState = CurrentState;
            CurrentState = newState;

            ApplyPauseState(CurrentState);
            EventBus.Raise(new GameStateChangedEvent(previousState, CurrentState));
        }

        public void StartBattle()
        {
            ChangeState(GameState.Battle);
        }

        public void PauseGame()
        {
            ChangeState(GameState.Paused);
        }

        public void ResumeBattle()
        {
            ChangeState(GameState.Battle);
        }

        public void FailRun()
        {
            ChangeState(GameState.Failed);
        }

        public void WinRun()
        {
            ChangeState(GameState.Victory);
        }

        public void ReturnToMainMenu()
        {
            ChangeState(GameState.MainMenu);
        }

        private static void ApplyPauseState(GameState state)
        {
            Time.timeScale = state == GameState.Paused ? 0f : 1f;
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
        }
    }
}
