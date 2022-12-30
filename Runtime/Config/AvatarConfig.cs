namespace AvatarBuild.Config
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Avatar数据
    /// </summary>
    public sealed class AvatarConfig : IDisposable
    {
        public static string Empty = "{}";
        public string name;
        public List<ElementData> elements;

        public AvatarConfig()
        {
            elements = new List<ElementData>();
        }

        public void Dispose()
        {
            name = string.Empty;
            elements.Clear();
        }

        public void SetElementData(ElementData elementData)
        {
            RemoveElement(elementData.element);
            elements.Add(elementData);
        }

        public ElementData GetElementData(Element element)
        {
            return this.elements.Find(x => x.element == element);
        }

        /// <summary>
        /// 移除部件
        /// </summary>
        /// <param name="element"></param>
        public void RemoveElement(Element element)
        {
            ElementData elementData = GetElementData(element);
            if (elementData == null)
            {
                return;
            }
            elements.Remove(elementData);
        }
    }
}