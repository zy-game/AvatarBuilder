namespace Gaming.Editor
{
    using Gaming.Avatar;
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    internal static class Extension
    {
        public static void Save(this EditorWindow window)
        {
            window.SaveChanges();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static string GetObjectUniqueName(string defaultName, string ext, params string[] folders)
        {
            string[] exists = AssetDatabase.FindAssets(defaultName, folders);
            for (int i = 0; i < exists.Length; i++)
            {
                exists[i] = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(exists[i]));
                Debug.Log(exists[i]);
            }
            return ObjectNames.GetUniqueName(exists, defaultName) + ext;
        }
        public static string DrawingRowTextField(this EditorWindow window, string name, string text, int nameWidth = 100, int valueWidth = 200)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, GUILayout.Width(nameWidth));
            text = GUILayout.TextField(text, GUILayout.Width(valueWidth));
            GUILayout.EndHorizontal();
            return text;
        }

        public static Enum DrawingEnumPopup(this EditorWindow window, string name, Enum field, int nameWidth = 100, int valueWidth = 200, GUIStyle style = null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, GUILayout.Width(nameWidth));
            if (style != null)
            {
                field = EditorGUILayout.EnumPopup(field, style, GUILayout.Width(valueWidth));
            }
            else
            {
                field = EditorGUILayout.EnumPopup(field, GUILayout.Width(valueWidth));
            }
            GUILayout.EndHorizontal();
            return field;
        }



        public static string DrawingArraryPopup(this EditorWindow window, string name, string[] array, int index, int nameWidth = 100, int valueWidth = 200, GUIStyle style = null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, GUILayout.Width(nameWidth));
            if (style != null)
            {
                index = EditorGUILayout.Popup(index, array, style, GUILayout.Width(valueWidth));
            }
            else
            {
                index = EditorGUILayout.Popup(index, array, GUILayout.Width(valueWidth));
            }
            GUILayout.EndHorizontal();
            return array[index];
        }

        public static void DrawingLabel(this EditorWindow window, string name, string text, int nameWidth = 100, int valueWidth = -1)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, GUILayout.Width(nameWidth));
            if (valueWidth == -1)
            {
                GUILayout.Label(text);
            }
            else
            {
                GUILayout.Label(text, GUILayout.Width(valueWidth));
            }
            GUILayout.EndHorizontal();
        }

        public static bool DrawingToggle(this EditorWindow window, string name, bool value, int nameWidth = 100, int valueWidth = 200)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, GUILayout.Width(nameWidth));
            value = GUILayout.Toggle(value, "", GUILayout.Width(valueWidth));
            GUILayout.EndHorizontal();
            return value;
        }

        public static T DrawingObject<T>(this EditorWindow window, string name, UnityEngine.Object value, int nameWidth = 100, int valueWidth = 100, int valueHeight = 100) where T : UnityEngine.Object
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, GUILayout.Width(nameWidth));
            value = (T)EditorGUILayout.ObjectField(value, typeof(T), false, GUILayout.Width(valueWidth), GUILayout.Height(valueHeight));
            GUILayout.EndHorizontal();
            return (T)value;
        }

        public static string TryCreateDirectory(string path, bool isDelete = false)
        {
            if (!string.IsNullOrEmpty(Path.GetExtension(path)))
                path = Path.GetDirectoryName(path);
            if (Directory.Exists(path))
            {
                if (!isDelete)
                {
                    return path;
                }
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);
            return path;
        }

        public static void CopyDirectory(string scrPath, string desPath)
        {
            desPath = Extension.TryCreateDirectory(desPath);
            string[] files = Directory.GetFiles(scrPath);
            for (int i = 0; i < files.Length; i++)
            {
                string destPath = desPath + Path.GetFileName(files[i]);
                if (files[i].EndsWith(".meta"))
                {
                    continue;
                }
                File.Copy(files[i], destPath, true);
            }
        }

        public static void SetParent(this GameObject gameObject, GameObject target, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            if (gameObject == null)
            {
                return;
            }
            if (target != null)
            {
                gameObject.transform.SetParent(target.transform);
            }
            gameObject.transform.position = position;
            gameObject.transform.rotation = Quaternion.Euler(rotation);
            gameObject.transform.localScale = scale;
        }

        public static void SaveTexture2D(this Texture2D texture, string path)
        {
            Services.File.Delete(path);
            Services.File.WriteData(path, texture.EncodeToPNG());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(path);
            TextureImporterPlatformSettings textureImporterPlatformSettings = textureImporter.GetDefaultPlatformTextureSettings();
            textureImporter.isReadable = true;
            textureImporter.mipmapEnabled = false;
            textureImporter.alphaIsTransparency = true;
            textureImporterPlatformSettings.maxTextureSize = 512;
            textureImporterPlatformSettings.compressionQuality = 100;
            textureImporterPlatformSettings.format = TextureImporterFormat.Automatic;
            textureImporterPlatformSettings.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static string SavePrefab(this GameObject gameObject)
        {
            GameObject temp = GameObject.Instantiate<GameObject>(gameObject);
            string path = Path.GetDirectoryName(gameObject.GetInEditorAssetPath()) + "/" + Path.GetFileNameWithoutExtension(gameObject.name) + ".prefab";
            GameObject asset = PrefabUtility.SaveAsPrefabAsset(temp, path);
            GameObject.DestroyImmediate(temp);
            return GetInEditorAssetPath(asset);
        }

        public static string GetInEditorAssetPath(this GameObject gameObject)
        {
            return AssetDatabase.GetAssetPath(gameObject);
        }
    }
}
