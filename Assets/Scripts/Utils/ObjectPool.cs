using System.Collections.Generic;
using UnityEngine;

namespace XianxiaSurvivor.Utils
{
    /// <summary>
    /// 用途：提供基础 GameObject 对象池，后续用于怪物、子弹、掉落物等需要复用的对象。
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int initialSize = 10;

        private readonly Queue<GameObject> pool = new Queue<GameObject>();

        private void Awake()
        {
            Preload(initialSize);
        }

        public GameObject Get()
        {
            if (pool.Count == 0)
            {
                GameObject createdObject = CreateObject();

                if (createdObject == null)
                {
                    return null;
                }

                pool.Enqueue(createdObject);
            }

            GameObject pooledObject = pool.Dequeue();
            pooledObject.SetActive(true);
            return pooledObject;
        }

        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            GameObject pooledObject = Get();

            if (pooledObject == null)
            {
                return null;
            }

            pooledObject.transform.SetPositionAndRotation(position, rotation);
            return pooledObject;
        }

        public void Release(GameObject pooledObject)
        {
            if (pooledObject == null)
            {
                return;
            }

            pooledObject.SetActive(false);
            pooledObject.transform.SetParent(transform);
            pool.Enqueue(pooledObject);
        }

        private void Preload(int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject createdObject = CreateObject();

                if (createdObject != null)
                {
                    pool.Enqueue(createdObject);
                }
            }
        }

        private GameObject CreateObject()
        {
            if (prefab == null)
            {
                Debug.LogWarning("ObjectPool 缺少 prefab，无法创建对象。", this);
                return null;
            }

            GameObject createdObject = Instantiate(prefab, transform);
            createdObject.SetActive(false);
            return createdObject;
        }
    }
}
