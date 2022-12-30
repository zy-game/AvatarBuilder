namespace AvatarBuild.Interface
{
    using AvatarBuild.Config;
    using System;
    using UnityEngine;

    /// <summary>
    /// 第三方接口
    /// </summary>
    public interface IContorller : IDisposable
    {
        /// <summary>
        /// 接口初始化
        /// </summary>
        /// <param name="skeleton">基础骨骼资源地址</param>
        /// <param name="address">服务器地址</param>
        void Initialize<T>(string skeleton, string address, Camera iconCamera) where T : IResourceLoader, new();

        /// <summary>
        /// 获取指定部位数据
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        ElementData GetElementData(Element element);

        /// <summary>
        /// 设置部位模型
        /// </summary>
        /// <param name="element">部位枚举</param>
        /// <param name="assetName">资源名或者路径</param>
        void SetElementData(ElementData elementData);

        /// <summary>
        /// 清理指定部位
        /// </summary>
        /// <param name="element">部位枚举</param>
        void ClearElement(Element element);

        /// <summary>
        /// 导出配置
        /// </summary>
        /// <param name="configName">配置名</param>
        /// <returns>配置json数据</returns>
        string ExportConfig(string configName);

        /// <summary>
        /// 导入配置
        /// </summary>
        /// <param name="config">配置json数据</param>
        void ImportConfig(string config);

        /// <summary>
        /// 预览部件，只能预览修改的贴图
        /// </summary>
        /// <param name="element">部位枚举</param>
        /// <param name="bytes">文件数据</param>
        void PreviewElement(Element element, byte[] bytes);

        /// <summary>
        /// 上传部件
        /// </summary>
        /// <param name="element">部件枚举</param>
        /// <param name="bytes">文件数据</param>
        void UploadElementAsset(Element element, byte[] bytes);

        /// <summary>
        /// 合并模型
        /// </summary>
        void Combine();
    }
}