using System.Collections;
using UnityEngine;

namespace XianxiaSurvivor.Player
{
    /// <summary>
    /// 用途：播放玩家释放功法时的短促视觉反馈，不切换 Animator 状态。
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerCastFeedback : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private float flashDuration = 0.08f;
        [SerializeField] private float scalePunch = 1.05f;
        [SerializeField] private float scaleDuration = 0.08f;
        [SerializeField] private GameObject castEffectPrefab;
        [SerializeField] private Transform castEffectPoint;
        [SerializeField] private bool spawnCastEffect = false;

        private Color originalColor = Color.white;
        private Vector3 originalScale = Vector3.one;
        private Coroutine feedbackRoutine;
        private bool hasCachedOriginalState;
        private bool hasWarnedMissingSpriteRenderer;

        private void Awake()
        {
            CacheReferences();
            CacheOriginalState();
        }

        private void OnDisable()
        {
            StopFeedbackRoutine();
            RestoreState();
        }

        private void OnValidate()
        {
            flashDuration = Mathf.Max(0f, flashDuration);
            scalePunch = Mathf.Max(0.01f, scalePunch);
            scaleDuration = Mathf.Max(0f, scaleDuration);
        }

        public void PlayCastFeedback()
        {
            CacheReferences();

            if (spriteRenderer == null)
            {
                WarnMissingSpriteRendererOnce();
                return;
            }

            if (visualRoot == null)
            {
                visualRoot = spriteRenderer.transform;
            }

            if (!hasCachedOriginalState)
            {
                CacheOriginalState();
            }

            StopFeedbackRoutine();
            RestoreState();
            SpawnCastEffect();
            feedbackRoutine = StartCoroutine(PlayFeedbackRoutine());
        }

        private void CacheReferences()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (visualRoot == null && spriteRenderer != null)
            {
                visualRoot = spriteRenderer.transform;
            }
        }

        private void CacheOriginalState()
        {
            if (spriteRenderer == null)
            {
                WarnMissingSpriteRendererOnce();
                return;
            }

            originalColor = spriteRenderer.color;
            originalScale = visualRoot != null ? visualRoot.localScale : Vector3.one;
            hasCachedOriginalState = true;
        }

        private IEnumerator PlayFeedbackRoutine()
        {
            spriteRenderer.color = flashColor;

            if (visualRoot != null)
            {
                visualRoot.localScale = originalScale * scalePunch;
            }

            float waitDuration = Mathf.Max(flashDuration, scaleDuration);

            if (waitDuration > 0f)
            {
                yield return new WaitForSeconds(waitDuration);
            }

            RestoreState();
            feedbackRoutine = null;
        }

        private void SpawnCastEffect()
        {
            if (!spawnCastEffect || castEffectPrefab == null)
            {
                return;
            }

            Transform effectPoint = castEffectPoint != null ? castEffectPoint : visualRoot;
            Vector3 position = effectPoint != null ? effectPoint.position : transform.position;
            Quaternion rotation = effectPoint != null ? effectPoint.rotation : Quaternion.identity;
            Instantiate(castEffectPrefab, position, rotation);
        }

        private void StopFeedbackRoutine()
        {
            if (feedbackRoutine == null)
            {
                return;
            }

            StopCoroutine(feedbackRoutine);
            feedbackRoutine = null;
        }

        private void RestoreState()
        {
            if (!hasCachedOriginalState)
            {
                return;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }

            if (visualRoot != null)
            {
                visualRoot.localScale = originalScale;
            }
        }

        private void WarnMissingSpriteRendererOnce()
        {
            if (hasWarnedMissingSpriteRenderer)
            {
                return;
            }

            hasWarnedMissingSpriteRenderer = true;
            Debug.LogWarning("PlayerCastFeedback 找不到 SpriteRenderer，施法反馈不会播放。", this);
        }
    }
}
