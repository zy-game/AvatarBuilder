#if UNITY_EDITOR
namespace AvatarBuild.Editor
{
    using Gaming.Avatar;
    using System.Collections.Generic;
    using UnityEngine;

    public class ElemetConfig : ScriptableObject
    {
        public string address = "http://192.168.199.88:3456/";
        public List<ElementItem> elements;
    }
    public class ElementItem
    {
        public string path;
        public bool isNormal;
        public Element element;
    }
}
#endif