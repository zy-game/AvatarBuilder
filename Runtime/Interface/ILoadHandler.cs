namespace AvatarBuild.Interface
{
    using System;

    /// <summary>
    /// ��Դ������
    /// </summary>
    public interface IResourceLoader
    {
        void LoadAssetAsync(string AssetUrl, Action<IAssetHandle> callback);
        void Release(IAssetHandle handle);
    }
}
