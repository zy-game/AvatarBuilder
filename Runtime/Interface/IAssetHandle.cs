namespace AvatarBuild.Interface
{
    using UnityEngine;
    using Object = UnityEngine.Object;

    /// <summary>
    /// ��Դ����
    /// </summary>
    public interface IAssetHandle
    {
        string name { get; }
        T GetObject<T>(GameObject bind = null) where T : Object;
    }
}
