namespace Gaming.Avatar
{
    using Gaming;
    using System;
    using UnityEngine;
    /// <summary>
    /// 组装器
    /// </summary>
    public interface IBuilder : IRefrence
    {
        /// <summary>
        /// 设置部件模型
        /// </summary>
        /// <param name="element"></param>
        /// <param name="modle"></param>
        void SetElementModle(Element element, GameObject modle);

        /// <summary>
        /// 设置部件图片
        /// </summary>
        /// <param name="element"></param>
        /// <param name="texture"></param>
        void SetElementTexture(Element element, Texture texture);

        /// <summary>
        /// 删除部件
        /// </summary>
        /// <param name="element"></param>
        void DestoryElement(Element element);

        /// <summary>
        /// 获取部位模型
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        GameObject GetElementObject(Element element);

        /// <summary>
        /// 获取部位模型贴图
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        Texture2D GetElementTexture(Element element);

        /// <summary>
        /// 合并部件
        /// </summary>
        void Combine();

        /// <summary>
        /// 清理所有部件模型
        /// </summary>
        void Clear();

        /// <summary>
        /// 拆分合并模型
        /// </summary>
        void UnCombine();

        /// <summary>
        /// 隐藏部件
        /// </summary>
        /// <param name="element"></param>
        void DisableElement(Element element);

        /// <summary>
        /// 显示部件
        /// </summary>
        /// <param name="element"></param>
        void EnableElement(Element element);
    }
}