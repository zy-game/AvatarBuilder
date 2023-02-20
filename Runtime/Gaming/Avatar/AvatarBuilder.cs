namespace Gaming.Avatar
{
    using System;
    using UnityEngine;
    using System.Collections.Generic;
    using Gaming.Config;
    using System.Linq;
    using System.Collections;

    /// <summary>
    /// Avatar ����
    /// </summary>
    public sealed class AvatarBuilder : IBuilder
    {
        private const int COMBINE_TEXTURE_MAX = 512;
        private const string COMBINE_DIFFUSE_TEXTURE = "_MainTex";

        private GameObject _basic;
        private Dictionary<Element, GameObject> allModles;

        private SkinnedMeshRenderer combineSkinnedRenderer;

        //private Dictionary<Element, GameObject> combineModles;
        public AvatarBuilder(GameObject basicModle)
        {
            _basic = basicModle;
            allModles = new Dictionary<Element, GameObject>();
            //combineModles = new Dictionary<Element, GameObject>();
        }

        public void Combine()
        {
            // Fetch all bones of the skeleton
            List<Transform> transforms = new List<Transform>();
            transforms.AddRange(_basic.GetComponentsInChildren<Transform>(true));
            List<SkinnedMeshRenderer> skinneds = _basic.GetComponentsInChildren<SkinnedMeshRenderer>(true).ToList();
            List<Material> materials = new List<Material>();
            List<CombineInstance> combineInstances = new List<CombineInstance>();
            List<Transform> bones = new List<Transform>();
            List<Vector2[]> oldUV = new List<Vector2[]>();
            for (int i = 0; i < skinneds.Count; i++)
            {
                SkinnedMeshRenderer smr = skinneds[i];
                materials.AddRange(smr.materials);
                for (int sub = 0; sub < smr.sharedMesh.subMeshCount; sub++)
                {
                    CombineInstance ci = new CombineInstance();
                    ci.mesh = smr.sharedMesh;
                    ci.subMeshIndex = sub;
                    combineInstances.Add(ci);
                }

                // Collect bones
                for (int j = 0; j < smr.bones.Length; j++)
                {
                    int tBase = 0;
                    for (tBase = 0; tBase < transforms.Count; tBase++)
                    {
                        if (smr.bones[j].name.Equals(transforms[tBase].name))
                        {
                            bones.Add(transforms[tBase]);
                            break;
                        }
                    }
                }
            }

            Material newMaterial = new Material(materials[0].shader);
            List<Texture2D> Textures = new List<Texture2D>();
            for (int i = 0; i < materials.Count; i++)
            {
                Textures.Add(materials[i].GetTexture(COMBINE_DIFFUSE_TEXTURE) as Texture2D);
            }

            Texture2D newDiffuseTex = new Texture2D(COMBINE_TEXTURE_MAX, COMBINE_TEXTURE_MAX, TextureFormat.RGBA32, false);
            newDiffuseTex.name = "combine_texture";
            Rect[] uvs = newDiffuseTex.PackTextures(Textures.ToArray(), 0);
            newMaterial.mainTexture = newDiffuseTex;
            Vector2[] uva, uvb;
            for (int j = 0; j < combineInstances.Count; j++)
            {
                uva = (Vector2[])(combineInstances[j].mesh.uv);
                uvb = new Vector2[uva.Length];
                for (int k = 0; k < uva.Length; k++)
                {
                    uvb[k] = new Vector2((uva[k].x * uvs[j].width) + uvs[j].x, (uva[k].y * uvs[j].height) + uvs[j].y);
                }

                oldUV.Add(combineInstances[j].mesh.uv);
                combineInstances[j].mesh.uv = uvb;
            }

            // Create a new SkinnedMeshRenderer
            SkinnedMeshRenderer oldSKinned = _basic.GetComponent<SkinnedMeshRenderer>();
            if (oldSKinned != null)
            {
                GameObject.DestroyImmediate(oldSKinned);
            }

            combineSkinnedRenderer = _basic.AddComponent<SkinnedMeshRenderer>();
            combineSkinnedRenderer.sharedMesh = new Mesh();
            combineSkinnedRenderer.sharedMesh.name = "runtime_combine_mesh";
            combineSkinnedRenderer.sharedMesh.CombineMeshes(combineInstances.ToArray(), true, false); // Combine meshes
            combineSkinnedRenderer.sharedMesh.UploadMeshData(true);
            combineSkinnedRenderer.bones = bones.ToArray(); // Use new bones
            combineSkinnedRenderer.material = newMaterial;
            for (int i = 0; i < combineInstances.Count; i++)
            {
                combineInstances[i].mesh.uv = oldUV[i];
            }

            Resources.UnloadUnusedAssets();
        }

        public void SetElementModle(Element element, GameObject modle)
        {
            if (allModles.TryGetValue(element, out GameObject lastElement))
            {
                GameObject.DestroyImmediate(lastElement);
                allModles.Remove(element);
                //combineModles.Remove(element);
            }

            BonesData bones = BonesConfig.instance.GetConfig(element);
            allModles.Add(element, modle);
            if (bones == null)
            {
                modle.transform.SetParent(_basic.transform);
            }
            else
            {
                Transform parent = _basic.transform.Find(bones.location_path);
                if (parent == null)
                {
                    Debug.LogError("config error,not find:" + bones.location_path);
                    GameObject.DestroyImmediate(modle);
                    return;
                }

                modle.transform.SetParent(_basic.transform.Find(bones.location_path));
            }

            modle.transform.localPosition = Vector3.zero;
            modle.transform.localRotation = Quaternion.identity;
            modle.transform.localScale = Vector3.one;
            MeshRenderer[] renderer = modle.GetComponentsInChildren<MeshRenderer>();
            if (renderer != null)
            {
                for (int i = 0; i < renderer.Length; i++)
                {
                    foreach (var item in renderer[i].sharedMaterials)
                    {
                        Shader shader = Shader.Find(item.shader.name);
                        if (shader != null)
                        {
                            item.shader = shader;
                        }
                    }
                }
            }

            SkinnedMeshRenderer[] skinned = modle.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (skinned == null || skinned.Length <= 0)
            {
                return;
            }

            for (int i = 0; i < skinned.Length; i++)
            {
                foreach (var item in skinned[i].sharedMaterials)
                {
                    Shader shader = Shader.Find(item.shader.name);
                    if (shader != null)
                    {
                        item.shader = shader;
                    }
                }
            }
        }

        public void SetElementTexture(Element element, Texture texture)
        {
            if (!allModles.TryGetValue(element, out GameObject elementObject))
            {
                Services.Console.WriteLine("you need to set the model before you can set the texture:" + element);
                return;
            }

            MeshRenderer renderer = elementObject.GetComponentInChildren<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial.mainTexture = texture;
            }

            SkinnedMeshRenderer skinnedMeshRenderer = elementObject.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                skinnedMeshRenderer.sharedMaterial.mainTexture = texture;
            }
        }

        public void DestoryElement(Element element)
        {
            if (!allModles.TryGetValue(element, out GameObject gameObject))
            {
                return;
            }

            GameObject.DestroyImmediate(gameObject);
            allModles.Remove(element);
        }

        public void Dispose()
        {
            GameObject.DestroyImmediate(_basic);
            allModles.Clear();
        }

        public GameObject GetElementObject(Element element)
        {
            if (allModles.TryGetValue(element, out GameObject gameObject))
            {
                return gameObject;
            }

            return default;
        }

        public void Clear()
        {
            foreach (var item in allModles.Values)
            {
                GameObject.DestroyImmediate(item);
            }

            allModles.Clear();
        }

        public Texture2D GetElementTexture(Element element)
        {
            GameObject gameObject = GetElementObject(element);
            if (gameObject == null)
            {
                return default;
            }

            MeshRenderer renderer = gameObject.GetComponentInChildren<MeshRenderer>();
            if (renderer == null)
            {
                SkinnedMeshRenderer skinnedMeshRenderer = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
                return (Texture2D)skinnedMeshRenderer.sharedMaterial.mainTexture;
            }

            return (Texture2D)renderer.sharedMaterial.mainTexture;
        }

        public void UnCombine()
        {
            GameObject.DestroyImmediate(combineSkinnedRenderer);
            foreach (var item in allModles.Values)
            {
                item.SetActive(true);
            }
        }

        public void DisableElement(Element element)
        {
            if (element == Element.None)
            {
                foreach (var item in allModles.Values)
                {
                    item.SetActive(false);
                }

                return;
            }

            if (allModles.TryGetValue(element, out GameObject obj))
            {
                obj.SetActive(false);
            }
        }

        public void EnableElement(Element element)
        {
            if (element == Element.None)
            {
                foreach (var item in allModles.Values)
                {
                    item.SetActive(true);
                }

                return;
            }

            if (allModles.TryGetValue(element, out GameObject obj))
            {
                obj.SetActive(true);
            }
        }
    }
}