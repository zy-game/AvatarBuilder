namespace Gaming.Event
{
    using System;

    class ActionEventDispatch : IEventDispatch
    {
        private Action<string, object> _callback;
        public void Dispose()
        {
            _callback = null;
        }

        public void Notifly(string evtName, object evtData)
        {
            try
            {
                GamingService.Logger.Log("Event:" + evtName + " args:" + evtData?.ToString());
                _callback(evtName, evtData);
            }
            catch (Exception e)
            {
                GamingService.Logger.LogError(e);
            }
        }

        public void SetCallback(Action<string, object> callback)
        {
            _callback = callback;
        }
    }
}