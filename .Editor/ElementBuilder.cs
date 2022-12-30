#if UNITY_EDITOR
namespace AvatarBuild.Editor
{
    using AvatarBuild.Config;
    using AvatarBuild.Resource;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using Unity.EditorCoroutines.Editor;


    internal class CopyChildPath
    {
        [MenuItem("GameObject/Copy Path", false, 2)]
        public static void CopyPath()
        {
            if (Selection.activeGameObject == null)
            {
                return;
            }
            string path = Selection.activeGameObject.name;
            Transform parent = Selection.activeGameObject.transform.parent;
            while (parent != null)
            {
                if (parent != null)
                {
                    path = parent.name + "/" + path;
                }
                parent = parent.parent;
            }
            Debug.Log(path);
            TextEditor text = new TextEditor();
            text.text = path;
            text.SelectAll();
            text.Copy();
        }
    }

    internal class ElementBuilder : EditorWindow
    {
        [MenuItem("Builder/Element Builder")]
        public static void OpenBuilder()
        {
            GetWindow<ElementBuilder>(false, "Element Builder", true);
        }
        private Vector2 point = Vector2.zero;
        private ElemetConfig elemetConfig;
        private Element current;
        private string[] pages;


        private void SaveConfig()
        {
            File.WriteAllText("Assets/ElementConfig.ini", Newtonsoft.Json.JsonConvert.SerializeObject(elemetConfig));
            AssetDatabase.Refresh();
        }

        public void OnGUI()
        {
            if (elemetConfig == null)
            {
                string configPath = "Assets/ElementConfig.ini";
                if (!File.Exists(configPath))
                {
                    elemetConfig = new ElemetConfig();
                    elemetConfig.elements = new List<ElementItem>();
                    SaveConfig();
                }
                else
                {
                    elemetConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<ElemetConfig>(File.ReadAllText(configPath));
                }
                pages = Enum.GetNames(typeof(Element));
            }
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Server Address:", GUILayout.Width(100));
            elemetConfig.address = GUILayout.TextField(elemetConfig.address, GUILayout.Width(200));
            if (GUI.changed)
            {
                if (!elemetConfig.address.EndsWith("/"))
                {
                    elemetConfig.address += "/";
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.Label("Show Type");
            current = (Element)EditorGUILayout.EnumPopup(current, EditorStyles.toolbarPopup);
            if (GUILayout.Button("Save", EditorStyles.toolbarButton))
            {
                SaveConfig();
            }
            if (GUILayout.Button("Upload All", EditorStyles.toolbarButton))
            {
                UploadConfig(elemetConfig.elements.ToArray());
            }
            GUILayout.EndHorizontal();
            if (elemetConfig == null || elemetConfig.elements == null)
            {
                elemetConfig = null;
                return;
            }

            point = EditorGUILayout.BeginScrollView(point, EditorStyles.helpBox);
            Rect rect = EditorGUILayout.BeginVertical();
            for (int i = 0; i < elemetConfig.elements.Count; i++)
            {
                if (current != elemetConfig.elements[i].element && current != Element.None)
                {
                    continue;
                }
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.BeginVertical(GUILayout.Height(100));
                GUILayout.Label(elemetConfig.elements[i].path);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Element Type:", GUILayout.Width(100));
                elemetConfig.elements[i].element = (Element)EditorGUILayout.EnumPopup(elemetConfig.elements[i].element, GUILayout.Width(200));

                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                elemetConfig.elements[i].isNormal = GUILayout.Toggle(elemetConfig.elements[i].isNormal, "Normal");
                if (GUILayout.Button("Upload Element", GUILayout.Width(100)))
                {
                    //GetMeshDatas(elemetConfig.elements[i].path);
                    UploadConfig(elemetConfig.elements[i]);
                }
                GUILayout.EndVertical();
                UnityEngine.Object texture = AssetDatabase.LoadAssetAtPath(elemetConfig.elements[i].path, typeof(UnityEngine.Object));
                EditorGUILayout.ObjectField(AssetPreview.GetAssetPreview(texture), typeof(Texture2D), false, GUILayout.Height(100), GUILayout.Width(100));
                GUILayout.EndHorizontal();
            }
            if (Event.current.type == EventType.DragUpdated && rect.Contains(Event.current.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            }

            if (Event.current.type == EventType.DragPerform && rect.Contains(Event.current.mousePosition))
            {
                for (int i = 0; i < DragAndDrop.paths.Length; i++)
                {
                    if (Path.GetExtension(DragAndDrop.paths[i]) == string.Empty)
                    {
                        string[] files = Directory.GetFiles(DragAndDrop.paths[i], "*.*", SearchOption.AllDirectories);
                        for (int j = 0; j < files.Length; j++)
                        {
                            if (files[j].EndsWith(".meta") || !files[i].EndsWith(".prefab"))
                            {
                                continue;
                            }
                            elemetConfig.elements.Add(new ElementItem() { path = files[j], element = current });
                        }
                    }
                    else
                    {
                        elemetConfig.elements.Add(new ElementItem() { path = DragAndDrop.paths[i], element = current });
                    }
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        private void UploadConfig(params ElementItem[] elements)
        {
            AssetBundleBuild[] assetBundles = new AssetBundleBuild[elements.Length];
            for (int i = 0; i < elements.Length; i++)
            {
                assetBundles[i] = new AssetBundleBuild();
                assetBundles[i].assetBundleName = Path.GetFileNameWithoutExtension(elements[i].path) + ".assetbundle";
                assetBundles[i].assetNames = new string[] { elements[i].path };
            }
            string outputPath = Application.dataPath + "/../Temp/output";
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            BuildPipeline.BuildAssetBundles(outputPath, assetBundles, BuildAssetBundleOptions.None,
#if UNITY_STANDALONE
                BuildTarget.StandaloneWindows
#elif UNITY_WEBGL
                BuildTarget.WebGL
#elif UNITY_ANDROID
                BuildTarget.Android
#elif UNITY_IPHONE
                BuildTarget.iOS
#endif
                );

            bool[] upload_state = new bool[elements.Length];
            Dictionary<ElementItem, Exception> responses = new Dictionary<ElementItem, Exception>();
            int index = 0;
            void StartUpload()
            {
                upload_state[index] = false;
                ElementItem element = elements[index];
                float progress = upload_state.Where(x => x == true).Count() / (float)upload_state.Length;
                EditorUtility.DisplayProgressBar("Upload", "Waiting upload element:" + element.path, progress);
                UploadAsset(outputPath, element, exception =>
                {
                    upload_state[index] = true;
                    float progress = upload_state.Where(x => x == true).Count() / (float)upload_state.Length;
                    EditorUtility.DisplayProgressBar("Upload", "Upload element Successfly:" + element.path, progress);
                    if (exception != null)
                    {
                        responses.Add(element, exception);
                    }
                    index++;
                    if (index < elements.Length)
                    {
                        StartUpload();
                        return;
                    }
                    EditorUtility.ClearProgressBar();
                    if (responses.Count <= 0)
                    {
                        EditorUtility.DisplayDialog("Tips", "Upload Element Successfly!", "OK");
                    }
                    else
                    {
                        foreach (var item in responses)
                        {
                            Debug.LogErrorFormat("{0}\n{1}", item.Key.path, exception);
                        }
                        EditorUtility.DisplayDialog("Tips", "Uploading element succeeded, but some failed. Please check the console for details", "OK");
                    }
                    Directory.Delete(outputPath, true);
                });
            }
            StartUpload();
        }
        private void UploadAsset(string outputpath, ElementItem element, Action<Exception> callback)
        {
            string path = Path.Combine(outputpath, Path.GetFileNameWithoutExtension(element.path) + ".assetbundle");
            if (!File.Exists(path))
            {
                return;
            }
            byte[] bytes = Array.Empty<byte>();
            string fileName = Path.GetFileNameWithoutExtension(element.path);
            HttpUtlis.UploadAssetResponse upload_modle_reponse = null;
            HttpUtlis.UploadAssetResponse upload_icon_response = null;
            HttpUtlis.UploadAssetResponse upload_texture_reponse = null;
            HttpUtlis.UploadAssetResponse upload_zip_response = null;

            void Runnable_UploadAssetZip()
            {
                bytes = GetZipDatas(element.path);
                HttpUtlis.RequestCreateFileData createAssetZip = new HttpUtlis.RequestCreateFileData(fileName + ".zip", ResourceManager.GetFileMd5(bytes), "zip", "2", bytes.Length);
                this.StartCoroutine(HttpUtlis.UploadAsset(elemetConfig.address, createAssetZip, bytes, (response, exception) =>
                 {
                     if (exception != null)
                     {
                         callback(exception);
                         return;
                     }
                     upload_zip_response = response;
                     Runnable_UploadAssetBundle();
                 }));
            }
            void Runnable_UploadAssetBundle()
            {
                byte[] bytes = File.ReadAllBytes(element.path);
                HttpUtlis.RequestCreateFileData requestCreateModleFileData = new HttpUtlis.RequestCreateFileData(fileName + ".assetbundle", ResourceManager.GetFileMd5(bytes), "assetbundle", "2", bytes.Length);
                this.StartCoroutine(HttpUtlis.UploadAsset(elemetConfig.address, requestCreateModleFileData, bytes, (response, exception) =>
                 {
                     if (exception != null)
                     {
                         callback(exception);
                         return;
                     }
                     upload_modle_reponse = response;
                     Runnable_UploadElementIcon();
                 }));
            }
            void Runnable_UploadElementIcon()
            {
                Texture2D texture = AssetPreview.GetAssetPreview(AssetDatabase.LoadAssetAtPath(element.path, typeof(UnityEngine.Object)));
                if (texture != null)
                {
                    bytes = texture.EncodeToPNG();
                }
                if (bytes == null || bytes.Length <= 0)
                {
                    callback(new Exception("load icon file failur,please check the file is exist!"));
                    return;
                }
                HttpUtlis.RequestCreateFileData requestCreateIconFileData = new HttpUtlis.RequestCreateFileData(fileName + "_icon.png", ResourceManager.GetFileMd5(bytes), "image/png", "2", bytes.Length);
                this.StartCoroutine(HttpUtlis.UploadAsset(elemetConfig.address, requestCreateIconFileData, bytes, (response, exception) =>
                 {
                     if (exception != null)
                     {
                         callback(exception);
                         return;
                     }
                     upload_icon_response = response;
                     Runnable_UploadDefaultTexture();
                 }));
            }
            void Runnable_UploadDefaultTexture()
            {
                bytes = GetTextureDatas(element.path);
                HttpUtlis.RequestCreateFileData requestCreateTextureFileData = new HttpUtlis.RequestCreateFileData(fileName + "_default.png", ResourceManager.GetFileMd5(bytes), "image/png", "2", bytes.Length);
                this.StartCoroutine(HttpUtlis.UploadAsset(elemetConfig.address, requestCreateTextureFileData, bytes, (response, exception) =>
                 {
                     if (exception != null)
                     {
                         callback(exception);
                         return;
                     }
                     upload_texture_reponse = response;
                     Runnable_AvatarElementData();
                 }));
            }
            void Runnable_AvatarElementData()
            {
                ElementData elementData = new ElementData();
                elementData.name = fileName + "_default";
                elementData.element = element.element;
                elementData.icon = upload_icon_response?.data.url;
                elementData.texture = upload_texture_reponse?.data.url;
                elementData.model = upload_modle_reponse?.data.url;
                this.StartCoroutine(HttpUtlis.CreateElementRequest(elemetConfig.address, elementData, (response, exception) =>
                {
                    if (exception != null)
                    {
                        callback(exception);
                        return;
                    }
                    if (response.code != 200)
                    {
                        callback(exception != null ? exception : new Exception(response.msg));
                        return;
                    }
                    callback(null);
                }));
            }
            Runnable_UploadAssetZip();
        }

        byte[] GetTextureDatas(string path)
        {
            byte[] bytes = null;
            GameObject temp = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            MeshRenderer meshRenderer = temp.GetComponentInChildren<MeshRenderer>(true);
            Texture2D texture = null;
            if (meshRenderer == null)
            {
                SkinnedMeshRenderer skinned = temp.GetComponentInChildren<SkinnedMeshRenderer>(true);
                texture = (Texture2D)skinned.sharedMaterial.mainTexture;
                string texturePath = AssetDatabase.GetAssetPath(texture);
                bytes = File.ReadAllBytes(texturePath);
            }
            else
            {
                texture = (Texture2D)meshRenderer.sharedMaterial.mainTexture;
                string texturePath = AssetDatabase.GetAssetPath(texture);
                bytes = File.ReadAllBytes(texturePath);
            }
            return bytes;
        }


        byte[] GetMeshDatas(string path)
        {
            GameObject temp = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            MeshFilter meshRenderer = temp.GetComponentInChildren<MeshFilter>(true);
            Mesh mesh = null;
            if (meshRenderer != null)
            {
                mesh = meshRenderer.sharedMesh;
            }
            else
            {
                SkinnedMeshRenderer skinned = temp.GetComponentInChildren<SkinnedMeshRenderer>(true);
                mesh = skinned.sharedMesh;
            }
            return File.ReadAllBytes(AssetDatabase.GetAssetPath(mesh));
        }

        byte[] GetZipDatas(string path)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            string zipPath = Application.dataPath + "/../" + fileName + ".zip";
            byte[] bytes = null;
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }
            using (FileStream zipToOpen = new FileStream(zipPath, FileMode.OpenOrCreate))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    ZipArchiveEntry fbx_entry = archive.CreateEntry(fileName + ".fbx");
                    using (Stream writer = fbx_entry.Open())
                    {
                        bytes = GetMeshDatas(path);
                        writer.Write(bytes, 0, bytes.Length);
                    }
                    ZipArchiveEntry png_entry = archive.CreateEntry(fileName + ".png");
                    using (Stream writer = png_entry.Open())
                    {
                        bytes = GetTextureDatas(path);
                        writer.Write(bytes, 0, bytes.Length);
                    }
                }
            }
            bytes = File.ReadAllBytes(zipPath);
            File.Delete(zipPath);
            return bytes;
        }
    }
}
#endif