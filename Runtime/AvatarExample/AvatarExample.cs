using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using Gaming.Avatar;
using Gaming.Drawing;
using Gaming.Event;

namespace Example
{
    public class AvatarExample : MonoBehaviour
    {
        public WebGLAvatar avatar;
        public bool InternalNetwork;
        private List<string> layers = new List<string>();

        public void Start()
        {
            List<string> array = Enum.GetNames(typeof(Element)).ToList();

            Gaming.Services.Events.Register(EventNames.GRAFFITI_INITIALIZED_COMPLETED, OnEntryGraffiti);
            Gaming.Services.Events.Register(EventNames.CREATE_LAYER_COMPLETED, OnNewDrawingLayer);
            Gaming.Services.Events.Register(EventNames.DELETE_LAYER_COMPLETED, OnDeleteDrawingLayer);
            InitializedDropdown("graffiti", array, avatar.ElementGraffiti);
            InitializedDropdown("perview", array, avatar.PreviewAsset);
            InitializedDropdown("upload", array, avatar.UploadAsset);

            List<string> paints = Enum.GetNames(typeof(PaintBrush)).ToList();
            InitializedDropdown("Drawing/paint", paints, args => { avatar.SetPaintbrushType(args + 1); });
            transform.Find("Drawing/LayerSize").GetComponent<Slider>().onValueChanged.AddListener(avatar.SetLayerSize);
            transform.Find("Drawing/PaintSize").GetComponent<Slider>().onValueChanged.AddListener(avatar.SetPaintbrushWidth);
            transform.Find("Drawing").gameObject.SetActive(false);


            InitData initData = new()
            {
                skeleton = "https://avatar-oss.oss-cn-beijing.aliyuncs.com/6/9/girl/webgl/mesh_girl_001_skeleton.assetbundle",
                address = InternalNetwork ? "http://192.168.199.88:3456/" : "http://139.9.0.36/",
                config = Resources.Load<TextAsset>("bones").text
            };
            Gaming.Services.Events.Register(EventNames.INITIALIZED_COMPLATED_EVENT, Runnable_Initialized);
            avatar.Initialized(Newtonsoft.Json.JsonConvert.SerializeObject(initData));
        }

        private void OnDeleteDrawingLayer(object args)
        {
            if (args == null)
            {
                return;
            }

            if (!layers.Contains(args.ToString()))
            {
                return;
            }

            layers.Remove(args.ToString());
            InitializedDropdown("Drawing/layers", layers, args => { avatar.SelectionLayer(layers[args]); });
        }

        private void OnEntryGraffiti(object args)
        {
            transform.Find("Drawing").gameObject.SetActive(true);
        }

        private void OnNewDrawingLayer(object args)
        {
            if (args == null)
            {
                return;
            }

            if (layers.Contains(args.ToString()))
            {
                return;
            }

            layers.Add(args.ToString());
            InitializedDropdown("Drawing/layers", layers, args => { avatar.SelectionLayer(layers[args]); });
        }

        void Runnable_Initialized(object args)
        {
            Gaming.Services.Events.Unregister(EventNames.INITIALIZED_COMPLATED_EVENT, Runnable_Initialized);
            StartCoroutine(Gaming.Transport.WebService.RequestPublishingElementDatas(avatar.avatar.address, Runnable_ResponsePublishingElementDatas));
        }

        void Runnable_ResponsePublishingElementDatas(List<Gaming.Config.ElementData> elements, Exception exception)
        {
            if (exception != null)
            {
                return;
            }

            avatar.SetElementData(Newtonsoft.Json.JsonConvert.SerializeObject(elements));
        }

        void InitializedDropdown(string name, List<string> items, UnityEngine.Events.UnityAction<int> callback)
        {
            Dropdown component = this.transform.Find(name).GetComponent<Dropdown>();
            component.ClearOptions();
            component.AddOptions(items);
            component.onValueChanged.RemoveAllListeners();
            component.onValueChanged.AddListener(callback);
        }

        public void Undo()
        {
            avatar.Undo(this.transform.Find("Drawing/Toggle").GetComponent<Toggle>().isOn ? 0 : 1);
        }

        public void Save()
        {
            void SaveGraffiti(object args)
            {
                Gaming.Services.Events.Unregister(EventNames.NOTICE_SAVED_GRAFFITI_DATA, SaveGraffiti);
                avatar.Save("testGraffiti");
            }

            Gaming.Services.Events.Register(EventNames.NOTICE_SAVED_GRAFFITI_DATA, SaveGraffiti);
            avatar.ExitGraffiti();
        }

        public void Delete()
        {
            avatar.DeleteLayer();
        }

        public void NewLayer()
        {
            avatar.NewLayer(Guid.NewGuid().ToString());
        }

        public void Import()
        {
            avatar.ImportGraffitiTexture();
        }

        public void ImportConfig()
        {
            avatar.ImportGraffitiData("https://avatar-oss.oss-cn-beijing.aliyuncs.com/12/test/testGraffiti_1673524427.png");
        }
    }
}