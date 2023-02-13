using Gaming;
using Gaming.Resource;
using UnityEngine;

namespace Example
{
    public sealed class WebGLResContext : IResContext
    {
        public string name { get; private set; }
        public int refCount { get; private set; }

        private Object mainAsset;
        private AssetBundle bundle;

        public static WebGLResContext Generic(string name, Object assetObject, AssetBundle bundle)
        {
            WebGLResContext resContext = Services.Refrence.Require<WebGLResContext>();
            resContext.bundle = bundle;
            resContext.mainAsset = assetObject;
            resContext.name = name;
            return resContext;
        }

        public static WebGLResContext Generic(string name, Texture2D texture)
        {
            WebGLResContext resContext = Services.Refrence.Require<WebGLResContext>();
            resContext.mainAsset = texture;
            resContext.name = name;
            return resContext;
        }

        public T GetObject<T>(GameObject bind = null) where T : Object
        {
            if (mainAsset == null)
            {
                return default;
            }

            Object result = null;
            if (typeof(GameObject) != typeof(T))
            {
                result = mainAsset;
            }
            else
            {
                GameObject gameObject = (GameObject)Object.Instantiate(mainAsset);
                gameObject.name = mainAsset.name;
                bind = gameObject;
                result = gameObject;
            }

            if (bind != null)
            {
                Services.MonoBehaviour.DestoryCallback(bind, Services.Resource.Release, this);
            }

            refCount++;
            return (T)result;
        }

        public void Dispose()
        {
            refCount = 0;
            if (bundle != null)
            {
                bundle.Unload(true);
            }

            bundle = null;
            mainAsset = null;
            Resources.UnloadUnusedAssets();
        }

        public void Release()
        {
            refCount--;
        }

        public bool EnsureSuccessful()
        {
            return mainAsset != null;
        }
    }
}