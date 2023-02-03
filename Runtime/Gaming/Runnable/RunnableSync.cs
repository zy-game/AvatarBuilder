namespace Gaming.Runnable
{
    using Gaming.Extension;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public sealed class RunnableSync : IRunnable
    {
        private IExecuter executer;
        private Queue<Func<IEnumerator>> funcs = new Queue<Func<IEnumerator>>();
        public void Dispose()
        {
            GamingService.Refrence.Release(executer);
            executer = null;
            funcs.Clear();
        }

        public void Execute<E>(params object[] args) where E : IExecuter
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

        public IRunnable Then(Action action)
        {
            IEnumerator Runner()
            {
                if (executer == null)
                {
                    yield break;
                }
                action();
            }
            Then(new Func<IEnumerator>(Runner));
            return this;
        }

        public IRunnable Then(Func<IEnumerator> func)
        {
            funcs.Enqueue(func);
            return this;
        }
    }
}
