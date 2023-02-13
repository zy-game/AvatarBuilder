using Gaming;
using Gaming.Resource;
using UnityEngine;

namespace Example
{
    class AssetCache : IRefrence
    {
        public float time;
        public IResContext context;

        internal static AssetCache Generic(IResContext context)
        {
            AssetCache assetCache = Services.Refrence.Require<AssetCache>();
            assetCache.context = context;
            assetCache.time = Time.realtimeSinceStartup;
            return assetCache;
        }

        public void Dispose()
        {
            context = null;
        }
    }
}