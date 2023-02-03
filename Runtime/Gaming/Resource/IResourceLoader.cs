namespace Gaming.Resource
{
    using Gaming;
    using System;

    /// <summary>
    /// ��Դ������
    /// </summary>
    public interface IResourceLoader : IRefrence
    {
        void LoadAssetAsync(string AssetUrl, Action<IResContext> callback);
        void Release(IResContext handle);
    }
}
