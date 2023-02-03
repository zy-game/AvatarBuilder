namespace Gaming.Resource
{
    using Gaming;
    using System;

    /// <summary>
    /// ×ÊÔ´¼ÓÔØÆ÷
    /// </summary>
    public interface IResourceLoader : IRefrence
    {
        void LoadAssetAsync(string AssetUrl, Action<IResContext> callback);
        void Release(IResContext handle);
    }
}
