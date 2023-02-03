namespace Gaming.Event
{
    using System;
    using System.Collections.Generic;

    class DefaultEventDispatch : IEventDispatch
    {
        private Dictionary<string, List<Action<object>>> actions = new Dictionary<string, List<Action<object>>>();

        public void Notifly(string evtName, object evtData)
        {
            if (!actions.TryGetValue(evtName, out List<Action<object>> handle))
            {
                return;
            }
            GamingService.Logger.Log("Event:" + evtName + " args:" + evtData?.ToString());
            for (int i = handle.Count - 1; i >= 0; i--)
            {
                SafeRun(handle[i], evtData);
            }
        }

        void SafeRun(Action<object> callback, object args)
        {
            try
            {
                callback(args);
            }
            catch
            {
            }
        }

        public void Add(string evtName, Action<object> callback)
        {
            if (!actions.TryGetValue(evtName, out List<Action<object>> callbacks))
            {
                actions.Add(evtName, callbacks = new List<Action<object>>());
            }
            actions[evtName].Add(callback);
        }

        public void Remove(string evtName, Action<object> callback)
        {
            if (!actions.TryGetValue(evtName, out List<Action<object>> callbacks))
            {
                return;
            }
            if (!actions[evtName].Contains(callback))
            {
                return;
            }
            actions[evtName].Remove(callback);
        }

        public void Dispose()
        {
            actions.Clear();
        }
    }
}