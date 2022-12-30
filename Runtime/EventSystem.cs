namespace AvatarBuild
{
    using System;
    using UnityEngine;
    using System.Collections.Generic;

    public sealed class EventSystem
    {
        private static event Action<string, object> ohterNotifly;
        private static Dictionary<string, List<Action<object>>> eventHandles = new Dictionary<string, List<Action<object>>>();
        public static void NotiflyEvent(string eventName, object eventData = null)
        {
            ohterNotifly?.Invoke(eventName, eventData);
            if (!eventHandles.TryGetValue(eventName, out List<Action<object>> callbacks))
            {
                return;
            }
            for (int i = callbacks.Count - 1; i >= 0; i--)
            {
                try
                {
                    callbacks[i].Invoke(eventData);
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("notifly event failur:{0}", e);
                }
            }
        }

        public static void RegisterNotifly(Action<string, object> notifly)
        {
            ohterNotifly += notifly;
        }

        public static void UnregisterNotifly(Action<string, object> notifly)
        {
            ohterNotifly -= notifly;
        }

        public static void Register(string eventName, Action<object> callback)
        {
            if (!eventHandles.TryGetValue(eventName, out List<Action<object>> callbacks))
            {
                eventHandles.Add(eventName, callbacks = new List<Action<object>>());
            }
            callbacks.Add(callback);
        }

        public static void Unregister(string eventName, Action<object> callback)
        {
            if (!eventHandles.TryGetValue(eventName, out List<Action<object>> callbacks))
            {
                eventHandles.Add(eventName, callbacks = new List<Action<object>>());
            }
            if (callbacks.Contains(callback))
            {
                callbacks.Remove(callback);
            }
        }
    }
}