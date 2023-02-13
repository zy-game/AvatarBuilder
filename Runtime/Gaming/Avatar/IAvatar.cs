namespace Gaming.Avatar
{
    using Gaming;
    using Gaming.Config;
    using Gaming.Resource;
    using UnityEngine;

    /// <summary>
    /// 第三方接口
    /// </summary>
    public interface IAvatar : IRefrence
    {
        string address { get; }
        /// <summary>
        /// 模型控制器
        /// </summary>
        IBuilder builder { get; }

        /// <summary>
        /// Avatar物体
        /// </summary>
        GameObject gameObject { get; }

        /// <summary>
        /// 接口初始化
        /// </summary>
        /// <param name="skeleton">基础骨骼资源地址</param>
        /// <param name="address">服务器地址</param>
        void Initialize(string skeleton, string address);

        /// <summary>
        /// 播放动画
        /// </summary>
        /// <param name="name"></param>
        void PlayAni(string name);

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
        void SetElementData(params ElementData[] elementDatas);

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
        void UploadElementAsset(Element element, string name, byte[] bytes);

        /// <summary>
        /// 合并模型
        /// </summary>
        void Combine();

        /// <summary>
        /// 拆分已合并的模型
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

        /// <summary>
        /// 将所有部位显示在视图中
        /// </summary>
        /// <param name="element">如果参数为<see cref="Element.None"/>则将整个模型显示在视图中，否则将部件最大化显示</param>
        void ShowInView(Element element);
    }
}