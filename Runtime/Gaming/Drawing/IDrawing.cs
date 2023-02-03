namespace Gaming.Drawing
{
    using Gaming;
    using Gaming.Avatar;
    using System;
    using UnityEngine;

    public enum Changed
    {
        WaitSave,
        Saved,
    }

    public interface IDrawing : IRefrence
    {
        Changed changed { get; }

        GameObject gameObject { get; }

        bool Initialized(Camera camera, IAvatar contorller, Texture2D paint, Element element = Element.None, byte[] bytes = null);

        /// <summary>
        /// 设置笔触类型
        /// </summary>
        /// <param name="brush">笔刷类型</param>
        void SetPaintbrushType(PaintBrush brush);

        /// <summary>
        /// 设置画笔颜色
        /// </summary>
        /// <param name="color"></param>
        void SetPaintbrushColor(Color color);

        /// <summary>
        /// 设置画笔大小
        /// </summary>
        /// <param name="width"></param>
        void SetPaintbrushWidth(float width);

        /// <summary>
        /// 将图片作为涂鸦导入
        /// </summary>
        /// <param name="textureBytes">图片数据</param>
        void ImportTexture(byte[] textureBytes);

        /// <summary>
        /// 创建新图层,创建后自动调用<see cref="SelectionLayer(string)"/>
        /// </summary>
        /// <param name="name">图层名</param>
        void NewLayer(string name);

        /// <summary>
        /// 删除图层,调用前先调用<see cref="SelectionLayer(string)"/>
        /// </summary>
        void DeleteLayer();

        /// <summary>
        /// 选中图层
        /// </summary>
        /// <param name="name">图层名</param>
        void SelectionLayer(string name);

        /// <summary>
        /// 设置图层透明度
        /// </summary>
        /// <param name="alpha"></param>
        void SetLayerAlpha(float alpha);

        /// <summary>
        /// 撤销
        /// </summary>
        /// <param name="isBackup">true:后退，flase:前进</param>
        void UndoRecord(bool isBackup);

        /// <summary>
        /// 发布涂鸦
        /// </summary>
        void Publishing(string name);

        /// <summary>
        /// 保存涂鸦数据
        /// </summary>
        /// <param name="name">文件名</param>
        void Save(string name);

        /// <summary>
        /// 设置图层大小
        /// </summary>
        /// <param name="size"></param>
        void ResizeLayer(float size);
    }
}
