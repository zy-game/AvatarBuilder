namespace AvatarBuild
{
    using System;
    using UnityEngine;
    using System.Collections.Generic;
    using AvatarBuild.Interface;
    using AvatarBuild.Config;

    /// <summary>
    /// Avatar ¥¶¿Ì
    /// </summary>
    public sealed class AvatarBuilder : IBuilder
    {
        private GameObject _basic;
        /// <summary>
        /// Only for merge materials.
        /// </summary>
        private const int COMBINE_TEXTURE_MAX = 512;
        private const string COMBINE_DIFFUSE_TEXTURE = "_MainTex";
        private Dictionary<Element, GameObject> allModles;
        private Dictionary<Element, GameObject> combineModles;
        public AvatarBuilder(GameObject basicModle)
        {
            _basic = basicModle;
            allModles = new Dictionary<Element, GameObject>();
            combineModles = new Dictionary<Element, GameObject>();
        }

        public void Combine()
        {
            // Fetch all bones of the skeleton
            List<Transform> transforms = new List<Transform>();
            transforms.AddRange(_basic.GetComponentsInChildren<Transform>(true));

            List<SkinnedMeshRenderer> skinneds = new List<SkinnedMeshRenderer>();
            foreach (var item in combineModles.Values)
            {
                skinneds.AddRange(item.GetComponentsInChildren<SkinnedMeshRenderer>());
            }

            List<Material> materials = new List<Material>();//the list of materials
            List<CombineInstance> combineInstances = new List<CombineInstance>();//the list of meshes
            List<Transform> bones = new List<Transform>();//the list of bones

            // Below informations only are used for merge materilas(bool combine = true)
            List<Vector2[]> oldUV = null;
            Material newMaterial = null;
            Texture2D newDiffuseTex = null;

            // Collect information from meshes
            for (int i = 0; i < skinneds.Count; i++)
            {
                SkinnedMeshRenderer smr = skinneds[i];
                materials.AddRange(smr.materials); // Collect materials
                                                   // Collect meshes
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

            // merge materials
            newMaterial = new Material(Shader.Find("Unlit/Texture"));
            oldUV = new List<Vector2[]>();
            // merge the texture
            List<Texture2D> Textures = new List<Texture2D>();
            for (int i = 0; i < materials.Count; i++)
            {
                Textures.Add(materials[i].GetTexture(COMBINE_DIFFUSE_TEXTURE) as Texture2D);
            }

            newDiffuseTex = new Texture2D(COMBINE_TEXTURE_MAX, COMBINE_TEXTURE_MAX, TextureFormat.RGBA32, true);
            Rect[] uvs = newDiffuseTex.PackTextures(Textures.ToArray(), 0);
            newMaterial.mainTexture = newDiffuseTex;

            // reset uv
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
            SkinnedMeshRenderer r = _basic.AddComponent<SkinnedMeshRenderer>();
            r.sharedMesh = new Mesh();
            r.sharedMesh.name = "runtime_combine_mesh";
            r.sharedMesh.CombineMeshes(combineInstances.ToArray(), true, false);// Combine meshes
            r.bones = bones.ToArray();// Use new bones
            r.material = newMaterial;
            for (int i = 0; i < combineInstances.Count; i++)
            {
                combineInstances[i].mesh.uv = oldUV[i];
            }
        }

        public void SetElementModle(Element element, GameObject modle)
        {
            if (allModles.TryGetValue(element, out GameObject lastElement))
            {
                GameObject.DestroyImmediate(lastElement);
                allModles.Remove(element);
                combineModles.Remove(element);
            }
            BonesConfig bones = BonesConfig.GetConfig(element);
            allModles.Add(element, modle);
            if (bones == null)
            {
                modle.transform.SetParent(_basic.transform);
            }
            else
            {
                Transform parent = _basic.transform.Find(bones.path);
                if (parent == null)
                {
                    Debug.LogError("config error,not find:" + bones.path);
                    GameObject.DestroyImmediate(modle);
                    return;
                }
                modle.transform.SetParent(_basic.transform.Find(bones.path));
            }
            modle.transform.localPosition = Vector3.zero;
            modle.transform.localRotation = Quaternion.identity;
            modle.transform.localScale = Vector3.one;
            SkinnedMeshRenderer[] skinned = modle.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (skinned == null || skinned.Length <= 0)
            {
                return;
            }
            combineModles.Add(element, modle);
            Combine();
            modle.SetActive(false);
        }

        public void SetElementTexture(Element element, Texture texture)
        {
            if (!allModles.TryGetValue(element, out GameObject elementObject))
            {
                throw new Exception("you need to set the model before you can set the texture");
            }
            MeshRenderer renderer = elementObject.GetComponentInChildren<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.mainTexture = texture;
                renderer.sharedMaterial.mainTexture = texture;
            }
            SkinnedMeshRenderer skinnedMeshRenderer = elementObject.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                skinnedMeshRenderer.sharedMaterial.mainTexture = texture;
                Combine();
            }
        }

        public void DestoryElement(Element element)
        {
            if (!allModles.TryGetValue(element, out GameObject gameObject))
            {
                return;
            }
            GameObject.DestroyImmediate(gameObject);
            if (combineModles.TryGetValue(element, out gameObject))
            {
                combineModles.Remove(element);
                Combine();
            }
            allModles.Remove(element);
        }

        public void Dispose()
        {
            GameObject.DestroyImmediate(_basic);
            allModles.Clear();
            combineModles.Clear();
        }

        public GameObject GetElementObject(Element element)
        {
            if (allModles.TryGetValue(element, out GameObject gameObject))
            {
                return gameObject;
            }
            return default;
        }

        public void ClearCombine()
        {
            foreach (var item in combineModles.Keys)
            {
                DestoryElement(item);
            }
        }
    }
}