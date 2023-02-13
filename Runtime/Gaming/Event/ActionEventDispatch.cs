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
                Services.Console.WriteLine("Event:" + evtName + " args:" + evtData?.ToString());
                _callback(evtName, evtData);
            }
            catch (Exception e)
            {
                Services.Console.WriteError(e);
            }
        }

        public void SetCallback(Action<string, object> callback)
        {
            _callback = callback;
        }
    }
}