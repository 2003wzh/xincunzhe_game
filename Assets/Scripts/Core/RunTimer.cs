using UnityEngine;

namespace XianxiaSurvivor.Core
{
    /// <summary>
    /// 用途：记录一局战斗经过的时间，只负责计时，不负责刷怪或 Boss 逻辑。
    /// </summary>
    public class RunTimer : MonoBehaviour
    {
        [SerializeField] private bool startOnAwake;

        public float ElapsedSeconds { get; private set; }
        public bool IsRunning { get; private set; }

        private void Awake()
        {
            ResetTimer();

            if (startOnAwake)
            {
                StartTimer();
            }
        }

        private void Update()
        {
            if (!IsRunning)
            {
                return;
            }

            ElapsedSeconds += Time.deltaTime;
        }

        public void StartTimer()
        {
            IsRunning = true;
        }

        public void PauseTimer()
        {
            IsRunning = false;
        }

        public void ResetTimer()
        {
            ElapsedSeconds = 0f;
            IsRunning = false;
        }

        public string GetFormattedTime()
        {
            int totalSeconds = Mathf.FloorToInt(ElapsedSeconds);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}
