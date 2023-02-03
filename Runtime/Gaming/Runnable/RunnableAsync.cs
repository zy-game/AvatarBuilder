namespace Gaming.Runnable
{
    using Gaming.Extension;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public sealed class RunnableAsync<T> : IRunnable<T>
    {
        private IExecuter<T> executer;
        private Queue<Func<T, IEnumerator>> funcs = new Queue<Func<T, IEnumerator>>();
        public void Dispose()
        {
            GamingService.Refrence.Release(executer);
            executer = null;
            funcs.Clear();
        }

        public void Execute<E>(params object[] args) where E : IExecuter<T>
        {
            executer = GamingService.Refrence.Require<E>();
            MonoBehaviourInstance.StartCor(Running());
            IEnumerator Running()
            {
                if (executer == null)
                {
                    yield break;
                }
                yield return executer.Execute(args);
                foreach (var item in funcs)
                {
                    yield return item;
                }
            }
        }

        public IRunnable<T> Then(Action<T> action)
        {
            IEnumerator Runner(T args)
            {
                if (executer == null)
                {
                    yield break;
                }
                action(executer.target);
            }
            Then(new Func<T, IEnumerator>(a => Runner(a)));
            return this;
        }

        public IRunnable<T> Then(Action action)
        {
            IEnumerator Runner(T args)
            {
                if (executer == null)
                {
                    yield break;
                }
                action();
            }
            Then(new Func<T, IEnumerator>(a => Runner(a)));
            return this;
        }

        public IRunnable<T> Then(Func<IEnumerator> func)
        {
            IEnumerator Runner(T args)
            {
                if (executer == null)
                {
                    yield break;
                }
                yield return func();
            }
            Then(new Func<T, IEnumerator>(a => Runner(a)));
            return this;
        }

        public IRunnable<T> Then(Func<T, IEnumerator> func)
        {
            funcs.Enqueue(func);
            return this;
        }
    }
}
