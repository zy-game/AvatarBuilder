namespace Example
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Gaming.Resource;
    using Gaming.Runnable;
    using Gaming;

    public class WebGLAssetLoader : IResourceLoader
    {
        private const float UnloadAssetIntervalTime = 5f;
        private List<AssetCache> _cacheDataList = new List<AssetCache>();
        internal Dictionary<string, IResContext> _contexts = new Dictionary<string, IResContext>();
        private Dictionary<string, IRunnable<IResContext>> _waitingLoadAssetList = new Dictionary<string, IRunnable<IResContext>>();


        public WebGLAssetLoader()
        {
            Services.MonoBehaviour.AddUpdate(OnUpdate);
        }

        private void OnUpdate()
        {
            if (_cacheDataList.Count == 0)
            {
                return;
            }

            for (int i = _cacheDataList.Count - 1; i >= 0; i--)
            {
                AssetCache assetCache = _cacheDataList[i];
                if (Time.realtimeSinceStartup - assetCache.time < UnloadAssetIntervalTime)
                {
                    continue;
                }

                _cacheDataList.Remove(assetCache);
                Services.Refrence.Release(assetCache);
            }
        }

        private bool TryGetCacheValue(string name, out AssetCache assetCache)
        {
            lock (_cacheDataList)
            {
                assetCache = _cacheDataList.Find(x => x.context.name == name);
                return assetCache != null;
            }
        }

        public void Dispose()
        {
            foreach (var item in _contexts.Values)
            {
                Services.Refrence.Release(item);
            }

            _contexts.Clear();
            foreach (var item in _waitingLoadAssetList.Values)
            {
                Services.Refrence.Release(item);
            }

            _waitingLoadAssetList.Clear();
        }

        public IRunnable<IResContext> LoadAssetAsync(string assetUrl)
        {
            if (string.IsNullOrEmpty(assetUrl))
            {
                throw new ArgumentNullException("assetUrl");
            }

            IRunnable<IResContext> runnable = Gaming.Services.Execute.Create<IResContext>();
            if (_contexts.TryGetValue(assetUrl, out IResContext context))
            {
                Services.MonoBehaviour.StartCoroutine(runnable.Execute(context));
                return runnable;
            }

            if (TryGetCacheValue(assetUrl, out AssetCache cache))
            {
                _cacheDataList.Remove(cache);
                _contexts.Add(assetUrl, cache.context);
                Services.MonoBehaviour.StartCoroutine(runnable.Execute(cache.context));
            }

            if (_waitingLoadAssetList.TryGetValue(assetUrl, out IRunnable<IResContext> waiting))
            {
                waiting.Then(runnable.Execute);
                return runnable;
            }

            WebGLAssetLoadExecute webGLAssetLoadExecute = Gaming.Services.Refrence.Require<WebGLAssetLoadExecute>();
            webGLAssetLoadExecute.Execute(runnable, this, assetUrl);
            _waitingLoadAssetList.Add(assetUrl, runnable);
            runnable.Then(_ => { _waitingLoadAssetList.Remove(assetUrl); });
            return runnable;
        }

        public void Release(IResContext context)
        {
            context.Release();
            if (context.refCount > 0)
            {
                return;
            }

            if (_contexts.ContainsKey(context.name))
            {
                _contexts.Remove(context.name);
            }

            _cacheDataList.Add(AssetCache.Generic(context));
        }
    }
}