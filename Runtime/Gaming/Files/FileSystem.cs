namespace Gaming.Files
{
    using Gaming.Runnable;
    using System;
    using UnityEngine;
    using Extension;
    using System.Collections;

    public class FileSystem : Singleton<FileSystem>, IRefrence
    {
        public string GetFilePath(string fileName)
        {
            return Application.persistentDataPath + "/" + fileName.GetMd5();
        }

        public byte[] Read(string fileName, bool isDecryption, bool isDecomperss)
        {
            fileName = GetFilePath(fileName);
            if (!Exist(fileName))
            {
                return Array.Empty<byte>();
            }
            byte[] bytes = System.IO.File.ReadAllBytes(fileName);
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

        internal bool Exist(string fileName)
        {
            fileName = GetFilePath(fileName);
            return System.IO.File.Exists(fileName);
        }

        internal bool Delete(string fileName)
        {
            System.IO.File.Delete(GetFilePath(fileName));
            return Exist(fileName);
        }

        internal void Write(string fileName, byte[] bytes, bool isEncryption, bool isComperss)
        {
            if (!Delete(fileName))
            {
                throw new Exception("the file is already exist! name:" + fileName);
            }
            if (isEncryption)
            {
                //todo 加密
            }
            if (isComperss)
            {
                //todo 压缩
            }
            System.IO.File.WriteAllBytes(GetFilePath(fileName), bytes);
        }

        internal bool Copy(string fileName, string desPath)
        {
            if (!Exist(fileName))
            {
                return false;
            }
            System.IO.File.Delete(desPath);
            System.IO.File.Copy(GetFilePath(fileName), desPath);
            return System.IO.File.Exists(desPath);
        }

        internal bool Move(string fileName, string desPath)
        {
            if (!Exist(fileName))
            {
                return false;
            }
            System.IO.File.Delete(desPath);
            System.IO.File.Move(GetFilePath(fileName), desPath);
            return System.IO.File.Exists(desPath);
        }

        internal IRunnable<byte[]> ReadAsync(string fileName, bool isDecryption, bool isDecomperss)
        {
            throw new NotImplementedException();
        }

        internal IRunnable<T> ReadAsync<T>(string fileName, bool isDecryption, bool isDecomperss)
        {
            RunnableAsync<T> internalRunnable = GamingService.Refrence.Require<RunnableAsync<T>>();
            internalRunnable.Execute<ReadableFileExecuter<T>>(fileName);
            return internalRunnable;
        }

        internal IRunnable WriteAsync(string fileName, byte[] bytes, bool isEncryption, bool isComperss)
        {
            RunnableSync commonRunnable = GamingService.Refrence.Require<RunnableSync>();
            commonRunnable.Execute<WriteableFileExecuter>();
            return commonRunnable;
        }
    }

    public class WriteableFileExecuter : IExecuter
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IEnumerator Execute(params object[] args)
        {
            throw new NotImplementedException();
        }
    }

    public class ReadableFileExecuter<T> : IExecuter<T>
    {
        public T target => throw new NotImplementedException();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IEnumerator Execute(params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
