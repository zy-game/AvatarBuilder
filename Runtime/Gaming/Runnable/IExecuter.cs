
using System.Collections;

namespace Gaming.Runnable
{

    /// <summary>
    /// 执行器
    /// </summary>
    public interface IExecuter : IRefrence
    {
        void Execute(IRunnable runnable, params object[] args);
    }

    /// <summary>
    /// 有返回值执行器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IExecuter<T> : IRefrence
    {
        void Execute(IRunnable<T> runnable, params object[] args);
    }
}
