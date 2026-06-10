using System;
using UnityEngine;
using UnityEngine.UI;
using XianxiaSurvivor.Skills;

namespace XianxiaSurvivor.UI
{
    /// <summary>
    /// 用途：显示单个升级候选项的标题和描述，并把点击结果回传给 LevelUpPanel。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public class LevelUpOptionButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;

        private SkillUpgradeOption boundOption;
        private Action<SkillUpgradeOption> clickCallback;

        private void Awake()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (button != null)
            {
                button.onClick.AddListener(HandleClick);
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
            }
        }

        public void Bind(SkillUpgradeOption option, Action<SkillUpgradeOption> onClicked)
        {
            boundOption = option;
            clickCallback = onClicked;

            if (titleText != null)
            {
                titleText.text = option != null ? option.Title : string.Empty;
            }

            if (descriptionText != null)
            {
                descriptionText.text = option != null ? option.Description : string.Empty;
            }

            if (button != null)
            {
                button.interactable = option != null;
            }

            gameObject.SetActive(true);
        }

        public void Clear()
        {
            boundOption = null;
            clickCallback = null;

            if (titleText != null)
            {
                titleText.text = string.Empty;
            }

            if (descriptionText != null)
            {
                descriptionText.text = string.Empty;
            }

            if (button != null)
            {
                button.interactable = false;
            }

            gameObject.SetActive(false);
        }

        private void HandleClick()
        {
            if (boundOption == null || clickCallback == null)
            {
                return;
            }

            clickCallback.Invoke(boundOption);
        }
    }
}
