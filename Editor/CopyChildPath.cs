#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Gaming.Editor
{
    using UnityEditor;
    using UnityEngine;

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
}

//public class GamingImportProcessor : AssetPostprocessor
//{

//    public void OnPreprocessModel()
//    {
//        ModelImporter importer = this.assetImporter as ModelImporter;
//        importer.meshCompression = ModelImporterMeshCompression.High;
//        importer.optimizeMeshPolygons = true;
//        importer.importTangents = ModelImporterTangents.None;
//        importer.useSRGBMaterialColor = false;
//        importer.animationType = ModelImporterAnimationType.Generic;
//        importer.SaveAndReimport();
//    }

//    public void OnPostprocessModel(GameObject go)
//    {
//    }

//    public void OnPreprocessTexture()
//    {
//        TextureImporter importer = this.assetImporter as TextureImporter;
//        importer.alphaIsTransparency = true;
//        importer.textureType = TextureImporterType.Default;
//        importer.mipmapEnabled = false;
//        importer.isReadable = true;
//        importer.streamingMipmaps = false;
//        importer.vtOnly = false;
//        importer.filterMode = FilterMode.Bilinear;
//        importer.wrapMode = TextureWrapMode.Clamp;
//        importer.GetSourceTextureWidthAndHeight(out int width, out int height);
//        int maxSize = Mathf.Min(width, height) / 2;

//        importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings()
//        {
//            name = "Android",
//            overridden = true,
//            maxTextureSize = maxSize,
//            resizeAlgorithm = TextureResizeAlgorithm.Bilinear,
//            compressionQuality = 50,
//            format = TextureImporterFormat.ETC2_RGBA8,
//            androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings
//        });

//        importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings()
//        {
//            name = "Standalone",
//            overridden = true,
//            maxTextureSize = maxSize,
//            resizeAlgorithm = TextureResizeAlgorithm.Bilinear,
//            compressionQuality = 50,
//            format = TextureImporterFormat.DXT5,
//            androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings
//        });

//        importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings()
//        {
//            name = "iOS",
//            overridden = true,
//            maxTextureSize = maxSize,
//            resizeAlgorithm = TextureResizeAlgorithm.Bilinear,
//            compressionQuality = 50,
//            format = TextureImporterFormat.PVRTC_RGBA4,
//            androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings
//        });

//        importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings()
//        {
//            name = "WebGL",
//            overridden = true,
//            maxTextureSize = maxSize,
//            resizeAlgorithm = TextureResizeAlgorithm.Bilinear,
//            compressionQuality = 50,
//            format = TextureImporterFormat.DXT5,
//            androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings
//        });
//        importer.SaveAndReimport();
//    }

//    public void OnPostprocessTexture(Texture2D tex)
//    {
//    }


//    public void OnPostprocessAudio(AudioClip clip)
//    {

//    }

//    public void OnPreprocessAudio()
//    {
//        AudioImporter audio = this.assetImporter as AudioImporter;
//    }

//    public static void OnPostprocessAllAssets(string[] importedAsset, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
//    {

//    }
//}
#endif