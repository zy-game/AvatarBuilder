namespace Gaming.Resource
{
    using Gaming;
    using UnityEngine;
    using Object = UnityEngine.Object;

    /// <summary>
    /// 资源对象
    /// </summary>
    public interface IResContext : IRefrence
    {
        string name { get; }
        int refCount { get; }
        T GetObject<T>(GameObject bind = null) where T : Object;

        bool EnsureSuccessful();

        void Release();
    }
}
