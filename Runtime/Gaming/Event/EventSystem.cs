namespace Gaming.Event
{
    using System;
    using System.Collections.Generic;

    sealed class EventSystem : Singleton<EventSystem>, IRefrence
    {
        private DefaultEventDispatch _defaultEventDistribution;
        private Dictionary<int, IEventDispatch> _eventDispatches = new Dictionary<int, IEventDispatch>();

        public EventSystem()
        {
            _defaultEventDistribution = this.SubscribeDistribution<DefaultEventDispatch>();
        }

        public void NotiflyEvent(string evtName, object args = null)
        {
            try
            {
                foreach (var item in _eventDispatches.Values)
                {
                    item.Notifly(evtName, args);
                }
            }
            catch (Exception e)
            {
                Services.Console.WriteError(e);
            }
        }

        public T SubscribeDistribution<T>() where T : IEventDispatch => (T)SubscribeDistribution(typeof(T));

        public void UnsubscribeDistribution<T>() where T : IEventDispatch => UnsubscribeDistribution(typeof(T));

        public IEventDispatch SubscribeDistribution(Action<string, object> notifly)
        {
            ActionEventDispatch actionEventDispatch = SubscribeDistribution<ActionEventDispatch>();
            actionEventDispatch.SetCallback(notifly);
            _eventDispatches.Add(notifly.GetHashCode(), actionEventDispatch);
            return actionEventDispatch;
        }

        public void UnsubscribeDistribution(Action<string, object> notifly)
        {
            if (!_eventDispatches.TryGetValue(notifly.GetHashCode(), out IEventDispatch distribution))
            {
                return;
            }
            _eventDispatches.Remove(notifly.GetHashCode());
        }

        public IEventDispatch SubscribeDistribution(Type distributionType)
        {
            if (distributionType == null)
            {
                throw new ArgumentNullException("distributionType");
            }
            if (!typeof(IEventDispatch).IsAssignableFrom(distributionType))
            {
                throw new NotImplementedException("the event distribution is not impmented to IEventDistribution");
            }
            if (distributionType.IsAbstract || distributionType.IsInterface)
            {
                throw new Exception("the eventdestribution can not be abstract or interface");
            }
            IEventDispatch eventDistribution = (IEventDispatch)Services.Refrence.Require(distributionType);
            _eventDispatches.Add(distributionType.GetHashCode(), eventDistribution);
            return eventDistribution;
        }

        public void UnsubscribeDistribution(Type distributionType)
        {
            if (!_eventDispatches.TryGetValue(distributionType.GetHashCode(), out IEventDispatch distribution))
            {
                return;
            }
            _eventDispatches.Remove(distributionType.GetHashCode());
        }

        public void Register(string eventName, Action<object> callback) => _defaultEventDistribution.Add(eventName, callback);

        public void Unregister(string eventName, Action<object> callback) => _defaultEventDistribution.Remove(eventName, callback);

        public void Dispose()
        {
            this._defaultEventDistribution.Dispose();
            this._eventDispatches.Clear();
        }
    }
}