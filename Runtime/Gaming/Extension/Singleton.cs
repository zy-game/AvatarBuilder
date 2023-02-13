using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gaming
{
    public abstract class Singleton<T> where T : class, new()
    {
        private static Lazy<T> _instance = new Lazy<T>(Instantiate);

        private static T Instantiate()
        {
            return new T();
        }
        public static T instance
        {
            get
            {
                return _instance.Value;
            }
        }
    }
}
