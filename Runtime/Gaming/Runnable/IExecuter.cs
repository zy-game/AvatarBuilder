
using System.Collections;

namespace Gaming.Runnable
{

    /// <summary>
    /// 执行器
    /// </summary>
    public interface IExecuter : IRefrence
    {
        IEnumerator Execute(params object[] args);
    }

    /// <summary>
    /// 有返回值执行器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IExecuter<T> : IExecuter
    {
        T target { get; }
    }
}
