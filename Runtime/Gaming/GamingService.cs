﻿namespace Gaming
{
    using System;

    public static class GamingService
    {
        public sealed class Refrence
        {
            /// <summary>
            /// 引用一个对象
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public static T Require<T>() where T : IRefrence => RefrencePooled.Require<T>();

            /// <summary>
            /// 引用一个对象
            /// </summary>
            /// <param name="type"></param>
            /// <returns></returns>
            public static IRefrence Require(Type type) => RefrencePooled.Require(type);

            /// <summary>
            /// 回收一个对象
            /// </summary>
            /// <param name="refrence"></param>
            public static void Release(IRefrence refrence) => RefrencePooled.Release(refrence);
        }

        public sealed class Logger
        {
            /// <summary>
            /// 输出日志
            /// </summary>
            /// <param name="message"></param>
            public static void Log(object message) => UnityEngine.Debug.Log(message);

            /// <summary>
            /// 输出日志
            /// </summary>
            /// <param name="format"></param>
            /// <param name="args"></param>
            public static void LogFormat(string format, params object[] args) => UnityEngine.Debug.LogFormat(format, args);

            /// <summary>
            /// 输出错误日志
            /// </summary>
            /// <param name="message"></param>
            public static void LogError(object message) => UnityEngine.Debug.LogError(message);

            /// <summary>
            /// 输出错误日志
            /// </summary>
            /// <param name="format"></param>
            /// <param name="args"></param>
            public static void LogErrorFormat(string format, params object[] args) => UnityEngine.Debug.LogErrorFormat(format, args);
        }

        public sealed class Config
        {
            /// <summary>
            /// 获取配置表
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="fileName"></param>
            /// <returns></returns>
            public static T GetConfig<T>(string fileName) where T : Gaming.Config.IConfig => Gaming.Config.ConfigManager.instance.LoadConfig<T>(fileName);

            /// <summary>
            /// 获取配置表
            /// </summary>
            /// <param name="type"></param>
            /// <param name="fileName"></param>
            /// <returns></returns>
            public static Gaming.Config.IConfig GetConfig(Type type, string fileName) => Gaming.Config.ConfigManager.instance.LoadConfig(type, fileName);

            /// <summary>
            /// 卸载配置表
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public static void UnloadConfig<T>() => Gaming.Config.ConfigManager.instance.Unload<T>();

            /// <summary>
            /// 是否存在配置表
            /// </summary>
            /// <param name="fileName"></param>
            /// <returns></returns>
            public static bool ExistConfig(string fileName) => Gaming.Config.ConfigManager.instance.Exist(fileName);

            /// <summary>
            /// 保存配置表
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="config"></param>
            public static void Save(string fileName, Gaming.Config.IConfig config) => Gaming.Config.ConfigManager.instance.Save(fileName, config);

            /// <summary>
            /// 删除配置表
            /// </summary>
            /// <param name="fileName"></param>
            /// <returns></returns>
            public static bool Delete(string fileName) => Gaming.Config.ConfigManager.instance.Delete(fileName);
        }

        public sealed class File
        {
            /// <summary>
            /// 读取文件数据
            /// </summary>
            /// <param name="fileName"></param>
            /// <returns></returns>
            public static byte[] ReadData(string fileName, bool isDecryption = false, bool isDecomperss = false) => Files.FileSystem.instance.Read(fileName, isDecryption, isDecomperss);

            /// <summary>
            /// 异步读取文件
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="isDecryption"></param>
            /// <param name="isDecomperss"></param>
            /// <returns></returns>
            public static Runnable.IRunnable<T> ReadDataAsync<T>(string fileName, bool isDecryption = false, bool isDecomperss = false) => Files.FileSystem.instance.ReadAsync<T>(fileName, isDecryption, isDecomperss);

            /// <summary>
            /// 是否存在文件
            /// </summary>
            /// <param name="fileName"></param>
            /// <returns></returns>
            public static bool Exist(string fileName) => Files.FileSystem.instance.Exist(fileName);

            /// <summary>
            /// 删除文件
            /// </summary>
            /// <param name="fileName"></param>
            /// <returns></returns>
            public static bool Delete(string fileName) => Files.FileSystem.instance.Delete(fileName);

            /// <summary>
            /// 写入文件数据
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="bytes"></param>
            /// <param name="isEncryption"></param>
            /// <param name="isComperss"></param>
            public static void WriteData(string fileName, byte[] bytes, bool isEncryption = false, bool isComperss = false) => Files.FileSystem.instance.Write(fileName, bytes, isEncryption, isComperss);

            /// <summary>
            /// 异步写入数据
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="bytes"></param>
            /// <param name="isEncryption"></param>
            /// <param name="isComperss"></param>
            /// <returns></returns>
            public static Runnable.IRunnable WriteDataAsync(string fileName, byte[] bytes, bool isEncryption = false, bool isComperss = false) => Files.FileSystem.instance.WriteAsync(fileName, bytes, isEncryption, isComperss);

            /// <summary>
            /// 复制文件到指定的路径
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="desPath"></param>
            /// <returns></returns>
            public static bool Copy(string fileName, string desPath) => Files.FileSystem.instance.Copy(fileName, desPath);

            /// <summary>
            /// 移动文件到指定的路径
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="desPath"></param>
            /// <returns></returns>
            public static bool Move(string fileName, string desPath) => Files.FileSystem.instance.Move(fileName, desPath);
        }

        public sealed class Resource
        {
            /// <summary>
            /// 设置资源加载器
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public static void SetResourceLoader<T>() where T : Gaming.Resource.IResourceLoader => Gaming.Resource.ResourceManager.instance.SetResourceLoader<T>();

            /// <summary>
            /// 加载资源
            /// </summary>
            /// <param name="assetName"></param>
            /// <returns></returns>
            public static Runnable.IRunnable<Gaming.Resource.IResContext> LoadAsset(string assetName) => Gaming.Resource.ResourceManager.instance.LoadAsset(assetName);

            /// <summary>
            /// 回收资源
            /// </summary>
            /// <param name="context"></param>
            public static void Release(Gaming.Resource.IResContext context) => Gaming.Resource.ResourceManager.instance.Release(context);
        }

        public sealed class Events
        {
            /// <summary>
            /// 通知事件
            /// </summary>
            /// <param name="evtName"></param>
            /// <param name="evtData"></param>
            public static void Notice(string evtName, object evtData = null) => Event.EventSystem.instance.NotiflyEvent(evtName, evtData);

            /// <summary>
            /// 清理所有订阅
            /// </summary>
            public static void Clear() => Event.EventSystem.instance.Dispose();

            /// <summary>
            /// 注册事件回调
            /// </summary>
            /// <param name="evtName"></param>
            /// <param name="callback"></param>
            public static void Register(string evtName, Action<object> callback) => Event.EventSystem.instance.Register(evtName, callback);

            /// <summary>
            /// 移除事件回调
            /// </summary>
            /// <param name="evtName"></param>
            /// <param name="callback"></param>
            public static void Unregister(string evtName, Action<object> callback) => Event.EventSystem.instance.Unregister(evtName, callback);

            /// <summary>
            /// 订阅事件分发
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public static void Subscribe<T>() where T : Event.IEventDispatch => Event.EventSystem.instance.SubscribeDistribution<T>();

            /// <summary>
            /// 移除事件分发订阅
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public static void Unsubacribe<T>() where T : Event.IEventDispatch => Event.EventSystem.instance.UnsubscribeDistribution<T>();

            /// <summary>
            /// 订阅事件分发
            /// </summary>
            /// <param name="callback"></param>
            public static void Subscribe(Action<string, object> callback) => Event.EventSystem.instance.SubscribeDistribution(callback);

            /// <summary>
            /// 移除事件分发订阅
            /// </summary>
            /// <param name="callback"></param>
            public static void Unsubscribe(Action<string, object> callback) => Event.EventSystem.instance.UnsubscribeDistribution(callback);

            /// <summary>
            /// 订阅事件分发
            /// </summary>
            /// <param name="type"></param>
            public static void Subscribe(Type type) => Event.EventSystem.instance.SubscribeDistribution(type);

            /// <summary>
            /// 移除事件分发订阅
            /// </summary>
            /// <param name="type"></param>
            public static void Unsubscribe(Type type) => Event.EventSystem.instance.UnsubscribeDistribution(type);
        }
    }
}
