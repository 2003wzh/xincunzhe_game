using UnityEngine;

namespace XianxiaSurvivor.Combat
{
    /// <summary>
    /// 用途：控制一次性视觉特效的生命周期，启用后自动销毁自身。
    /// </summary>
    [DisallowMultipleComponent]
    public class OneShotEffect : MonoBehaviour
    {
        [SerializeField] private float lifetime = 0.5f;

        private void OnEnable()
        {
            Destroy(gameObject, Mathf.Max(0f, lifetime));
        }
    }
}
