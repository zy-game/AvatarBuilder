namespace Gaming.Runnable
{
    using Gaming.Extension;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public sealed class ScheduleRunnable : IRunnable
    {
        private Queue<Func<IEnumerator>> funcs = new Queue<Func<IEnumerator>>();


        public void Dispose()
        {
            funcs.Clear();
        }

        public IEnumerator Execute()
        {
            while (funcs.Count > 0)
            {
                Func<IEnumerator> func = funcs.Dequeue();
                yield return func();
            }
        }

        public IRunnable Then(Func<IEnumerator> func)
        {
            funcs.Enqueue(func);
            return this;
        }
    }
}
