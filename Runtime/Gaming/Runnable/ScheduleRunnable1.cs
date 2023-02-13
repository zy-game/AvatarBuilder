namespace Gaming.Runnable
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public sealed class ScheduleRunnable<T> : IRunnable<T>
    {
        private bool _isSetResult = false;
        private Queue<Func<T, IEnumerator>> _queue = new();
        public T target { get; private set; }

        public IEnumerator Execute()
        {
            yield return new WaitUntil(EnsureSchedulerCompleted);
            while (_queue.Count > 0)
            {
                var func = _queue.Dequeue();
                yield return func(target);
            }
        }

        private bool EnsureSchedulerCompleted()
        {
            return _isSetResult;
        }

        public void SetResult(T args)
        {
            target = args;
            _isSetResult = true;
        }

        public void Dispose()
        {
            _queue.Clear();
            target = default;
            _isSetResult = false;
        }

        public IRunnable<T> Then(Func<T, IEnumerator> func)
        {
            IEnumerator GetEnumerator(T args)
            {
                yield return func(args);
            }

            _queue.Enqueue(GetEnumerator);
            return this;
        }
    }
}