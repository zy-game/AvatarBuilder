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
        /// �¼�
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
                OnNotiflyEvent(evtName, (string)evtData);
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
        /// ��ʼ���༭��
        /// </summary>
        /// <param name="args">��ʼ������</param>
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
        /// ������
        /// </summary>
        /// <param name="element"></param>
        public void ClearElement(int element)
        {
            this.EnsureAvatarInitialized();
            avatar.ClearElement((Element)element);
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="configName">�����������</param>
        public void ExportConfig(string configName)
        {
            this.EnsureAvatarInitialized();
            avatar.ExportConfig(configName);

        }
        /// <summary>
        /// ��ȡ��������
        /// </summary>
        /// <param name="element">��λö��</param>
        public void GetElementData(int element)
        {
            this.EnsureAvatarInitialized();
            avatar.GetElementData((Element)element);
        }
        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="config">��������</param>
        public void ImportConfig(string config)
        {
            this.EnsureAvatarInitialized();
            Debug.Log("import avatar config:" + config);
            avatar.ImportConfig(config);
        }

        /// <summary>
        /// ���ò���ģ��
        /// </summary>
        /// <param name="elementData">��λ����</param>
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
        /// �ϲ�Avatar
        /// </summary>
        public void Combine()
        {
            this.EnsureAvatarInitialized();
            Debug.Log("combined avatar data");
            avatar.Combine();
        }
        /// <summary>
        /// �ϴ�������Դ
        /// </summary>
        /// <param name="element">����λ��</param>
        /// <param name="fileDataString">�ļ�����</param>
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
        /// Ԥ��
        /// </summary>
        /// <param name="element">��λö��</param>
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
        /// ���ز�λ
        /// </summary>
        /// <param name="element"></param>
        public void DisableElement(Element element)
        {
            this.EnsureAvatarInitialized();
            this.avatar.DisableElement(element);
        }

        /// <summary>
        /// ��ʾ��λ
        /// </summary>
        /// <param name="element"></param>
        public void EnableElement(Element element)
        {
            this.EnsureAvatarInitialized();
            this.avatar.EnableElement(element);
        }

        /// <summary>
        /// ����Ϳѻ
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
        /// �˳�Ϳѻ
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
        /// ����Ϳѻ����
        /// </summary>
        /// <param name="url">���ݵ�ַ�����Ϊ����򿪱����ļ������</param>
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
        /// ����Ϳѻ����
        /// </summary>
        /// <param name="name">�ļ���,����ļ�Ϊ���򲻱�������</param>
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
        /// ����Ϳѻ
        /// </summary>
        public void PublishGraffiti(string name)
        {
            this.EnsureInitializedGraffiti();
            this.graffiti.Publishing(name);
        }

        /// <summary>
        /// ���û���
        /// </summary>
        /// <param name="brush">1:�ֱ� 2:ˢ�� 3:��Ƥ�� 4:�϶�</param>
        public void SetPaintbrushType(int brush)
        {
            this.EnsureInitializedGraffiti();
            this.graffiti.SetPaintbrushType((PaintBrush)brush);
        }

        /// <summary>
        /// ���û�����ɫ
        /// </summary>
        /// <param name="hexadecimal">��ɫֵ</param>
        public void SetPaintbrushColor(string hexadecimal)
        {
            this.EnsureInitializedGraffiti();
            this.graffiti.SetPaintbrushColor(hexadecimal.ToColor());
        }

        /// <summary>
        /// ���û��ʴ�С
        /// </summary>
        /// <param name="width"></param>
        public void SetPaintbrushWidth(float width)
        {
            this.EnsureInitializedGraffiti();
            this.graffiti.SetPaintbrushWidth(width);
        }

        /// <summary>
        /// �ڵ�ǰѡ�е�ͼ���е���ͿѻͼƬ
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
        /// �½�ͼ��
        /// </summary>
        /// <param name="name">ͼ����</param>
        public void NewLayer(string name)
        {
            this.EnsureInitializedGraffiti();
            this.graffiti.NewLayer(name);
        }

        /// <summary>
        /// ѡ��ͼ��
        /// </summary>
        /// <param name="name">Ҫѡ�е�ͼ����</param>
        public void SelectionLayer(string name)
        {
            this.EnsureInitializedGraffiti();
            this.graffiti.SelectionLayer(name);
        }

        /// <summary>
        /// ɾ��ѡ��ͼ��
        /// </summary>
        public void DeleteLayer()
        {
            this.EnsureInitializedGraffiti();
            this.graffiti.DeleteLayer();
        }

        /// <summary>
        /// ����ѡ��ͼ��͸����
        /// </summary>
        /// <param name="alpha">͸���� </param>
        public void SetLayerAlpha(float alpha)
        {
            this.EnsureInitializedGraffiti();
            this.graffiti.SetLayerAlpha(alpha);
        }

        /// <summary>
        /// ����ͼ�����Ŵ�С
        /// </summary>
        /// <param name="size"></param>
        public void SetLayerSize(float size)
        {
            this.EnsureInitializedGraffiti();
            this.graffiti.ResizeLayer(size);
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="isBackup">true:���ˣ�flase:ǰ��</param>
        public void Undo(bool isBackup)
        {
            this.EnsureInitializedGraffiti();
            this.graffiti.UndoRecord(isBackup);
        }
    }
}
