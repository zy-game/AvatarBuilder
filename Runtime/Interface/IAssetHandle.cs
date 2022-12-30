namespace AvatarBuild.Interface
{
    using UnityEngine;
    using Object = UnityEngine.Object;

    /// <summary>
    /// 资源对象
    /// </summary>
    public interface IAssetHandle
    {
        string name { get; }
        T GetObject<T>(GameObject bind = null) where T : Object;
    }
}
