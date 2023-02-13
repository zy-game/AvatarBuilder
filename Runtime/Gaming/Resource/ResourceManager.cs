using Gaming.Drawing;
using Gaming.Extension;
using Gaming.Runnable;
using System;
using System.Collections.Generic;
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
            Services.Refrence.Release(_resourceLoader);
            _resourceLoader = null;
        }

        internal void SetResourceLoader<T>() where T : IResourceLoader
        {
            _resourceLoader = Services.Refrence.Require<T>();
        }

        internal IRunnable<IResContext> LoadAssetAsync(string assetName)
        {
            EnsureResourceLoader();
            return _resourceLoader.LoadAssetAsync(assetName);
        }

        internal void Release(IResContext context)
        {
            _resourceLoader.Release(context);
        }
    }
}
