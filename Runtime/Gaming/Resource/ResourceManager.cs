using Gaming.Runnable;
using System;
using System.Collections;
using UnityEngine;
namespace Gaming.Resource
{
    class ResourceManager : Singleton<ResourceManager>, IRefrence
    {
        private IResourceLoader _resourceLoader;

        private void EnsureResourceLoader()
        {
            if (_resourceLoader == null)
            {
                throw new ArgumentNullException("resource loader");
            }
        }
        public void Dispose()
        {
            GamingService.Refrence.Release(_resourceLoader);
            _resourceLoader = null;
        }

        internal void SetResourceLoader<T>() where T : IResourceLoader
        {
            _resourceLoader = GamingService.Refrence.Require<T>();
        }

        internal IRunnable<IResContext> LoadAsset(string assetName)
        {
            EnsureResourceLoader();
            RunnableAsync<IResContext> internalRunnable = GamingService.Refrence.Require<RunnableAsync<IResContext>>();
            internalRunnable.Execute<InternalResourceLoadableExecuter>(assetName, _resourceLoader);
            return internalRunnable;
        }

        internal void Release(IResContext context)
        {
            EnsureResourceLoader();
            _resourceLoader.Release(context);
        }
    }

    class InternalResourceLoadableExecuter : IExecuter<IResContext>
    {
        public IResContext target
        {
            get;
            private set;
        }

        public void Dispose()
        {
            target = null;
        }

        public IEnumerator Execute(params object[] args)
        {
            string assetName = (string)args[0];
            IResourceLoader resourceLoader = (IResourceLoader)args[1];
            resourceLoader.LoadAssetAsync(assetName, res =>
            {
                target = res;
            });
            yield return new WaitUntil(() => target != null);
        }
    }
}
