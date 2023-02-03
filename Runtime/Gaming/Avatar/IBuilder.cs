namespace Gaming.Avatar
{
    using Gaming;
    using System;
    using UnityEngine;
    /// <summary>
    /// ��װ��
    /// </summary>
    public interface IBuilder : IRefrence
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
        /// ��ȡ��λģ����ͼ
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        Texture2D GetElementTexture(Element element);

        /// <summary>
        /// �ϲ�����
        /// </summary>
        void Combine();

        /// <summary>
        /// �������в���ģ��
        /// </summary>
        void Clear();

        /// <summary>
        /// ��ֺϲ�ģ��
        /// </summary>
        void UnCombine();

        /// <summary>
        /// ���ز���
        /// </summary>
        /// <param name="element"></param>
        void DisableElement(Element element);

        /// <summary>
        /// ��ʾ����
        /// </summary>
        /// <param name="element"></param>
        void EnableElement(Element element);
    }
}