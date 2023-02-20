using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;
using Gaming.Avatar;
using Gaming.Drawing;
using Gaming.Extension;
using Gaming.Config;
using Gaming.Event;

namespace Example
{
    class InitData
    {
        public string address;
        public string skeleton;
        public string config;
    }

    public class WebGLAvatar : MonoBehaviour
    {
        public IAvatar avatar;
        private IDrawing graffiti;
        private bool isInitialized = false;
        public Camera iconCamera;
        public Texture2D paint;

        /// <summary>
        /// 事件
        /// </summary>
        [DllImport("__Internal")]
        private static extern void OnNotiflyEvent(string name, string data);


        [DllImport("__Internal")]
        private static extern void OnSelectionFile();

        public void OpenFileCallback(string blob)
        {
            IEnumerator Runnable_ReadingFile()
            {
                UnityWebRequest request = UnityWebRequest.Get(blob);
                yield return request.SendWebRequest();
                if (request.isDone == false || request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("reading file failur:" + blob);
                }
                else
                {
                    byte[] bytes = request.downloadHandler.data;
                    Debug.LogFormat("read file successfly:{0} length:{1}", blob, bytes.Length);
                    Gaming.Services.Events.Notice(Gaming.Event.EventNames.OPEN_FILE_COMPLATED, bytes);
                }
            }

            Gaming.Services.MonoBehaviour.StartCoroutine(Runnable_ReadingFile());
        }

        class WebGLEventDispatcher : IEventDispatch
        {
            public void Dispose()
            {
            }

            public void Notifly(string evtName, object evtData)
            {
                if (evtName == EventNames.OPEN_FILE_COMPLATED)
                {
                    return;
                }

                if (evtData == null)
                {
                    OnNotiflyEvent(evtName, "");
                    return;
                }

                OnNotiflyEvent(evtName, evtData.ToString());
            }
        }

        private void InitializedEventScheduler()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Gaming.Services.Events.Subscribe<WebGLEventDispatcher>();
#endif
        }

        private void EnsureAvatarInitialized()
        {
            if (avatar != null && isInitialized)
            {
                return;
            }

            throw new Exception("the avatar is not initialized");
        }

        private void EnsureInitializedGraffiti()
        {
            this.EnsureAvatarInitialized();
            if (this.graffiti != null)
            {
                return;
            }

            throw new Exception("not initialized graffiti");
        }

        /// <summary>
        /// 初始化编辑器
        /// </summary>
        /// <param name="args">初始化参数</param>
        public void Initialized(string args)
        {
            this.InitializedEventScheduler();

            void InitializedComplatedCallback(object args)
            {
                this.isInitialized = true;
                Gaming.Services.Events.Unregister(EventNames.INITIALIZED_COMPLATED_EVENT, InitializedComplatedCallback);
            }

            Gaming.Services.Events.Register(EventNames.INITIALIZED_COMPLATED_EVENT, InitializedComplatedCallback);
            Gaming.Services.Resource.SetResourceLoader<WebGLAssetLoader>();
            InitData initData = Newtonsoft.Json.JsonConvert.DeserializeObject<InitData>(args);
            BonesConfig.instance.Initialized(config: initData.config);
            avatar = new AvatarContorller(Camera.main, iconCamera);
            avatar.Initialize(initData.skeleton, initData.address);
        }

        /// <summary>
        /// 清理部件
        /// </summary>
        /// <param name="element"></param>
        public void ClearElement(int element)
        {
            this.EnsureAvatarInitialized();
            avatar.ClearElement((Element)element);
        }

        /// <summary>
        /// 将部位显示在视图中心
        /// </summary>
        public void ShowInView(int element)
        {
            this.EnsureAvatarInitialized();
            avatar.ShowInView((Element)element);
        }

        /// <summary>
        /// 导出配置
        /// </summary>
        /// <param name="configName">保存的配置名</param>
        public void ExportConfig(string configName)
        {
            this.EnsureAvatarInitialized();
            avatar.ExportConfig(configName);
        }

        /// <summary>
        /// 获取部件数据
        /// </summary>
        /// <param name="element">部位枚举</param>
        public void GetElementData(int element)
        {
            this.EnsureAvatarInitialized();
            avatar.GetElementData((Element)element);
        }

        /// <summary>
        /// 导入配置
        /// </summary>
        /// <param name="config">配置数据</param>
        public void ImportConfig(string config)
        {
            this.EnsureAvatarInitialized();
            Debug.Log("import avatar config:" + config);
            avatar.ImportConfig(config);
        }

        /// <summary>
        /// 设置部件模型
        /// </summary>
        /// <param name="elementData">部位数据</param>
        public void SetElementData(string elementData)
        {
            this.EnsureAvatarInitialized();
            List<ElementData> element = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ElementData>>(elementData);
            if (element == null)
            {
                throw new Exception("element data cannot be null");
            }

            Debug.Log("set avatar element data:" + elementData);
            avatar.SetElementData(element.ToArray());
        }

        /// <summary>
        /// 合并Avatar
        /// </summary>
        public void Combine()
        {
            this.EnsureAvatarInitialized();
            Debug.Log("combined avatar data");
            avatar.Combine();
        }

        /// <summary>
        /// 上传部件资源
        /// </summary>
        /// <param name="element">部件位置</param>
        /// <param name="fileDataString">文件数据</param>
        public void UploadAsset(int element)
        {
            this.EnsureAvatarInitialized();

            void Runnable_OpenFileComplated(object args)
            {
                Gaming.Services.Events.Unregister(EventNames.OPEN_FILE_COMPLATED, Runnable_OpenFileComplated);
                avatar.UploadElementAsset((Element)element, string.Empty, (byte[])args);
            }

            Gaming.Services.Events.Register(EventNames.OPEN_FILE_COMPLATED, Runnable_OpenFileComplated);
            OnSelectionFile();
        }

        /// <summary>
        /// 预览
        /// </summary>
        /// <param name="element">部位枚举</param>
        public void PreviewAsset(int element)
        {
            this.EnsureAvatarInitialized();
#if UNITY_EDITOR
            avatar.PreviewElement((Element)element, null);
#else
            void Runnable_OpenFileComplated(object args)
            {
                Gaming.Services.Events.Unregister(EventNames.OPEN_FILE_COMPLATED, Runnable_OpenFileComplated);
                avatar.PreviewElement((Element)element, (byte[])args);
            }
            Gaming.Services.Events.Register(EventNames.OPEN_FILE_COMPLATED, Runnable_OpenFileComplated);
            OnSelectionFile();
#endif
        }

        /// <summary>
        /// 隐藏部位
        /// </summary>
        /// <param name="element"></param>
        public void DisableElement(int element)
        {
            this.EnsureAvatarInitialized();
            this.avatar.DisableElement((Element)element);
        }

        /// <summary>
        /// 显示部位
        /// </summary>
        /// <param name="element"></param>
        public void EnableElement(int element)
        {
            this.EnsureAvatarInitialized();
            this.avatar.EnableElement((Element)element);
        }

        /// <summary>
        /// 部件涂鸦
        /// </summary>
        /// <param name="element"></param>
        public void ElementGraffiti(int element)
        {
            if (this.graffiti != null)
            {
                Gaming.Services.Events.Notice(EventNames.NOTICE_SAVED_GRAFFITI_DATA);
                return;
            }

            if (paint == null)
            {
                Gaming.Services.Events.Notice(EventNames.MESSAGE_NOTICE, "not find paint data");
                return;
            }

            this.graffiti = new ElementDrawing();
            if (!this.graffiti.Initialized(iconCamera, this.avatar, paint, (Element)element))
            {
                this.graffiti = null;
            }
        }

        /// <summary>
        /// 退出涂鸦
        /// </summary>
        public void ExitGraffiti()
        {
            this.EnsureInitializedGraffiti();
            if (this.graffiti.changed == Changed.WaitSave)
            {
                Gaming.Services.Events.Notice(EventNames.NOTICE_SAVED_GRAFFITI_DATA);
                return;
            }

            this.graffiti.Dispose();
            this.graffiti = null;
        }

        /// <summary>
        /// 导入涂鸦数据
        /// </summary>
        /// <param name="url">数据地址，如果为空则打开本地文件浏览器</param>
        public void ImportGraffitiData(string url)
        {
            if (this.graffiti != null)
            {
                Gaming.Services.Events.Notice(EventNames.NOTICE_SAVED_GRAFFITI_DATA);
                return;
            }

            if (paint == null)
            {
                Gaming.Services.Events.Notice(EventNames.MESSAGE_NOTICE, "not find paint data");
                return;
            }

            void Runnable_OpenFileComplated(object args)
            {
                Gaming.Services.Events.Unregister(EventNames.OPEN_FILE_COMPLATED, Runnable_OpenFileComplated);
                this.graffiti = new ElementDrawing();
                if (!this.graffiti.Initialized(iconCamera, this.avatar, paint, Element.None, (byte[])args))
                {
                    this.graffiti = null;
                }
            }

            Gaming.Services.Events.Register(EventNames.OPEN_FILE_COMPLATED, Runnable_OpenFileComplated);
            if (!string.IsNullOrEmpty(url))
            {
                OpenFileCallback(url);
                return;
            }

            OnSelectionFile();
        }

        /// <summary>
        /// 保存涂鸦数据
        /// </summary>
        /// <param name="name">文件名,如果文件为空则不保存数据</param>
        public void Save(string name)
        {
            this.EnsureInitializedGraffiti();
            if (string.IsNullOrEmpty(name))
            {
                this.graffiti.Dispose();
                this.graffiti = null;
                return;
            }

            this.graffiti.Save(name);
            this.graffiti.Dispose();
            this.graffiti = null;
        }

        /// <summary>
        /// 发布涂鸦
        /// </summary>
        public void PublishGraffiti(string name)
        {
            this.EnsureInitializedGraffiti();
            this.graffiti.Publishing(name);
        }

        /// <summary>
        /// 设置画笔
        /// </summary>
        /// <param name="brush">1:钢笔 2:刷子 3:橡皮檫 4:拖动</param>
        public void SetPaintbrushType(int brush)
        {
            this.EnsureInitializedGraffiti();
            this.graffiti.SetPaintbrushType((PaintBrush)brush);
        }

        /// <summary>
        /// 设置画笔颜色
        /// </summary>
        /// <param name="hexadecimal">颜色值</param>
        public void SetPaintbrushColor(string hexadecimal)
        {
            this.EnsureInitializedGraffiti();
            this.graffiti.SetPaintbrushColor(hexadecimal.ToColor());
        }

        /// <summary>
        /// 设置画笔大小
        /// </summary>
        /// <param name="width"></param>
        public void SetPaintbrushWidth(float width)
        {
            this.EnsureInitializedGraffiti();
            this.graffiti.SetPaintbrushWidth(width);
        }

        /// <summary>
        /// 在当前选中的图层中导入涂鸦图片
        /// </summary>
        public void ImportGraffitiTexture()
        {
            this.EnsureInitializedGraffiti();

            void Runnable_OpenFileComplated(object args)
            {
                Gaming.Services.Events.Unregister(EventNames.OPEN_FILE_COMPLATED, Runnable_OpenFileComplated);
                this.graffiti.ImportTexture((byte[])args);
            }

            Gaming.Services.Events.Register(EventNames.OPEN_FILE_COMPLATED, Runnable_OpenFileComplated);
            OnSelectionFile();
        }

        /// <summary>
        /// 新建图层
        /// </summary>
        /// <param name="name">图层名</param>
        public void NewLayer(string name)
        {
            this.EnsureInitializedGraffiti();
            this.graffiti.NewLayer(name);
        }

        /// <summary>
        /// 选中图层
        /// </summary>
        /// <param name="name">要选中的图层名</param>
        public void SelectionLayer(string name)
        {
            this.EnsureInitializedGraffiti();
            this.graffiti.SelectionLayer(name);
        }

        /// <summary>
        /// 删除选中图层
        /// </summary>
        public void DeleteLayer()
        {
            this.EnsureInitializedGraffiti();
            this.graffiti.DeleteLayer();
        }

        /// <summary>
        /// 设置选中图层透明度
        /// </summary>
        /// <param name="alpha">透明度 </param>
        public void SetLayerAlpha(float alpha)
        {
            this.EnsureInitializedGraffiti();
            this.graffiti.SetLayerAlpha(alpha);
        }

        /// <summary>
        /// 设置图层缩放大小
        /// </summary>
        /// <param name="size"></param>
        public void SetLayerSize(float size)
        {
            this.EnsureInitializedGraffiti();
            this.graffiti.ResizeLayer(size);
        }

        /// <summary>
        /// 撤销
        /// </summary>
        /// <param name="isBackup">0:后退，1:前进</param>
        public void Undo(int isBackup)
        {
            this.EnsureInitializedGraffiti();
            this.graffiti.UndoRecord(isBackup == 0);
        }
    }
}