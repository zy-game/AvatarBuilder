namespace Gaming.Event
{
    public interface IEventDispatch : IRefrence
    {
        void Notifly(string evtName, object evtData);
    }
}