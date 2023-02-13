#if UNITY_EDITOR
namespace Gaming.Editor
{
    using Gaming.Avatar;
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    class OutData
    {
        public int element;
        public bool is_normal;
        public string icon;
        public string texture;
        public string group;
        public string model;
    }


    [Serializable]
    public class ElementItemData
    {
        public bool isOn;
        public bool isNormal;
        public string name;
        public Element element;
        public Texture2D icon;
        public GameObject fbx;
        public Material material;
    }
}
#endif