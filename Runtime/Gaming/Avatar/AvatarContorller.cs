namespace Gaming.Avatar
{
    using System;
    using UnityEngine;
    using System.Collections;
    using System.IO;
    using System.Linq;
    using Gaming.Config;
    using Gaming.Resource;
    using Gaming.Extension;
    using Gaming.Event;
    using Gaming.Transport;
    using Gaming.Runnable;

    public sealed class AvatarContorller : IAvatar
    {
        private string _address;
        private IBuilder _builder;
        private Camera iconCamera;
        private Camera mainCamera;
        private AvatarConfig config;
        private GameObject _skeleton;
        private float nowCamEulerX;
        private Vector3 mouseLeapPose;
        private string bundleAddressable;
        private Vector3 rotationTargetPosition;


        private readonly int maxAngle = 90;
        private readonly int minAngle = -90;
        private readonly float rotateSpeed = 5;
        private readonly int minFieldOfView = 5;
        private readonly int maxFieldOfView = 15;
        private readonly float movementSpeed = 0.05f;
        private readonly float fieldOfViewInternal = 2f;

        public string address
        {
            get { return _address; }
        }

        public IBuilder builder
        {
            get { return _builder; }
        }

        public GameObject gameObject
        {
            get { return _skeleton; }
        }

        public AvatarContorller() : this(null)
        {
        }

        public AvatarContorller(Camera mainCamera) : this(mainCamera, null)
        {
        }

        public AvatarContorller(Camera mainCamera, Camera iconCamera)
        {
            this.mainCamera = mainCamera;
            this.iconCamera = iconCamera;
            if (this.iconCamera != null)
            {
                this.iconCamera.targetTexture = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
                RenderTexture.active = this.iconCamera.targetTexture;
            }
        }

        /// <summary>
        /// 释放接口
        /// </summary>
        public void Dispose()
        {
            Services.MonoBehaviour.RemoveCallback(RotationScreenView);
            Services.MonoBehaviour.RemoveCallback(MovementScreenView);
            Services.MonoBehaviour.RemoveCallback(ScaleScreenView);
            builder.Dispose();
            config.Dispose();
            config = null;
            _builder = null;
            GameObject.DestroyImmediate(_skeleton);
            iconCamera = null;
        }

        /// <summary>
        /// 初始化接口
        /// </summary>
        /// <param name="skeleton">基础骨骼模型资源地址</param>
        /// <param name="address">服务器地址</param>
        public void Initialize(string skeleton, string address)
        {
            //todo  skeleton = https://xxx.beijing.aliyun.com/webgl/mesh_girl_001_skeleton.assetbundle
            this._address = address;
            this.bundleAddressable = skeleton.Substring(0, skeleton.LastIndexOf("/"));
            this.bundleAddressable = bundleAddressable.Substring(0, bundleAddressable.LastIndexOf("/"));
            IRunnable<IResContext> runnable = Services.Resource.LoadAssetAsync(GetAssetBundleAddressable(Path.GetFileName(skeleton)));
            runnable.Then(OnLoadSkeletonObjectCompleted);
        }

        private void OnLoadSkeletonObjectCompleted(IResContext context)
        {
            if (context == null || !context.EnsureSuccessful())
            {
                throw new Exception("basic skeleton not find");
            }

            _skeleton = context.GetObject<GameObject>();
            _skeleton.transform.localScale = Vector3.one;
            _skeleton.transform.rotation = Quaternion.Euler(Vector3.zero);
            _skeleton.transform.position = Vector3.zero;
            Services.MonoBehaviour.AddUpdate(RotationScreenView);
            Services.MonoBehaviour.AddUpdate(MovementScreenView);
            Services.MonoBehaviour.AddUpdate(ScaleScreenView);
            Services.Events.Notice(EventNames.INITIALIZED_COMPLATED_EVENT);
        }


        public void PlayAni(string name)
        {
            _skeleton.GetComponent<Animator>().enabled = true;
            this.gameObject.GetComponent<Animator>()?.Play(name);
        }

        private void RotationScreenView()
        {
            if (mainCamera == null || !Input.GetKey(KeyCode.LeftAlt) || !Input.GetKey(KeyCode.Mouse0))
            {
                return;
            }

            float offset_x = Input.GetAxis("Mouse X");
            float offset_y = Input.GetAxis("Mouse Y");
            mouseLeapPose.x = Mathf.Lerp(mouseLeapPose.x, offset_x, 5 * Time.deltaTime);
            mouseLeapPose.y = Mathf.Lerp(mouseLeapPose.y, offset_y, 5 * Time.deltaTime);
            rotationTargetPosition = gameObject.transform.position;
            mainCamera.transform.RotateAround(rotationTargetPosition, Vector3.up, mouseLeapPose.x * rotateSpeed);
            if (iconCamera != null)
            {
                iconCamera.transform.RotateAround(rotationTargetPosition, Vector3.up, mouseLeapPose.x * rotateSpeed);
            }

            nowCamEulerX = nowCamEulerX - mouseLeapPose.y * rotateSpeed;
            if (nowCamEulerX > maxAngle || nowCamEulerX < minAngle)
            {
                nowCamEulerX = nowCamEulerX + mouseLeapPose.y * rotateSpeed;
                mouseLeapPose.y = 0;
            }

            if (Mathf.Abs(-mouseLeapPose.y * rotateSpeed) < 0.02)
                return;
            mainCamera.transform.RotateAround(Vector3.zero, mainCamera.transform.right, -mouseLeapPose.y * rotateSpeed);
            if (iconCamera != null)
            {
                iconCamera.transform.RotateAround(Vector3.zero, mainCamera.transform.right, -mouseLeapPose.y * rotateSpeed);
            }
        }


        private void MovementScreenView()
        {
            if (mainCamera == null || !Input.GetKey(KeyCode.Space) || !Input.GetKey(KeyCode.Mouse0))
            {
                return;
            }

            Vector3 p0 = mainCamera.transform.position;
            Vector3 p01 = p0 - Input.GetAxisRaw("Mouse X") * movementSpeed * mainCamera.transform.right;
            Vector3 p03 = p01 - Input.GetAxisRaw("Mouse Y") * movementSpeed * mainCamera.transform.up;
            mainCamera.transform.position = p03;
            if (iconCamera == null)
            {
                return;
            }

            iconCamera.transform.position = p03;
        }

        private void ScaleScreenView()
        {
            if (mainCamera == null)
            {
                return;
            }

            if (Input.mouseScrollDelta.y > 0 && mainCamera.fieldOfView > minFieldOfView)
            {
                mainCamera.fieldOfView -= fieldOfViewInternal;
                if (iconCamera == null)
                {
                    return;
                }

                iconCamera.fieldOfView -= fieldOfViewInternal;
            }

            if (Input.mouseScrollDelta.y < 0 && mainCamera.fieldOfView < maxFieldOfView)
            {
                mainCamera.fieldOfView += fieldOfViewInternal;
                if (iconCamera == null)
                {
                    return;
                }

                iconCamera.fieldOfView += fieldOfViewInternal;
            }
        }


        /// <summary>
        /// 确认是否初始化
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void EnsureInitializeInterface()
        {
            if (this._skeleton == null)
            {
                throw new Exception("the avatar is not instance skeleton data");
            }

            if (this.builder == null)
            {
                _builder = new AvatarBuilder(_skeleton);
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
            return this.config.GetValue(element);
        }

        private string GetAssetBundleAddressable(string bundleName)
        {
            bundleName = bundleName.ToLower();
#if UNITY_ANDROID
            return this.bundleAddressable + "/android/" + bundleName;
#elif UNITY_IPHONE
            return this.bundleAddressable + "/ios/" + bundleName;
#elif UNITY_WEBGL
            return this.bundleAddressable + "/webgl/" + bundleName;
#else
            return this.bundleAddressable + "/windows/" + bundleName;
#endif
        }

        private string GetDefaultTextureAddressable(string bundleName)
        {
            return this.bundleAddressable + "/elements/" + bundleName;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// 设置部位模型
        /// </summary>
        /// <param name="element"></param>
        /// <param name="assetName"></param>
        public void SetElementData(params ElementData[] elementDatas)
        {
            EnsureInitializeInterface();
            Services.Console.WriteLine("element count:" + elementDatas.Length);
            if (elementDatas.Length <= 0)
            {
                return;
            }

            IRunnable settingElementData = Services.Execute.Create();
            for (int i = 0; i < elementDatas.Length; i++)
            {
                ElementData elementData = elementDatas[i];
                ElementData oldElementData = GetElementData(elementData.element);
                if (elementData.element == Element.None)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(elementData.model) || string.IsNullOrEmpty(elementData.texture))
                {
                    Services.Console.WriteErrorFormat("bad element data:{0}", Newtonsoft.Json.JsonConvert.SerializeObject(elementData));
                    continue;
                }

                if (oldElementData != null && oldElementData.model == elementData.model)
                {
                    settingElementData.Then(OnSetElementTexture, elementData);
                    continue;
                }
                if (oldElementData != null && oldElementData.model == elementData.model)
                {
                    settingElementData.Then(OnSetElementTexture, elementData);
                    continue;
                }
                
                settingElementData.Then(CreateSettingElementModelRunnable, elementData);
                settingElementData.Then(OnSetElementTexture, elementData);
            }

            settingElementData.Then(Services.Events.Notice, EventNames.SET_ELEMENT_DATA_COMPLATED, Array.Empty<object>());
            Services.Execute.Runner(settingElementData);
        }

        IEnumerator CreateSettingElementModelRunnable(ElementData elementData)
        {
            IRunnable<IResContext> modelLoadRunnable = Services.Resource.LoadAssetAsync(GetAssetBundleAddressable(elementData.model));
            modelLoadRunnable.Then(OnLoadElementModelCompleted, elementData);
            yield return modelLoadRunnable.Execute();
        }

        IEnumerator OnSetElementTexture(ElementData elementData)
        {
            string url = elementData.texture.StartsWith("http") ? elementData.texture : GetDefaultTextureAddressable(elementData.texture);
            IRunnable<IResContext> elementTextureLoadRunnable = Services.Resource.LoadAssetAsync(url);
            elementTextureLoadRunnable.Then(OnLoadElementTextureCompleted, elementData);
            yield return elementTextureLoadRunnable.Execute();
        }

        void OnLoadElementModelCompleted(IResContext context, ElementData elementData)
        {
            if (context == null || !context.EnsureSuccessful())
            {
                Services.Console.WriteError("element model :" + elementData.element + " not find");
                return;
            }

            Services.Console.WriteLine("set element model:" + elementData.element);
            GameObject elementModle = context.GetObject<GameObject>();
            this.builder.SetElementModle(elementData.element, elementModle);
            this.config.SetElementData(elementData);
        }

        void OnLoadElementTextureCompleted(IResContext context, ElementData elementData)
        {
            if (context == null || !context.EnsureSuccessful())
            {
                Services.Console.WriteError("element texture :" + elementData.element + " not find:" + elementData.texture);
                return;
            }

            Texture texture = context.GetObject<Texture2D>(this.builder.GetElementObject(elementData.element));
            Services.Console.WriteLine("set element texture:" + elementData.element);
            this.builder.SetElementTexture(elementData.element, texture);
            this.config.SetElementData(elementData);
            _skeleton.ToViewCenter();
        }

        /// <summary>
        /// 清理指定部件
        /// </summary>
        /// <param name="element"></param>
        public void ClearElement(Element element)
        {
            this.EnsureInitializeInterface();
            this.builder.DestoryElement(element);
            this.config.RemoveElement(element);
            Services.Events.Notice(EventNames.CLEAR_ELMENT_DATA_COMPLATED, (int)element);
        }

        /// <summary>
        /// 导出配置
        /// </summary>
        /// <param name="configName"></param>
        /// <returns></returns>
        public string ExportConfig(string configName)
        {
            this.EnsureInitializeInterface();
            this.config.name = configName;
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(this.config);
            Services.Events.Notice(EventNames.EXPORT_AVATAR_CONFIG_COMPLATED, json);
            return json;
        }

        /// <summary>
        /// 导入配置
        /// </summary>
        /// <param name="config"></param>
        /// <exception cref="Exception"></exception>
        public void ImportConfig(string config)
        {
            this.EnsureInitializeInterface();
            Debug.Assert(string.IsNullOrEmpty(config) == false, "the config is not be null");
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
            Services.Events.Notice(EventNames.IMPORT_CONFIG_COMPLATED);
        }

        /// <summary>
        /// 预览部件
        /// </summary>
        /// <param name="element">部件位置</param>
        /// <param name="texturePath">贴图路径</param>
        public void PreviewElement(Element element, byte[] bytes)
        {
            if (element == Element.None)
            {
                _skeleton.ToViewCenter();
                return;
            }

            this.EnsureInitializeInterface();
            if (bytes != null && bytes.Length > 0)
            {
                Texture2D texture = new Texture2D(512, 512);
                texture.LoadImage(bytes);
                this.builder.SetElementTexture(element, texture);
            }

            ShowInView(element);
        }

        /// <summary>
        /// 上传部件资源
        /// </summary>
        /// <param name="element"></param>
        /// <param name="filePath"></param>
        public void UploadElementAsset(Element element, string name, byte[] bytes)
        {
            this.EnsureInitializeInterface();
            Debug.Assert(bytes != null && bytes.Length > 0, "cannot be upload empty data");
            ElementData elementData = GetElementData(element);
            string modleName = string.IsNullOrEmpty(name) ? Path.GetFileNameWithoutExtension(elementData.model) + bytes.GetMd5() : name;
            WebService.UploadAssetResponse upload_icon_response = null;
            WebService.UploadAssetResponse upload_texture_response = null;
            Texture2D texture = null;

            IEnumerator Runnable_GenericIcon()
            {
                PreviewElement(element, bytes);
                ShowInView(element);
                yield return new WaitForEndOfFrame();
                RenderTexture rt = iconCamera.targetTexture;
                RenderTexture.active = rt;
                RenderTexture.active = iconCamera.targetTexture;
                texture = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
                texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                texture.Apply();
                _skeleton.ToViewCenter();
                SetElementData(elementData);
                Runnable_UploadIconAsset();
            }

            void Runnable_UploadIconAsset()
            {
                string fileName = element.ToString() + "_" + modleName + "_icon.png";
                byte[] iconBytes = texture.EncodeToPNG();
                WebService.RequestCreateFileData requestCreateTextureFileData = new WebService.RequestCreateFileData(fileName, iconBytes.GetMd5(), "image/png", "2", bytes.Length);
                Services.MonoBehaviour.StartCoroutine(WebService.UploadAsset(address, requestCreateTextureFileData, iconBytes, (response, exception) =>
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
                WebService.RequestCreateFileData requestCreateTextureFileData = new WebService.RequestCreateFileData(fileName, bytes.GetMd5(), "image/png", "2", bytes.Length);
                Services.MonoBehaviour.StartCoroutine(WebService.UploadAsset(address, requestCreateTextureFileData, bytes, (response, exception) =>
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
                Services.Events.Notice(EventNames.UPLOAD_ELEMENT_ASSET_COMPLATED, Newtonsoft.Json.JsonConvert.SerializeObject(createElementData));
            }

            Services.MonoBehaviour.StartCoroutine(Runnable_GenericIcon());
        }

        /// <summary>
        /// 合并Avatar
        /// </summary>
        public void Combine()
        {
            this.EnsureInitializeInterface();
            this.builder.Combine();
            this.builder.Clear();
#if UNITY_WEBGL
            Services.Events.Notice(EventNames.COMBINE_AVATAR_COMPLATED);
#else
            Services.Events.Notice(EventNames.COMBINE_AVATAR_COMPLATED, this.gameObject);
#endif
        }

        public void UnCombine()
        {
            this.builder.UnCombine();
        }

        public void DisableElement(Element element)
        {
            this.builder.DisableElement(element);
        }

        public void EnableElement(Element element)
        {
            this.builder.EnableElement(element);
        }

        public void ShowInView(Element element)
        {
            GameObject gameObject = this.builder.GetElementObject(element);
            if (gameObject == null)
            {
                this.gameObject.ToViewCenter();
                return;
            }

            gameObject.ToViewCenter();
        }
    }
}