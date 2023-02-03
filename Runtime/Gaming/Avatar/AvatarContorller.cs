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

    public sealed class AvatarContorller : IAvatar
    {
        private string _address;
        private IBuilder _builder;
        private Camera iconCamera;
        private Camera mainCamera;
        private AvatarConfig config;
        private GameObject _skeleton;
        private IResourceLoader loader;
        private float nowCamEulerX;
        private Vector3 mouseLeapPose;
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
            get
            {
                return _address;
            }
        }
        public IBuilder builder
        {
            get
            {
                return _builder;
            }
        }

        public GameObject gameObject
        {
            get
            {
                return _skeleton;
            }
        }
        public AvatarContorller() : this(null) { }

        public AvatarContorller(Camera mainCamera) : this(mainCamera, null) { }

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
            MonoBehaviourInstance.RemoveUpdate(RotationScreenView);
            MonoBehaviourInstance.RemoveUpdate(MovementScreenView);
            MonoBehaviourInstance.RemoveUpdate(ScaleScreenView);
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
        public void Initialize<T>(string skeleton, string address) where T : IResourceLoader, new()
        {
            this._address = address;
            this.loader = new T();

            loader.LoadAssetAsync(skeleton, handle =>
            {

                _skeleton = handle.GetObject<GameObject>();
                if (_skeleton == null)
                {
                    throw new Exception("basic skeleton not find");
                }
                _skeleton.transform.localScale = Vector3.one;
                _skeleton.transform.rotation = Quaternion.Euler(Vector3.zero);
                _skeleton.transform.position = Vector3.zero;
                _skeleton.GetComponent<Animator>().enabled = false;
                MonoBehaviourInstance.AddUpdate(RotationScreenView);
                MonoBehaviourInstance.AddUpdate(MovementScreenView);
                MonoBehaviourInstance.AddUpdate(ScaleScreenView);
                GamingService.Events.Notice(EventNames.INITIALIZED_COMPLATED_EVENT);
            });
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

        /// <summary>
        /// 设置部位模型
        /// </summary>
        /// <param name="element"></param>
        /// <param name="assetName"></param>
        public void SetElementData(params ElementData[] elementDatas)
        {
            EnsureInitializeInterface();
            if (elementDatas == null || elementDatas.Length <= 0)
            {
                return;
            }
            bool[] state = new bool[elementDatas.Length];
            for (int i = 0; i < elementDatas.Length; i++)
            {
                ChangeElementData(i, elementDatas[i]);
            }
            void ChangeElementData(int index, ElementData elementData)
            {
                if (string.IsNullOrEmpty(elementData.model) || string.IsNullOrEmpty(elementData.texture))
                {
                    Debug.LogError("bad element data:" + Newtonsoft.Json.JsonConvert.SerializeObject(elementData));
                    CheckCompleted(index);
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
                    //Debug.Log("set element model:" + elementData.model);
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
                        CheckCompleted(index);
                        return;
                    }
                    //Debug.Log("set element texture:" + elementData.model);
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
                        _skeleton.ToCameraCenter();
                        CheckCompleted(index);
                    });
                }
                SetModle();
            }
            void CheckCompleted(int index)
            {
                state[index] = true;
                if (state.Where(x => x == false).Count() > 0)
                {
                    return;
                }
                GamingService.Events.Notice(EventNames.SET_ELEMENT_DATA_COMPLATED);
            }
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
            GamingService.Events.Notice(EventNames.CLEAR_ELMENT_DATA_COMPLATED, (int)element);
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
            GamingService.Events.Notice(EventNames.EXPORT_AVATAR_CONFIG_COMPLATED, json);
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
            GamingService.Events.Notice(EventNames.IMPORT_CONFIG_COMPLATED);
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
                _skeleton.ToCameraCenter();
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
                _skeleton.ToCameraCenter();
                SetElementData(elementData);
                Runnable_UploadIconAsset();
            }
            void Runnable_UploadIconAsset()
            {
                string fileName = element.ToString() + "_" + modleName + "_icon.png";
                byte[] iconBytes = texture.EncodeToPNG();
                WebService.RequestCreateFileData requestCreateTextureFileData = new WebService.RequestCreateFileData(fileName, iconBytes.GetMd5(), "image/png", "2", bytes.Length);
                MonoBehaviourInstance.StartCor(WebService.UploadAsset(address, requestCreateTextureFileData, iconBytes, (response, exception) =>
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
                MonoBehaviourInstance.StartCor(WebService.UploadAsset(address, requestCreateTextureFileData, bytes, (response, exception) =>
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
                GamingService.Events.Notice(EventNames.UPLOAD_ELEMENT_ASSET_COMPLATED, Newtonsoft.Json.JsonConvert.SerializeObject(createElementData));
            }
            MonoBehaviourInstance.StartCor(Runnable_GenericIcon());
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
            GamingService.Events.Notice(EventNames.COMBINE_AVATAR_COMPLATED);
#else
            GamingService.Events.Notice(EventNames.COMBINE_AVATAR_COMPLATED, basic);
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
                return;
            }
            gameObject.ToCameraCenter();
        }
    }
}