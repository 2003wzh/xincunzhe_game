using UnityEngine;

namespace XianxiaSurvivor.Utils
{
    /// <summary>
    /// Purpose: Keeps a 2D camera following a target with smooth movement and fixed camera depth.
    /// </summary>
    [DisallowMultipleComponent]
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Follow Settings")]
        [SerializeField] private Vector2 offset = Vector2.zero;
        [SerializeField] private float smoothTime = 0.12f;
        [SerializeField] private float cameraZ = -10f;

        private Vector3 velocity;
        private bool warnedMissingTarget;

        private void LateUpdate()
        {
            if (target == null)
            {
                WarnMissingTargetOnce();
                return;
            }

            Vector3 targetPosition = target.position;
            Vector3 desiredPosition = new Vector3(
                targetPosition.x + offset.x,
                targetPosition.y + offset.y,
                cameraZ);

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref velocity,
                Mathf.Max(0.01f, smoothTime));
        }

        private void OnValidate()
        {
            smoothTime = Mathf.Max(0.01f, smoothTime);
        }

        private void WarnMissingTargetOnce()
        {
            if (warnedMissingTarget)
            {
                return;
            }

            warnedMissingTarget = true;
            Debug.LogWarning("CameraFollow is missing Target. Drag the player Transform into Target in the Inspector.", this);
        }
    }
}
