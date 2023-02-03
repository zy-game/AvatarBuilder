
namespace Gaming.Runnable
{
    using System;
    using System.Collections;
    using UnityEngine;

    public interface IRunnable : IRefrence
    {
        IRunnable Then(Action action);
        IRunnable Then(Func<IEnumerator> func);
        void Execute<E>(params object[] args) where E : IExecuter;
    }

    public interface IRunnable<T> : IRefrence
    {
        IRunnable<T> Then(Action action);
        IRunnable<T> Then(Action<T> action);
        IRunnable<T> Then(Func<IEnumerator> func);
        IRunnable<T> Then(Func<T, IEnumerator> func);
        void Execute<E>(params object[] args) where E : IExecuter<T>;
    }
}
