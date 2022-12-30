namespace AvatarBuild
{
    using System;
    using UnityEngine;
    using System.Collections;
    using System.IO;
    using AvatarBuild.Interface;
    using AvatarBuild.Config;

    public sealed class AvatarContorller : IContorller
    {
        private AvatarConfig config;
        private IBuilder builder;
        private GameObject basic;
        private string skeletonUrl;
        private string address;
        private Camera camera;
        private IResourceLoader loader;

        /// <summary>
        /// 释放接口
        /// </summary>
        public void Dispose()
        {
            builder.Dispose();
            config.Dispose();
            config = null;
            builder = null;
        }

        /// <summary>
        /// 初始化接口
        /// </summary>
        /// <param name="skeleton">基础骨骼模型资源地址</param>
        /// <param name="address">服务器地址</param>
        public void Initialize<T>(string skeleton, string address, Camera iconCamera) where T : IResourceLoader, new()
        {
            skeletonUrl = skeleton;
            this.camera = iconCamera;
            this.address = address;
            this.loader = new T();
            this.camera.targetTexture = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
            RenderTexture.active = this.camera.targetTexture;
            loader.LoadAssetAsync(skeletonUrl, handle =>
            {
                basic = handle.GetObject<GameObject>();
                if (basic == null)
                {
                    throw new Exception("basic skeleton not find");
                }
                basic.transform.localScale = Vector3.one;
                basic.transform.rotation = Quaternion.Euler(Vector3.zero);
                basic.transform.position = Vector3.zero;
                EventSystem.NotiflyEvent(EventNames.INITIALIZED_COMPLATED_EVENT);
            });
        }


        /// <summary>
        /// 确认是否初始化
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void EnsureInitializeInterface()
        {
            if (this.builder == null)
            {
                builder = new AvatarBuilder(basic);
            }
            if (this.config == null)
            {
                this.config = new AvatarConfig();
            }
        }

        /// <summary>
        /// 获取部位数据
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public ElementData GetElementData(Element element)
        {
            EnsureInitializeInterface();
            return this.config.GetElementData(element);
        }

        /// <summary>
        /// 设置部位模型
        /// </summary>
        /// <param name="element"></param>
        /// <param name="assetName"></param>
        public void SetElementData(ElementData elementData)
        {
            EnsureInitializeInterface();
            if (elementData == null)
            {
                throw new Exception("The element data cannot be empty");
            }
            if (string.IsNullOrEmpty(elementData.model) || string.IsNullOrEmpty(elementData.texture))
            {
                Debug.LogError("bad element");
                return;
            }
            ElementData oldElementData = GetElementData(elementData.element);
            void SetModle()
            {
                if (oldElementData != null && oldElementData.model == elementData.model)
                {
                    SetTexture();
                    return;
                }
                Debug.Log("set element model:" + elementData.model);
                loader.LoadAssetAsync(elementData.model, handle =>
                {
                    if (handle == null)
                    {
                        return;
                    }
                    GameObject elementModle = handle.GetObject<GameObject>();
                    if (elementModle == null)
                    {
                        throw new Exception("load resource failur. please check the resource is exist");
                    }
                    this.builder.SetElementModle(elementData.element, elementModle);
                    this.config.SetElementData(elementData);
                    SetTexture();
                });
            }
            void SetTexture()
            {
                if (oldElementData != null && oldElementData.texture == elementData.texture)
                {
                    EventSystem.NotiflyEvent(EventNames.SET_ELEMENT_DATA_COMPLATED);
                    return;
                }
                Debug.Log("set element texture:" + elementData.model);
                loader.LoadAssetAsync(elementData.texture, handle =>
                {
                    if (handle == null)
                    {

                        return;
                    }
                    Texture texture = handle.GetObject<Texture>();
                    if (texture == null)
                    {
                        throw new Exception("load resource failur. please check the resource is exist");
                    }
                    this.builder.SetElementTexture(elementData.element, texture);
                    this.config.SetElementData(elementData);
                    ShowGameObjectToCenter(basic);
                    EventSystem.NotiflyEvent(EventNames.SET_ELEMENT_DATA_COMPLATED, (int)elementData.element);
                });
            }
            SetModle();
        }


        /// <summary>
        /// 清理指定部件
        /// </summary>
        /// <param name="element"></param>
        public void ClearElement(Element element)
        {
            this.builder.DestoryElement(element);
            this.config.RemoveElement(element);
            EventSystem.NotiflyEvent(EventNames.CLEAR_ELMENT_DATA_COMPLATED, (int)element);
        }

        /// <summary>
        /// 导出配置
        /// </summary>
        /// <param name="configName"></param>
        /// <returns></returns>
        public string ExportConfig(string configName)
        {
            this.config.name = configName;
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(this.config);
            EventSystem.NotiflyEvent(EventNames.EXPORT_AVATAR_CONFIG_COMPLATED, json);
            return json;
        }

        /// <summary>
        /// 导入配置
        /// </summary>
        /// <param name="config"></param>
        /// <exception cref="Exception"></exception>
        public void ImportConfig(string config)
        {
            if (string.IsNullOrEmpty(config))
            {
                throw new Exception("the config connot be null");
            }
            AvatarConfig tempConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<AvatarConfig>(config);
            for (int i = 0; i < tempConfig.elements.Count; i++)
            {
                ElementData elementData = tempConfig.elements[i];
                if (elementData == null || string.IsNullOrEmpty(elementData.model))
                {
                    continue;
                }
                this.SetElementData(elementData);
            }
            this.config = tempConfig;
            EventSystem.NotiflyEvent(EventNames.IMPORT_CONFIG_COMPLATED);
        }

        /// <summary>
        /// 预览部件
        /// </summary>
        /// <param name="element">部件位置</param>
        /// <param name="texturePath">贴图路径</param>
        public void PreviewElement(Element element, byte[] bytes)
        {
            if (bytes != null && bytes.Length > 0)
            {
                Texture2D texture = new Texture2D(512, 512);
                texture.LoadImage(bytes);
                this.builder.SetElementTexture(element, texture);
            }
            ShowElementToCenter(element);
        }

        /// <summary>
        /// 设置正交相机的Size
        /// </summary>
        /// <param name="xmin">包围盒x方向最小值</param>
        /// <param name="xmax">包围盒x方向最大值</param>
        /// <param name="ymin">包围盒y方向最小值</param>
        /// <param name="ymax">包围盒y方向最大值</param>
        private void SetOrthCameraSize(float xmin, float xmax, float ymin, float ymax)
        {
            float xDis = xmax - xmin;
            float yDis = ymax - ymin;
            float sizeX = xDis / Camera.main.aspect;
            float sizeY = yDis;
            if (sizeX >= sizeY)
            {
                Camera.main.fieldOfView = sizeX * 6;
                camera.fieldOfView = sizeX * 6;
            }
            else
            {
                Camera.main.fieldOfView = sizeY * 6;
                camera.fieldOfView = sizeY * 6;
            }
        }

        /// <summary>
        /// 获取物体包围盒
        /// </summary>
        /// <param name="obj">父物体</param>
        /// <returns>物体包围盒</returns>
        private Bounds GetBoundPointsByObj(GameObject obj)
        {
            var bounds = new Bounds();
            if (obj != null)
            {
                var renders = obj.GetComponentsInChildren<Renderer>();
                if (renders != null)
                {
                    var boundscenter = Vector3.zero;
                    foreach (var item in renders)
                    {
                        boundscenter += item.bounds.center;
                    }
                    if (obj.transform.childCount > 0)
                        boundscenter /= obj.transform.childCount;
                    bounds = new Bounds(boundscenter, Vector3.zero);
                    foreach (var item in renders)
                    {
                        bounds.Encapsulate(item.bounds);
                    }
                }
            }
            return bounds;
        }

        /// <summary>
        /// 上传部件资源
        /// </summary>
        /// <param name="element"></param>
        /// <param name="filePath"></param>
        public void UploadElementAsset(Element element, byte[] bytes)
        {
            ElementData elementData = GetElementData(element);
            string modleName = Path.GetFileNameWithoutExtension(elementData.model) + Service.GetFileMd5(bytes);
            Service.UploadAssetResponse upload_icon_response = null;
            Service.UploadAssetResponse upload_texture_response = null;
            Texture2D texture = null;
            IEnumerator Runnable_GenericIcon()
            {
                PreviewElement(element, bytes);
                ShowElementToCenter(element);
                yield return new WaitForEndOfFrame();
                RenderTexture rt = camera.targetTexture;
                RenderTexture.active = rt;
                RenderTexture.active = camera.targetTexture;
                texture = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
                texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                texture.Apply();
                ShowGameObjectToCenter(basic);
                SetElementData(elementData);
                Runnable_UploadIconAsset();
            }

            void Runnable_UploadIconAsset()
            {
                string fileName = element.ToString() + "_" + modleName + "_icon.png";
                byte[] iconBytes = texture.EncodeToPNG();
                Service.RequestCreateFileData requestCreateTextureFileData = new Service.RequestCreateFileData(fileName, Service.GetFileMd5(iconBytes), "image/png", "2", bytes.Length);
                MonoBehaviourInstance.StartCor(Service.UploadAsset(address, requestCreateTextureFileData, iconBytes, (response, exception) =>
                {
                    if (exception != null)
                    {
                        Debug.LogError(exception);
                        return;
                    }
                    upload_icon_response = response;
                    Runnable_UploadTextureAsset();
                }));
            }
            void Runnable_UploadTextureAsset()
            {
                string fileName = element.ToString() + "_" + modleName + ".png";
                Service.RequestCreateFileData requestCreateTextureFileData = new Service.RequestCreateFileData(fileName, Service.GetFileMd5(bytes), "image/png", "2", bytes.Length);
                MonoBehaviourInstance.StartCor(Service.UploadAsset(address, requestCreateTextureFileData, bytes, (response, exception) =>
                {
                    if (exception != null)
                    {
                        Debug.LogError(exception);
                        return;
                    }

                    upload_texture_response = response;
                    Runnable_CreateUserUoloadElementData();
                }));
            }
            void Runnable_CreateUserUoloadElementData()
            {
                ElementData createElementData = new ElementData();
                createElementData.name = modleName;
                createElementData.element = element;
                createElementData.icon = upload_icon_response.data.url;
                createElementData.texture = upload_texture_response.data.url;
                createElementData.model = elementData.model;
                EventSystem.NotiflyEvent(EventNames.UPLOAD_ELEMENT_ASSET_COMPLATED, Newtonsoft.Json.JsonConvert.SerializeObject(createElementData));
            }
            MonoBehaviourInstance.StartCor(Runnable_GenericIcon());
        }

        /// <summary>
        /// 合并Avatar
        /// </summary>
        public void Combine()
        {
            this.builder.Combine();
            this.builder.ClearCombine();
#if UNITY_WEBGL
            EventSystem.NotiflyEvent(EventNames.COMBINE_AVATAR_COMPLATED);
#else
            EventSystem.NotiflyEvent(EventNames.COMBINE_AVATAR_COMPLATED, basic);
#endif
        }

        private void ShowElementToCenter(Element element)
        {
            GameObject gameObject = this.builder.GetElementObject(element);
            if (gameObject == null)
            {
                return;
            }
            ShowGameObjectToCenter(gameObject);
        }

        private void ShowGameObjectToCenter(GameObject gameObject)
        {
            var bound = GetBoundPointsByObj(gameObject);
            var center = bound.center;
            var extents = bound.extents;
            camera.transform.position = new Vector3(center.x, center.y, center.z + 10);
            Camera.main.transform.position = new Vector3(center.x, center.y, center.z + 10);
            SetOrthCameraSize(center.x - extents.x, center.x + extents.x, center.y - extents.y, center.y + extents.y);
        }

    }
}