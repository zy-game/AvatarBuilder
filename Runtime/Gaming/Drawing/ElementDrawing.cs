namespace Gaming.Drawing
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using System.IO;
    using Gaming.Event;
    using Gaming.Avatar;
    using Gaming.Extension;
    using Gaming.Transport;

    class CacheData
    {
        public string name;
        public Texture2D next;
        public Texture2D prev;
    }

    public sealed class ElementDrawing : IDrawing
    {
        private int cacheId;
        private Camera camera;
        private Element element;
        private float paintSize;
        private IBuilder builder;
        private IAvatar avatar;
        private PaintBrush paintType;
        private List<DrawingData> layers;
        private Stack<CacheData> cacheings;
        private Stack<CacheData> waitingRemove;


        private CacheData cache;
        private Texture2D original;
        private DrawingData current;
        private Texture2D tempCache;
        private RenderTexture render;
        private Texture2D emptyTexture;
        private Texture2D paintTexture;
        private Changed _changed = Changed.Saved;
        private Vector2 startPoint = Vector2.zero;

        public Changed changed
        {
            get
            {
                return _changed;
            }
        }
        public GameObject gameObject
        {
            get;
            private set;
        }

        public ElementDrawing()
        {
            this.layers = new List<DrawingData>();
            this.cacheings = new Stack<CacheData>();
            this.waitingRemove = new Stack<CacheData>();
        }

        /// <summary>
        /// 初始化涂鸦
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="contorller"></param>
        /// <param name="paint"></param>
        /// <param name="element"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public bool Initialized(Camera camera, IAvatar contorller, Texture2D paint, Element element = Element.None, byte[] bytes = null)
        {
            this.camera = camera;
            this.avatar = contorller;
            this.paintTexture = paint;
            this.builder = contorller.builder;
            bool state = false;
            if (bytes == null)
            {
                this.element = element;
                state = GnericGraffitiObject();
            }
            else
            {
                MemoryStream stream = new MemoryStream(bytes);
                BinaryReader reader = new BinaryReader(stream);
                this.element = (Element)reader.ReadByte();
                state = GnericGraffitiObject();
                if (state != false)
                {
                    byte layerCount = reader.ReadByte();
                    for (int j = 0; j < layerCount; j++)
                    {
                        DrawingData layer = new(string.Empty);
                        layer.ReadData(reader);
                        layers.Add(layer);
                        GamingService.Events.Notice(EventNames.CREATE_LAYER_COMPLETED, layer.name);
                    }
                    current = layers.LastOrDefault();
                    Apply();
                    GamingService.Events.Notice(EventNames.IMPORT_GRAFFITI_DATA_COMPLETED);
                }
            }
            if (state)
            {
                GamingService.Events.Notice(EventNames.GRAFFITI_INITIALIZED_COMPLETED);
                GameObject.Find("Canvas/Drawing/RawImage").GetComponent<UnityEngine.UI.RawImage>().texture = render;
            }
            return state;
        }

        private bool GnericGraffitiObject()
        {
            gameObject = builder.GetElementObject(element);
            if (gameObject == null)
            {
                GamingService.Events.Notice(EventNames.MESSAGE_NOTICE, "not find the element object");
                return false;
            }
            original = builder.GetElementTexture(element);
            //emptyTexture = new Texture2D(original.width, original.height, TextureFormat.RGBA32, false);
            //emptyTexture.SetPixels(new Color[original.width * original.height]);
            //emptyTexture.Apply();
            render = new RenderTexture(original.width, original.height, 0);
            render.name = "default_texture";
            render.DrawTexture(new Rect(0, 0, original.width, original.height), original);
            builder.SetElementTexture(element, render);
            this.avatar.UnCombine();
            this.avatar.DisableElement(Element.None);
            this.avatar.EnableElement(element);
            this.avatar.EnableElement(Element.BaseModel);
            this.avatar.EnableElement(Element.Eyebrows);
            this.avatar.EnableElement(Element.Nose);
            this.avatar.EnableElement(Element.Eyes);
            this.avatar.EnableElement(Element.Mouth);
            this.avatar.EnableElement(Element.Head);
            this.avatar.EnableElement(Element.Shoes);
            gameObject.GenericMeshCollider();
            this.avatar.gameObject.ToCameraCenter();
            gameObject.ToCameraCenter();
            MonoBehaviourInstance.AddUpdate(OnUpdate);
            this.SetPaintbrushColor(Color.red);
            this.SetPaintbrushType(PaintBrush.Pen);
            this.SetPaintbrushWidth(16);
            return true;
        }

        private void EnsureSelectionLayer()
        {
            if (current == null)
            {
                throw new Exception("not selection layer");
            }
        }

        private void OnUpdate()
        {
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.LeftControl) || current == null)
            {
                return;
            }

            this.CheckMouseButtonDown();
            this.CheckMouseButtonUp();
            this.CheckMouseButtonDrag();
        }

        private void CheckMouseButtonDown()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
                {
                    if (this.paintType == PaintBrush.Drag)
                    {
                        startPoint = hit.textureCoord;
                        startPoint.x *= current.width;
                        startPoint.y *= current.height;
                    }
                    OnStartDrawing();
                }
            }
        }

        private void CheckMouseButtonUp()
        {
            if (Input.GetMouseButtonUp(0))
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit _))
                {
                    OnDrawingCompleted();
                }
            }
        }

        private void CheckMouseButtonDrag()
        {
            if (current == null)
            {
                return;
            }
            if (!Input.GetMouseButton(0))
            {
                return;
            }
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                if (this.paintType == PaintBrush.Drag)
                {
                    OnDragLayer(hit);
                    return;
                }
                OnDrawingLayer(hit);
            }
        }

        /// <summary>
        /// 拖动图层
        /// </summary>
        /// <param name="hit"></param>
        private void OnDragLayer(RaycastHit hit)
        {
            this._changed = Changed.WaitSave;
            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x *= current.width;
            pixelUV.y *= current.height;
            Vector2 offset = pixelUV - startPoint;
            startPoint = pixelUV;
            current.Drag(offset);
            Apply();
        }

        /// <summary>
        /// 绘画
        /// </summary>
        /// <param name="hit"></param>
        private void OnDrawingLayer(RaycastHit hit)
        {
            EnsureSelectionLayer();
            if (this.waitingRemove.Count > 0)
            {
                this.waitingRemove.Clear();
            }
            current.Drawing(hit.textureCoord.x, hit.textureCoord.y, paintType, this.paintTexture, paintSize);
            Apply();
        }


        private void OnStartDrawing()
        {
            cache = new CacheData();
            cache.name = current.name;
            RenderTexture.active = current.texture;
            cache.prev = new Texture2D(current.width, current.height, TextureFormat.ARGB32, false);
            cache.prev.name = current.name + "_" + cacheId++;
            cache.prev.ReadPixels(new Rect(0, 0, current.width, current.height), 0, 0);
            cache.prev.Apply();
            RenderTexture.active = null;
        }

        private void OnDrawingCompleted()
        {
            if (cache == null)
            {
                return;
            }
            if (this.cacheings.Count > 10)
            {
                this.cacheings.Reverse();
                this.cacheings.Pop();
                this.cacheings.Reverse();
            }
            RenderTexture.active = current.texture;
            cache.next = new Texture2D(current.width, current.height, TextureFormat.ARGB32, false);
            cache.next.name = current.name + "_" + cacheId++;
            cache.next.ReadPixels(new Rect(0, 0, current.width, current.height), 0, 0);
            cache.next.Apply();
            RenderTexture.active = null;
            this.cacheings.Push(cache);
            cache = null;
        }

        private void Apply()
        {
            render.Clear();
            render.DrawTexture(new Rect(0, 0, original.width, original.height), original);
            for (int i = 0; i < layers.Count; i++)
            {
                render.DrawTexture(new Rect(0, 0, layers[i].width, layers[i].height), layers[i].texture);
            }
            this._changed = Changed.WaitSave;
        }

        public void Publishing(string name)
        {
            RenderTexture.active = current.texture;
            Texture2D publishTexture = new Texture2D(current.width, current.height, TextureFormat.ARGB32, false);
            publishTexture.ReadPixels(new Rect(0, 0, current.width, current.height), 0, 0);
            publishTexture.Apply();
            RenderTexture.active = null;
            avatar.UploadElementAsset(element, name, publishTexture.EncodeToPNG());
        }

        public void Save(string name)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((byte)element);
            writer.Write((byte)layers.Count);
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].WriteData(writer);
            }
            IEnumerator Runnable_UploadGraffitiData()
            {
                byte[] bytes = stream.ToArray();
                WebService.RequestCreateFileData requestCreateTextureFileData = new WebService.RequestCreateFileData(name + ".png", bytes.GetMd5(), "image/png", "2", bytes.Length);
                yield return WebService.UploadAsset(avatar.address, requestCreateTextureFileData, bytes, (response, exception) =>
                {
                    if (exception != null)
                    {
                        GamingService.Logger.LogError(exception);
                        return;
                    }
                    this._changed = Changed.Saved;
                    GamingService.Events.Notice(EventNames.SAVED_GRAFFITI_DATA_COMPLETED);
                });
            }
            MonoBehaviourInstance.StartCor(Runnable_UploadGraffitiData());
        }

        public void ImportTexture(byte[] textureBytes)
        {
            EnsureSelectionLayer();
            Texture2D texture = new Texture2D(current.width, current.height, TextureFormat.RGBA32, false);
            texture.name = element + " import" + textureBytes.GetMd5();
            texture.LoadImage(textureBytes);
            current.texture.DrawTexture(new Rect(0, 0, current.width, current.height), texture);
            Apply();
            GamingService.Events.Notice(EventNames.IMPORT_GRAFFITI_TEXTURE_COMPLETED);
        }

        public void NewLayer(string name)
        {
            if (layers.Count >= 5)
            {
                GamingService.Events.Notice(EventNames.MESSAGE_NOTICE, "To reach the maximum number of floors");
                return;
            }
            layers.Add(new DrawingData(original.width, original.height, name));
            SelectionLayer(name);
            //current.texture.DrawTexture(emptyTexture);
            Apply();
            GamingService.Events.Notice(EventNames.CREATE_LAYER_COMPLETED, current.name);
        }

        public void ResizeLayer(float size)
        {
            EnsureSelectionLayer();
            OnStartDrawing();
            current.Resize(size);
            OnDrawingCompleted();
            Apply();
        }

        public void DeleteLayer()
        {
            EnsureSelectionLayer();
            if (!layers.Contains(current))
            {
                return;
            }
            string name = current.name;
            layers.Remove(current);
            current = layers.LastOrDefault();
            Apply();
            GamingService.Events.Notice(EventNames.DELETE_LAYER_COMPLETED, name);
        }

        public void SelectionLayer(string name)
        {
            DrawingData layer = layers.Find(x => x.name == name);
            if (layer == null)
            {
                GamingService.Events.Notice(EventNames.MESSAGE_NOTICE, "not find the layer");
                return;
            }
            current = layer;

        }

        public void SetLayerAlpha(float alpha)
        {
            EnsureSelectionLayer();
            current.SetAlpha(alpha);
        }

        public void SetPaintbrushColor(Color color)
        {
            Color[] newPaintColors = paintTexture.GetPixels();
            for (int i = 0; i < newPaintColors.Length; i++)
            {
                newPaintColors[i].r = color.r;
                newPaintColors[i].g = color.g;
                newPaintColors[i].b = color.b;
                newPaintColors[i].a = color.a;
            }
            this.paintTexture.SetPixels(newPaintColors);
            this.paintTexture.Apply();
        }

        public void SetPaintbrushType(PaintBrush brush)
        {
            this.paintType = brush;
        }

        public void SetPaintbrushWidth(float width)
        {
            this.paintSize = width;
        }

        public void UndoRecord(bool isBackup)
        {
            if (isBackup)
            {
                if (this.cacheings.Count <= 0)
                {
                    return;
                }
                CacheData cache = this.cacheings.Pop();
                this.waitingRemove.Push(cache);
                UndoLayer(cache, isBackup);
                return;
            }
            if (!isBackup)
            {
                if (this.waitingRemove.Count <= 0)
                {
                    return;
                }
                CacheData cache = this.waitingRemove.Pop();
                this.cacheings.Push(cache);
                UndoLayer(cache, isBackup);
            }
            void UndoLayer(CacheData cache, bool forward)
            {
                DrawingData layer = layers.Find(x => x.name == cache.name);
                if (layer == null)
                {
                    return;
                }
                Texture2D record = forward == false ? cache.next : cache.prev;
                current.texture.DrawTexture(new Rect(0, 0, record.width, record.height), record);
                Apply();
            }
        }

        public void Dispose()
        {
            MonoBehaviourInstance.RemoveUpdate(OnUpdate);
            this.builder.GetElementObject(element).DestroyMeshCollider();
            this.builder.SetElementTexture(element, original);
            this.avatar.EnableElement(element: Element.None);
            this.builder.Combine();
        }
    }
}
