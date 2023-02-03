namespace Gaming.Config
{
    using Gaming;
    using Gaming.Avatar;
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;

    /// <summary>
    /// Avatar数据
    /// </summary>
    public sealed class AvatarConfig : IConfig<ElementData>
    {
        public string name;
        public List<ElementData> elements;

        public AvatarConfig()
        {
            elements = new List<ElementData>();
        }

        public void Dispose()
        {
            Clear();
        }

        public void SetElementData(ElementData elementData)
        {
            RemoveElement(elementData.element);
            elements.Add(elementData);
        }

        public ElementData GetValue(Element element)
        {
            return this.elements.Find(x => x.element == element);
        }

        /// <summary>
        /// 移除部件
        /// </summary>
        /// <param name="element"></param>
        public void RemoveElement(Element element)
        {
            ElementData elementData = GetValue(element);
            if (elementData == null)
            {
                return;
            }
            elements.Remove(elementData);
        }

        public void AddConfig(ElementData config)
        {
            ElementData old = GetConfig(x => x.element == config.element);
            if (old != null)
            {
                RemoveConfig(old);
            }
            this.elements.Add(config);
        }

        public void RemoveConfig(ElementData config)
        {
            this.elements.Remove(config);
        }

        public ElementData GetConfig(Func<ElementData, bool> finder)
        {
            return this.elements.Find(x => finder(x));
        }

        public void Clear()
        {
            elements.Clear();
        }
    }
}