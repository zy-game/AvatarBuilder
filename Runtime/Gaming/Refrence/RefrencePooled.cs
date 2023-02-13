
namespace Gaming
{
    using System;
    using System.Collections.Generic;

    sealed class RefrenceService : Singleton<RefrenceService>
    {
        private Dictionary<Type, Queue<IRefrence>> map = new Dictionary<Type, Queue<IRefrence>>();

        private void EnsureRefrenceType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (type.IsInterface || type.IsAbstract)
            {
                throw new ArgumentException("the refrence type is can not form interface or abstract");
            }
            if (!typeof(IRefrence).IsAssignableFrom(type))
            {
                throw new NotImplementedException(typeof(IRefrence).Name);
            }
        }

        public T Require<T>() where T : IRefrence => (T)Require(typeof(T));

        public IRefrence Require(Type refrenceType)
        {
            EnsureRefrenceType(refrenceType);
            if (!map.TryGetValue(refrenceType, out Queue<IRefrence> queue))
            {
                map.Add(refrenceType, queue = new Queue<IRefrence>());
            }
            if (queue.TryDequeue(out IRefrence refrence))
            {
                return refrence;
            }
            return (IRefrence)Activator.CreateInstance(refrenceType);
        }


        public void Release(IRefrence refrence)
        {
            if (refrence == null)
            {
                throw new ArgumentNullException("refrence");
            }
            Type refrenceType = refrence.GetType();
            if (!map.TryGetValue(refrenceType, out Queue<IRefrence> queue))
            {
                map.Add(refrenceType, queue = new Queue<IRefrence>());
            }
            queue.Enqueue(refrence);
        }
    }
}
