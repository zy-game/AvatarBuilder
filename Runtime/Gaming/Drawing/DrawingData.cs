namespace Gaming.Drawing
{
    using Gaming.Extension;
    using System;
    using System.IO;
    using UnityEngine;

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
                        this._tempCache.SetPixel((int)(x * this.width + j), (int)(y * height + i), Color.clear);
                    }
                }
                this._tempCache.Apply();
                this._texture.Clear();
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
            int length_x = (int)Math.Abs(offset.x);
            if (length_x != 0)
            {
                RenderTexture.active = this._texture;
                if (offset.x < 0)//→
                {
                    this._tempCache.ReadPixels(new Rect(0, 0, length_x, height), width - length_x, 0);
                    this._tempCache.ReadPixels(new Rect(length_x, 0, width - length_x, height), 0, 0);
                }
                else if (offset.x > 0)//←
                {
                    this._tempCache.ReadPixels(new Rect(width - length_x, 0, length_x, height), 0, 0);
                    this._tempCache.ReadPixels(new Rect(0, 0, width - length_x, height), length_x, 0);
                }
            }
            int length_y = (int)Math.Abs(offset.y);
            if (length_y != 0)
            {
                RenderTexture.active = this._texture;
                if (offset.y > 0)//↑
                {
                    this._tempCache.ReadPixels(new Rect(0, 0, width, length_y), 0, 0);
                    this._tempCache.ReadPixels(new Rect(0, length_y, width, height - length_y), 0, length_y);
                }
                else if (offset.y < 0)//↓
                {
                    this._tempCache.ReadPixels(new Rect(0, height - length_y, width, length_y), 0, height - length_y);
                    this._tempCache.ReadPixels(new Rect(0, 0, width, height - length_y), 0, 0);
                }
            }
            if (length_x != 0 || length_y != 0)
            {
                this._tempCache.Apply();
                this._texture.Clear();
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
