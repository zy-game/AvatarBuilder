namespace Gaming.Resource
{
    using Gaming;
    using Gaming.Runnable;
    using System;

    /// <summary>
    /// ×ÊÔ´¼ÓÔØÆ÷
    /// </summary>
    public interface IResourceLoader : IRefrence
    {
        IRunnable<IResContext> LoadAssetAsync(string AssetUrl);
        void Release(IResContext handle);
    }
}
