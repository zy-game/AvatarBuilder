namespace AvatarBuild.Interface
{
    using System;
    using UnityEngine;
    /// <summary>
    /// ��װ��
    /// </summary>
    public interface IBuilder : IDisposable
    {
        /// <summary>
        /// ���ò���ģ��
        /// </summary>
        /// <param name="element"></param>
        /// <param name="modle"></param>
        void SetElementModle(Element element, GameObject modle);

        /// <summary>
        /// ���ò���ͼƬ
        /// </summary>
        /// <param name="element"></param>
        /// <param name="texture"></param>
        void SetElementTexture(Element element, Texture texture);

        /// <summary>
        /// ɾ������
        /// </summary>
        /// <param name="element"></param>
        void DestoryElement(Element element);

        /// <summary>
        /// ��ȡ��λģ��
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        GameObject GetElementObject(Element element);

        /// <summary>
        /// �ϲ�����
        /// </summary>
        void Combine();

        /// <summary>
        /// ���������Ѻϲ��Ĳ���ģ��
        /// </summary>
        void ClearCombine();
    }
}