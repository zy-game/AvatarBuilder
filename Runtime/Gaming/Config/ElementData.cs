namespace Gaming.Config
{
    using Gaming.Avatar;
    /// <summary>
    /// 部位数据
    /// </summary>
    public sealed class ElementData : IConfigData
    {
        /// <summary>
        /// 使用部件ID
        /// </summary>
        public string id;

        /// <summary>
        /// 部件使用的模型资源
        /// </summary>
        public string model;

        /// <summary>
        /// 部位
        /// </summary>
        public Element element;

        /// <summary>
        /// 部位使用的贴图
        /// </summary>
        public string texture;

        /// <summary>
        /// 部位Icon
        /// </summary>
        public string icon;

        /// <summary>
        /// 部位名称
        /// </summary>
        public string name;

        /// <summary>
        /// 分组名
        /// </summary>
        public string group;

        public void Dispose()
        {
        }
    }
}