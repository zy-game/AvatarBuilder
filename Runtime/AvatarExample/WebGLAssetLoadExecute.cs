using System.Collections;
using System.IO;
using Gaming;
using Gaming.Resource;
using Gaming.Runnable;
using UnityEngine;
using UnityEngine.Networking;

namespace Example
{
    class WebGLAssetLoadExecute : IExecuter<IResContext>
    {
        private IEnumerator context;

        public void Dispose()
        {
            Gaming.Services.MonoBehaviour.StopCoroutine(context);
        }

        public void Execute(IRunnable<IResContext> runnable, params object[] args)
        {
            Services.MonoBehaviour.StartCoroutine(context = Runnable(runnable, (WebGLAssetLoader)args[0], (string)args[1]));
        }

        IEnumerator Runnable(IRunnable<IResContext> runnable, WebGLAssetLoader resourceLoader, string assetUrl)
        {
            string extension = Path.GetExtension(assetUrl);
            string fileName = Path.GetFileNameWithoutExtension(assetUrl);
            UnityWebRequest request = null;
            if (assetUrl.EndsWith(".assetbundle"))
            {
                request = UnityWebRequestAssetBundle.GetAssetBundle(assetUrl);
            }
            else
            {
                request = UnityWebRequestTexture.GetTexture(assetUrl);
            }

            yield return request.SendWebRequest();
            IResContext resContext = null;
            if (!request.isDone || request.result != UnityWebRequest.Result.Success)
            {
                Services.Console.WriteErrorFormat("asset load failur!\n{0}\n{1}", assetUrl, request.error);
                yield return runnable.Execute(default);
                yield break;
            }

            if (assetUrl.EndsWith(".png"))
            {
                resContext = WebGLResContext.Generic(assetUrl, DownloadHandlerTexture.GetContent(request));
                resourceLoader._contexts.Add(assetUrl, resContext);
                yield return runnable.Execute(resContext);
                yield break;
            }

            if (assetUrl.EndsWith(".assetbundle"))
            {
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
                AssetBundleRequest bundleCreateRequest = bundle.LoadAssetAsync(fileName);
                yield return bundleCreateRequest;
                resContext = WebGLResContext.Generic(assetUrl, bundleCreateRequest.asset, bundle);
                resourceLoader._contexts.Add(assetUrl, resContext);
                yield return runnable.Execute(resContext);
            }
        }
    }
}