using System;
using UnityEngine;
using System.Text;
using System.Security.Cryptography;
using System.Globalization;
using System.IO;

namespace Gaming.Extension
{
    public static class Utility
    {
        /// <summary>
        /// 计算文件的MD5值
        /// </summary>
        public static string GetMd5(this byte[] bytes)
        {
            try
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(bytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("md5file() fail, error:" + ex.Message);
            }
        }

        public static string GetMd5(this string info)
        {
            return GetMd5(Encoding.UTF8.GetBytes(info));
        }

        public static Color ToColor(this string hex)
        {
            hex = hex.Replace("0x", string.Empty);
            hex = hex.Replace("#", string.Empty);
            byte a = byte.MaxValue;
            byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
            }

            return new Color32(r, g, b, a);
        }

        public static MeshCollider GenericMeshCollider(this GameObject gameObject)
        {
            if (gameObject == null)
            {
                return default;
            }

            SkinnedMeshRenderer skinned = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
            MeshCollider collider = null;
            if (skinned == null)
            {
                Renderer renderer = gameObject.GetComponent<Renderer>();
                collider = renderer.gameObject.AddComponent<MeshCollider>();
                collider.sharedMesh = renderer.GetComponent<MeshFilter>().sharedMesh;
                gameObject.SetActive(true);
            }
            else
            {
                collider = skinned.gameObject.AddComponent<MeshCollider>();
                collider.sharedMesh = skinned.sharedMesh;
            }

            return collider;
        }

        public static void DestroyMeshCollider(this GameObject gameObject)
        {
            if (gameObject == null)
            {
                return;
            }

            MeshCollider[] colliders = gameObject.GetComponentsInChildren<MeshCollider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                GameObject.DestroyImmediate(colliders[i]);
            }
        }

        public static Texture2D Screenshot(this Camera camera, int width, int height, GameObject gameObject)
        {
            camera.ToViewCenter(gameObject);
            camera.ToViewCenter(gameObject);
            RenderTexture renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.Default);
            camera.targetTexture = renderTexture;
            RenderTexture.active = camera.targetTexture;
            camera.Render();
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.name = gameObject.name.Replace("(Clone)", "");
            texture.Apply();
            RenderTexture.active = null;
            camera.targetTexture = null;
            return texture;
        }

        public static void ToViewCenter(this GameObject gameObject)
        {
            Camera[] cameras = GameObject.FindObjectsOfType<Camera>();
            var bound = gameObject.GetBoundingBox();
            var center = bound.center;
            var extents = bound.extents;
            if (cameras == null)
            {
                return;
            }

            for (int i = 0; i < cameras.Length; i++)
            {
                cameras[i].transform.LookAt(center, Vector3.up);
                cameras[i].transform.position = new Vector3(center.x, center.y, center.z + 10);
                cameras[i].fieldOfView = SetOrthCameraSize(cameras[i], center.x - extents.x, center.x + extents.x, center.y - extents.y, center.y + extents.y, 6);
            }
        }

        public static void ToViewCenter(this Camera camera, GameObject gameObject)
        {
            if (camera == null || gameObject == null)
            {
                return;
            }


            Renderer renderer = gameObject.GetComponentInChildren<Renderer>();
            var bound = renderer.transform.parent.gameObject.GetBoundingBox();

            float radiusToSizeFactor = 2;
            float radiusToDistanceFactor = 2;

            // 根据参数设置相机到物体的距离。
            camera.transform.localPosition = new Vector3(0, 0, -radiusToDistanceFactor * bound.extents.x);
            // 如果需要显示的物体不是正对相机中心，修改这个向量的初值。
            Vector3 worldPos = new Vector3(0, bound.extents.x, 0);
            // 把向量转换到世界坐标系，因为相机的接口只接受世界坐标
            worldPos = gameObject.transform.TransformPoint(worldPos);
            // 获得点在相机屏幕中的相对位置，如正好在屏幕左下角的点返回(0, 0, z)，正好在右上角的点返回(1, 1, z)。z为物体离相机的深度距离，在这里没有用处。
            Vector3 viewPos = camera.WorldToViewportPoint(worldPos);
            // 根据参数调整相机范围。这里我用的是正交相机，如果使用透视相机，这里应该修改fieldOfView。
            // 如果参数为2，则物体最多占到屏幕的1/2高度。
            camera.fieldOfView *= radiusToSizeFactor * viewPos.y;
            // var center = bound.center;
            // var extents = bound.extents;
            // camera.transform.LookAt(center, Vector3.up);
            // camera.transform.position = new Vector3(center.x, center.y, center.z + 10);
            // camera.fieldOfView = SetOrthCameraSize(camera, center.x - extents.x, center.x + extents.x, center.y - extents.y, center.y + extents.y, 7);
        }

        private static float SetOrthCameraSize(Camera camera, float xmin, float xmax, float ymin, float ymax, float scale)
        {
            float xDis = xmax - xmin;
            float yDis = ymax - ymin;
            float sizeX = xDis / camera.aspect;
            float sizeY = yDis;
            return sizeX >= sizeY ? sizeX * scale : sizeY * scale;
        }


        public static void DrawTexture(this RenderTexture renderTexture, Rect rect, Texture texture)
        {
            RenderTexture.active = renderTexture;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, renderTexture.width, renderTexture.height, 0);
            //GL.LoadPixelMatrix(0, renderTexture.width, 0, renderTexture.height);
            Graphics.DrawTexture(rect, texture /*, useOffset, 0, 0, 0, 0*/);
            GL.PopMatrix();
            RenderTexture.active = null;
        }

        public static void Clear(this RenderTexture renderTexture)
        {
            RenderTexture.active = renderTexture;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, renderTexture.width, renderTexture.height, 0);
            //GL.LoadPixelMatrix(0, renderTexture.width, 0, renderTexture.height);
            GL.Clear(false, true, Color.clear);
            GL.PopMatrix();
            RenderTexture.active = null;
        }

        /// <summary>
        /// 获取物体包围盒
        /// </summary>
        /// <param name="obj">父物体</param>
        /// <returns>物体包围盒</returns>
        public static Bounds GetBoundingBox(this GameObject obj)
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

        public static Color ReadColor(this BinaryReader reader)
        {
            Color color = Color.white;
            color.r = reader.ReadByte() / 255f;
            color.g = reader.ReadByte() / 255f;
            color.b = reader.ReadByte() / 255f;
            color.a = reader.ReadByte() / 255f;
            return color;
        }

        public static void Write(this BinaryWriter writer, Color color)
        {
            writer.Write((byte)(color.r * 255f));
            writer.Write((byte)(color.g * 255f));
            writer.Write((byte)(color.b * 255f));
            writer.Write((byte)(color.a * 255f));
        }


        public static void CopyTo(this Texture2D texture, Texture2D dest)
        {
            dest.SetPixels(texture.GetPixels());
            dest.Apply();
        }

        //双线性放大与缩小
        public static Texture2D TextureScaleFormat(Texture2D originalTexture, float scaleFactor)
        {
            Texture2D newTexture = new Texture2D(Mathf.CeilToInt(originalTexture.width * scaleFactor), Mathf.CeilToInt(originalTexture.height * scaleFactor));
            float scale = 1.0f / scaleFactor;
            int maxX = originalTexture.width - 1;
            int maxY = originalTexture.height - 1;
            for (int y = 0; y < newTexture.height; y++)
            {
                for (int x = 0; x < newTexture.width; x++)
                {
                    // Bilinear Interpolation
                    float targetX = x * scale;
                    float targetY = y * scale;
                    int x1 = Mathf.Min(maxX, Mathf.FloorToInt(targetX));
                    int y1 = Mathf.Min(maxY, Mathf.FloorToInt(targetY));
                    int x2 = Mathf.Min(maxX, x1 + 1);
                    int y2 = Mathf.Min(maxY, y1 + 1);

                    float u = targetX - x1;
                    float v = targetY - y1;
                    float w1 = (1 - u) * (1 - v);
                    float w2 = u * (1 - v);
                    float w3 = (1 - u) * v;
                    float w4 = u * v;
                    Color color1 = originalTexture.GetPixel(x1, y1);
                    Color color2 = originalTexture.GetPixel(x2, y1);
                    Color color3 = originalTexture.GetPixel(x1, y2);
                    Color color4 = originalTexture.GetPixel(x2, y2);
                    Color color = new Color(Mathf.Clamp01(color1.r * w1 + color2.r * w2 + color3.r * w3 + color4.r * w4),
                        Mathf.Clamp01(color1.g * w1 + color2.g * w2 + color3.g * w3 + color4.g * w4),
                        Mathf.Clamp01(color1.b * w1 + color2.b * w2 + color3.b * w3 + color4.b * w4),
                        Mathf.Clamp01(color1.a * w1 + color2.a * w2 + color3.a * w3 + color4.a * w4)
                    );
                    newTexture.SetPixel(x, y, color);
                }
            }

            newTexture.Apply();
            return newTexture;
        }

        public static void TextureColorScaleFormat(Color[] colors, int width, int height, float scaleFactor)
        {
            int newWidth = Mathf.CeilToInt(width * scaleFactor);
            int newHeight = Mathf.CeilToInt(height * scaleFactor);
            int maxX = width - 1;
            int maxY = height - 1;
            Color[] temp = new Color[newWidth * newHeight];
            float scale = 1.0f / scaleFactor;
            for (int i = 0; i < newHeight; i++)
            {
                for (int j = 0; j < newWidth; j++)
                {
                    float targetY = i * scale;
                    float targetX = j * scale;
                    int x1 = Mathf.Min(maxX, Mathf.FloorToInt(targetX));
                    int y1 = Mathf.Min(maxY, Mathf.FloorToInt(targetY));
                    int x2 = Mathf.Min(maxX, x1 + 1);
                    int y2 = Mathf.Min(maxY, y1 + 1);

                    float u = targetX - x1;
                    float v = targetY - y1;
                    float w1 = (1 - u) * (1 - v);
                    float w2 = u * (1 - v);
                    float w3 = (1 - u) * v;
                    float w4 = u * v;
                    Color color1 = colors[y1 * width + x1]; // originalTexture.GetPixel(x1, y1);
                    Color color2 = colors[y1 * width + x2]; // originalTexture.GetPixel(x2, y1);
                    Color color3 = colors[y2 * width + x1]; //originalTexture.GetPixel(x1, y2);
                    Color color4 = colors[y2 * width + x2]; //originalTexture.GetPixel(x2, y2);
                    Color color = new Color(Mathf.Clamp01(color1.r * w1 + color2.r * w2 + color3.r * w3 + color4.r * w4),
                        Mathf.Clamp01(color1.g * w1 + color2.g * w2 + color3.g * w3 + color4.g * w4),
                        Mathf.Clamp01(color1.b * w1 + color2.b * w2 + color3.b * w3 + color4.b * w4),
                        Mathf.Clamp01(color1.a * w1 + color2.a * w2 + color3.a * w3 + color4.a * w4)
                    );
                    temp[i * newWidth + j] = color;
                }
            }

            Array.Clear(colors, 0, colors.Length);
            for (int i = 0; i < newHeight; i++)
            {
                int point = ((height / 2 - newHeight / 2 + i) * width) + (width / 2 - newWidth / 2);
                Array.Copy(temp, i * newWidth, colors, point, newWidth);
                //temp.Slice(i * newWidth, newWidth).CopyTo(colors.Slice(point, newWidth));
            }
        }
    }
}