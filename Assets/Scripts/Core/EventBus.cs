using System;
using System.Collections.Generic;

namespace XianxiaSurvivor.Core
{
    /// <summary>
    /// 用途：提供最基础的事件订阅、取消订阅和分发，帮助 UI 和玩法逻辑解耦。
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> EventTable = new Dictionary<Type, Delegate>();

        public static void Subscribe<T>(Action<T> listener)
        {
            if (listener == null)
            {
                return;
            }

            Type eventType = typeof(T);

            if (EventTable.TryGetValue(eventType, out Delegate existingDelegate))
            {
                EventTable[eventType] = Delegate.Combine(existingDelegate, listener);
                return;
            }

            EventTable.Add(eventType, listener);
        }

        public static void Unsubscribe<T>(Action<T> listener)
        {
            if (listener == null)
            {
                return;
            }

            Type eventType = typeof(T);

            if (!EventTable.TryGetValue(eventType, out Delegate existingDelegate))
            {
                return;
            }

            Delegate currentDelegate = Delegate.Remove(existingDelegate, listener);

            if (currentDelegate == null)
            {
                EventTable.Remove(eventType);
                return;
            }

            EventTable[eventType] = currentDelegate;
        }

        public static void Raise<T>(T eventData)
        {
            Type eventType = typeof(T);

            if (EventTable.TryGetValue(eventType, out Delegate existingDelegate)
                && existingDelegate is Action<T> action)
            {
                action.Invoke(eventData);
            }
        }

        public static void Clear()
        {
            EventTable.Clear();
        }
    }
}
