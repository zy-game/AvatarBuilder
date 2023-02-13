
namespace Gaming.Runnable
{
    using System;
    using System.Collections;

    public interface IRunnable : IRefrence
    {
        IRunnable Then(Func<IEnumerator> func);
        IEnumerator Execute();
    }

    public interface IRunnable<T> : IRefrence
    {
        T target { get; }
        IEnumerator Execute();
        void SetResult(T args);
        IRunnable<T> Then(Func<T, IEnumerator> func);
    }
}
