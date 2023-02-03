namespace Gaming.Config
{
    using System;
    using System.Text;

    public interface IConfig : IRefrence
    {

    }

    public interface IConfig<T> : IConfig where T : IConfigData
    {
        void AddConfig(T config);
        void RemoveConfig(T config);
        T GetConfig(Func<T, bool> finder);
        void Clear();
    }

    public interface IConfigData : IRefrence
    {

    }

    public sealed class ConfigManager : Singleton<ConfigManager>, IRefrence
    {
        public T LoadConfig<T>(string fileName) where T : IConfig => (T)LoadConfig(typeof(T), fileName);

        public IConfig LoadConfig(Type configType, string fileName)
        {
            if (configType == null)
            {
                throw new ArgumentNullException("config type");
            }
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("file name");
            }
            if (typeof(IConfig).IsAssignableFrom(configType))
            {
                throw new NotImplementedException("the config type is not implemented iconfig interface");
            }
            if (configType.IsAbstract || configType.IsInterface)
            {
                throw new Exception("the config type cannot be abstract or interface");
            }
            byte[] bytes = GamingService.File.ReadData(fileName);
            return (IConfig)Newtonsoft.Json.JsonConvert.DeserializeObject(Encoding.UTF8.GetString(bytes), configType);
        }
        public void Dispose()
        {
        }

        internal void Unload<T>()
        {
            throw new NotImplementedException();
        }

        internal bool Exist(string fileName)
        {
            throw new NotImplementedException();
        }

        internal void Save(string fileName, IConfig config)
        {
            throw new NotImplementedException();
        }

        internal string Export<T>() where T : IConfig
        {
            return string.Empty;
        }

        internal bool Delete(string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
