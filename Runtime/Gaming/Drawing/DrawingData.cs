namespace Gaming.Drawing
{
    using Gaming.Extension;
    using System;
    using System.IO;
    using UnityEngine;
    using static Codice.CM.Common.CmCallContext;

    public sealed class DrawingData
    {
        private string _name;
        private bool isChange;
        private Texture2D _tempCache;
        private RenderTexture _texture;

        public DrawingData(string name)
        {
            this.name = name;
        }

        public DrawingData(int width, int height, string name) : this(name)
        {

            this.width = width;
            this.height = height;
            this.isChange = false;
            this._tempCache = new Texture2D(width, height, TextureFormat.RGBA32, false);
            this._tempCache.name = name + "_cache";
            this._texture = new RenderTexture(width, height, 0, RenderTextureFormat.Default);
            this._texture.name = name + "_render";
        }

        public string name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                if (this._texture != null)
                {
                    this._texture.name = value;
                }
                if (this._tempCache != null)
                {
                    this._tempCache.name = value + "_cache";
                }
            }
        }

        public RenderTexture texture
        {
            get
            {
                return this._texture;
            }
        }

        public int width
        {
            get;
            private set;
        }

        public int height
        {
            get;
            private set;
        }

        public void Clear()
        {
            RenderTexture.active = this._texture;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, this.width, this.height, 0);
            GL.Clear(true, true, Color.clear);
            GL.Color(Color.clear);
            GL.PopMatrix();
            RenderTexture.active = null;
        }

        public void SetAlpha(float alpha)
        {

        }

        public void Drawing(float x, float y, PaintBrush paintType, Texture2D paintTexture, float width)
        {
            float burshSize = paintTexture.width / 16f * width;
            RenderTexture.active = texture;
            if (paintType == PaintBrush.Rubber)
            {
                //if (_tempCache == null)
                //{
                //    _tempCache = new Texture2D(this.width, this.height, TextureFormat.ARGB32, false);
                //    _tempCache.ReadPixels(new Rect(0, 0, this.width, height), 0, 0);
                //}
                for (int i = 0; i < (int)burshSize; i++)
                {
                    for (int j = 0; j < (int)burshSize; j++)
                    {
                        _tempCache.SetPixel((int)(x * this.width + j), (int)(y * height + i), Color.clear);
                    }
                }
                _tempCache.Apply();
                this.texture.Clear();
                this._texture.DrawTexture(new Rect(0, 0, this.width, height), _tempCache);
            }
            else
            {
                var _x = (int)(x * texture.width);
                var _y = (int)(height - y * height);
                this._texture.DrawTexture(new Rect(_x, _y, burshSize, burshSize), paintTexture);
            }
            this.isChange = true;
        }
        public void Drag(Vector2 offset)
        {
            int size = (int)Math.Abs(offset.x);
            if (size != 0)
            {
                RenderTexture.active = this.texture;
                Debug.Log("x:" + offset.x);
                if (offset.x < 0)//→
                {
                    this._tempCache.ReadPixels(new Rect(0, 0, size, height), width - size, 0);
                    this._tempCache.ReadPixels(new Rect(size, 0, width - size, height), 0, 0);
                }
                else if (offset.x > 0)//←
                {
                    this._tempCache.ReadPixels(new Rect(width - size, 0, size, height), 0, 0);
                    this._tempCache.ReadPixels(new Rect(0, 0, width - size, height), size, 0);
                }
                this._tempCache.Apply();
                this.texture.Clear();
                this._texture.DrawTexture(new Rect(0, 0, width, height), this._tempCache);
            }
            size = (int)Math.Abs(offset.y);
            if (size != 0)
            {
                RenderTexture.active = this.texture;
                Debug.Log("y:" + offset.y);
                if (offset.y < 0)//↑
                {
                    this._tempCache.ReadPixels(new Rect(0, 0, width, size), 0, height - size);
                    this._tempCache.ReadPixels(new Rect(0, size, width, height - size), 0, 0);
                }
                else if (offset.y > 0)//↓
                {
                    this._tempCache.ReadPixels(new Rect(0, height - size, width, size), 0, 0);
                    this._tempCache.ReadPixels(new Rect(0, 0, width, height - size), 0, size);
                }
                this._tempCache.Apply();
                this.texture.Clear();
                this._texture.DrawTexture(new Rect(0, 0, width, height), this._tempCache);
            }
        }

        internal void Resize(float size)
        {
            int newWidth = (int)(width * size);
            int newHeight = (int)(height * size);
            if (this.isChange == true)
            {
                RenderTexture.active = this.texture;
                this._tempCache.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                this._tempCache.Apply();
                this.isChange = false;
                RenderTexture.active = null;
            }
            this.texture.Clear();
            this.texture.DrawTexture(new Rect((width - newWidth) / 2, (height - newHeight) / 2, newWidth, newHeight), this._tempCache);
        }

        public void WriteData(BinaryWriter writer)
        {
            int length = width * height;
            writer.Write(name);
            writer.Write(width);
            writer.Write(height);
            writer.Write(length);
            RenderTexture.active = this._texture;
            Texture2D temp = new Texture2D(width, height, TextureFormat.RGBA32, false);
            temp.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            temp.Apply();
            Color[] _colors = temp.GetPixels();
            for (int j = 0; j < length; j++)
            {
                writer.Write(_colors[j]);
            }
            RenderTexture.active = null;
        }

        public void ReadData(BinaryReader reader)
        {
            this.name = reader.ReadString();
            this.width = reader.ReadInt32();
            this.height = reader.ReadInt32();
            int length = reader.ReadInt32();
            Color[] _colors = new Color[length];
            for (int i = 0; i < length; i++)
            {
                _colors[i] = reader.ReadColor();
            }
            Texture2D temp = new Texture2D(width, height, TextureFormat.RGBA32, false);
            temp.SetPixels(_colors);
            this._texture.DrawTexture(new Rect(0, 0, width, height), temp);
        }
    }
}
