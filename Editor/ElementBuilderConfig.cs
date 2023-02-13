#if UNITY_EDITOR
namespace Gaming.Editor
{
    using UnityEditor;
    using UnityEngine;
    using System.Collections.Generic;
    using System.IO;
    using Avatar;
    using System.Linq;
    using Gaming.Extension;
    using UnityEditor.Callbacks;

    public class ElementBuilderConfig : ScriptableObject
    {
        [SerializeField]
        public GameObject skeleton;

        [SerializeField]
        public Texture2D normal;

        [SerializeField]
        public List<ElementItemData> elements;

        public static ElementBuilderConfig actived { get; set; }
        [OnOpenAssetAttribute(1)]
        public static bool OpenInEditor(int guid, int line)
        {
            Object target = EditorUtility.InstanceIDToObject(guid);
            if (target.GetType() == typeof(ElementBuilderConfig))
            {
                OpenBuilder((ElementBuilderConfig)target);
                return true;
            }
            return false;
        }

        [MenuItem("Game/Editor Element Config")]
        [MenuItem("Assets/Create/Gaming/Create Element Config")]
        public static void CreateElementBuilderConfig()
        {
            ElementBuilderConfig builderConfig = new ElementBuilderConfig();
            string save_path = string.Empty;
            if (Selection.activeGameObject == null)
            {
                EditorUtility.DisplayDialog("Tips", "Please select a skeleton gameobject", "OK");
                return;
            }
            builderConfig.skeleton = Selection.activeGameObject;
            builderConfig.elements = new List<ElementItemData>();
            string path = AssetDatabase.GetAssetPath(Selection.activeGameObject);
            save_path = Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path) + ".asset";
            AssetDatabase.CreateAsset(builderConfig, save_path);
            AssetDatabase.Refresh();
            OpenBuilder(builderConfig);
            EditorUtility.SetDirty(actived);
        }
        public static void OpenBuilder(ElementBuilderConfig builderConfig)
        {
            actived = builderConfig;
            if (actived.elements == null)
            {
                actived.elements = new List<ElementItemData>();
            }
            EditorWindow.GetWindow<ElementBuilder>(false, "Avatar Element Builder", true);
        }

        internal void RemoveSelect()
        {
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                if (!elements[i].isOn)
                {
                    continue;
                }
                elements.Remove(elements[i]);
            }
        }

        public void Clear(bool clearAll)
        {
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                if (!clearAll && !elements[i].isOn)
                {
                    continue;
                }
                elements.Remove(elements[i]);
            }
        }

        internal ElementItemData[] GetSelect()
        {
            List<ElementItemData> items = new List<ElementItemData>();
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                if (!elements[i].isOn)
                {
                    continue;
                }
                items.Add(elements[i]);
            }
            return items.ToArray();
        }

        internal ElementItemData[] GetElementDatas(Element current)
        {
            if (current == Element.None)
            {
                return elements.ToArray();
            }
            return elements.Where(x => x.element == current).ToArray();
        }

        private AssetBundleBuild[] GetElementItemDatas(bool isAll)
        {
            List<AssetBundleBuild> assetBundles = new List<AssetBundleBuild>();
            assetBundles.Add(GetBundleBuild(skeleton));
            for (int i = 1; i < elements.Count; i++)
            {
                if (!isAll && !elements[i].isOn)
                {
                    continue;
                }
                assetBundles.Add(GetBundleBuild(elements[i].fbx));
            }
            return assetBundles.ToArray();
        }

        private AssetBundleBuild GetBundleBuild(GameObject element)
        {
            return new AssetBundleBuild()
            {
                assetNames = new string[] { element.SavePrefab() },
                assetBundleName = Path.GetFileNameWithoutExtension(element.name) + ".assetbundle"
            };
        }

        public void Builder(Camera camera, bool buildAll)
        {
            AssetBundleBuild[] assetBundles = GetElementItemDatas(buildAll);

            string outputPath = Application.dataPath + "/../output/";
            string iconFolder = Extension.TryCreateDirectory(outputPath + "icons/");
            string elementFolder = Extension.TryCreateDirectory(outputPath + "elements/");
            BuildPipeline.BuildAssetBundles(Extension.TryCreateDirectory(outputPath + "android/"), assetBundles, BuildAssetBundleOptions.None, BuildTarget.Android);
            BuildPipeline.BuildAssetBundles(Extension.TryCreateDirectory(outputPath + "windows/"), assetBundles, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
            BuildPipeline.BuildAssetBundles(Extension.TryCreateDirectory(outputPath + "ios/"), assetBundles, BuildAssetBundleOptions.None, BuildTarget.iOS);
            BuildPipeline.BuildAssetBundles(Extension.TryCreateDirectory(outputPath + "webgl/"), assetBundles, BuildAssetBundleOptions.None, BuildTarget.WebGL);

            foreach (var item in elements)
            {
                File.WriteAllBytes(iconFolder + item.name + "_icon.png", File.ReadAllBytes(GetElementIconPath(item.name + "_icon.png")));
                File.WriteAllBytes(elementFolder + item.material.mainTexture.name + ".png", File.ReadAllBytes(AssetDatabase.GetAssetPath(item.material.mainTexture)));
            }

            GameObject root = new GameObject();
            root.SetParent(null, Vector3.zero, Vector3.zero, Vector3.one);
            for (int i = 0; i < elements.Count; i++)
            {
                if (!elements[i].isNormal)
                {
                    continue;
                }
                GameObject item = GameObject.Instantiate<GameObject>(elements[i].fbx);
                item.SetParent(root, Vector3.zero, Vector3.zero, Vector3.one);
            }
            normal = camera.Screenshot(256, 512, root);
            normal.SaveTexture2D(GetElementIconPath(normal.name + "_icon.png"));
            normal = AssetDatabase.LoadAssetAtPath<Texture2D>(GetElementIconPath(normal.name + "_icon.png"));
            GameObject.DestroyImmediate(root);

            File.WriteAllBytes(outputPath + "icons/" + normal.name + "_icon.png", File.ReadAllBytes(AssetDatabase.GetAssetPath(normal)));

            List<OutData> datas = new List<OutData>();
            datas.Add(GetElementDataJson(null));
            foreach (var item in elements)
            {
                datas.Add(GetElementDataJson(item));
            }

            File.WriteAllText(outputPath + "elements.json", Newtonsoft.Json.JsonConvert.SerializeObject(datas));
            for (int i = 0; i < assetBundles.Length; i++)
            {
                if (!File.Exists(assetBundles[i].assetNames[0]))
                {
                    continue;
                }
                AssetDatabase.DeleteAsset(assetBundles[i].assetNames[0]);
            }
        }

        OutData GetElementDataJson(ElementItemData data)
        {
            OutData elementJson = new OutData();
            if (data == null)
            {
                elementJson.element = (int)Element.None;
                elementJson.is_normal = true;
                elementJson.model = skeleton.name + ".assetbundle";
            }
            else
            {
                elementJson.is_normal = data.isNormal;
                elementJson.element = (int)data.element;
                elementJson.model = data.fbx.name + ".assetbundle";
                elementJson.icon = Path.GetFileName(data.name) + "_icon.png";
                elementJson.texture = data.material.mainTexture.name + ".png";
            }
            elementJson.group = skeleton.name;
            return elementJson;
        }

        internal void AddElementData(Camera camera, string path)
        {
            if (path.EndsWith(".meta") || !path.EndsWith(".fbx"))
            {
                return;
            }
            string newElementName = Path.GetFileNameWithoutExtension(path);
            ElementItemData elementItemData = elements.Find(x => x.fbx.name == newElementName);
            if (elementItemData != null)
            {
                elements.Remove(elementItemData);
            }
            ElementItemData item = new ElementItemData();
            item.fbx = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            item.name = Path.GetFileNameWithoutExtension(item.fbx.name);
            item.element = Element.None;
            GameObject temp = GameObject.Instantiate(item.fbx);
            Renderer renderer = temp.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                item.material = renderer.sharedMaterial;
            }
            string texturePath = GetElementIconPath(item.name + "_icon.png");
            camera.Screenshot(512, 512, temp).SaveTexture2D(texturePath);
            item.icon = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            GameObject.DestroyImmediate(temp);
            elements.Add(item);
        }
        private static string GetElementIconPath(string fileName)
        {
            string temp = Path.GetDirectoryName(AssetDatabase.GetAssetPath(actived)) + "/icons/";
            if (!Directory.Exists(temp))
            {
                Directory.CreateDirectory(temp);
            }
            return temp + fileName;
        }
    }
}
#endif