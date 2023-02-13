namespace Gaming.Files
{
    using Gaming.Runnable;
    using System;
    using UnityEngine;
    using Extension;
    using System.Collections;
    using UnityEngine.Networking;
    using System.IO;

    public class FileSystem : Singleton<FileSystem>, IRefrence
    {
        public byte[] Read(string filePath, bool isDecryption, bool isDecomperss)
        {
            if (!Exist(filePath))
            {
                return Array.Empty<byte>();
            }
            byte[] bytes = System.IO.File.ReadAllBytes(filePath);
            if (isDecryption)
            {
                //todo 解密
            }
            if (isDecomperss)
            {
                //todo 解压缩
            }
            return bytes;
        }
        public void Dispose()
        {

        }

        internal bool Exist(string filePath) => System.IO.File.Exists(filePath);

        internal bool Delete(string filePath)
        {
            if (!Exist(filePath))
            {
                return true;
            }
            System.IO.File.Delete(filePath);
            return Exist(filePath);
        }

        internal void Write(string filePath, byte[] bytes, bool isEncryption, bool isComperss)
        {
            if (!Delete(filePath))
            {
                throw new Exception("the file is already exist! name:" + filePath);
            }
            if (isEncryption)
            {
                //todo 加密
            }
            if (isComperss)
            {
                //todo 压缩
            }
            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            System.IO.File.WriteAllBytes(filePath, bytes);
        }

        internal bool Copy(string scrPath, string desPath)
        {
            if (!Exist(scrPath))
            {
                return false;
            }
            System.IO.File.Delete(desPath);
            System.IO.File.Copy(scrPath, desPath);
            return System.IO.File.Exists(desPath);
        }

        internal bool Move(string scrPath, string desPath)
        {
            if (!Exist(scrPath))
            {
                return false;
            }
            System.IO.File.Delete(desPath);
            System.IO.File.Move(scrPath, desPath);
            return System.IO.File.Exists(desPath);
        }

        internal IRunnable<byte[]> ReadAsync(string scrPath, bool isDecryption, bool isDecomperss)
        {
            throw new NotImplementedException();
        }

        internal IRunnable WriteAsync(string filePath, byte[] bytes, bool isEncryption, bool isComperss)
        {
            ScheduleRunnable commonRunnable = Services.Refrence.Require<ScheduleRunnable>();
            WriteableFileExecuter writeableFileExecuter = Services.Refrence.Require<WriteableFileExecuter>();
            writeableFileExecuter.Execute(commonRunnable, filePath, bytes, isEncryption, isComperss);
            return commonRunnable;
        }
    }

    public class WriteableFileExecuter : IExecuter
    {
        public void Dispose()
        {

        }

        public void Execute(IRunnable runnable, params object[] args)
        {
            string filePath = (string)args[0];
            byte[] bytes = (byte[])args[1];
            bool isEncryption = (bool)args[2];
            bool isComperss = (bool)args[2];
            Services.File.WriteData(filePath, bytes, isEncryption, isComperss);
        }
    }

    public class ReadableFileExecuter : IExecuter<byte[]>
    {
        public byte[] target
        {
            get;
            private set;
        }

        private IEnumerator enumerator;
        public void Dispose()
        {
            Services.MonoBehaviour.StopCoroutine(enumerator);
        }

        public void Execute(IRunnable<byte[]> runnable, params object[] args)
        {
            string filePath = (string)args[0];
            bool isEncryption = (bool)args[1];
            bool isComperss = (bool)args[2];
            enumerator = GetEnumerator(runnable, filePath, isEncryption, isComperss);
            Services.MonoBehaviour.StartCoroutine(enumerator);
        }

        private IEnumerator GetEnumerator(IRunnable<byte[]> runnable, string filePath, bool isEncryption, bool isComperss)
        {
            UnityWebRequest request = UnityWebRequest.Get(filePath);
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                target = Array.Empty<byte>();
            }
            else
            {
                target = request.downloadHandler.data;
            }
            yield return runnable.Execute(target);
        }
    }
}
