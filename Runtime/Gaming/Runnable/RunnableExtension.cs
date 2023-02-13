using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gaming.Runnable
{
    public static class RunnableExtension
    {
        public static IRunnable Then<T, T2>(this IRunnable target, Action<T, T2> callback, T args, T2 args2)
        {
            IEnumerator GetEnumerator()
            {
                callback(args, args2);
                yield break;
            }

            target.Then(GetEnumerator);
            return target;
        }

        public static IRunnable<T> Then<T>(this IRunnable<T> target, Action<T> callback)
        {
            IEnumerator GetEnumerator(T args)
            {
                callback(args);
                yield break;
            }

            target.Then(GetEnumerator);
            return target;
        }

        public static IRunnable<T> Then<T, T2>(this IRunnable<T> target,Action<T,T2>callback, T2 args)
        {
            IEnumerator GetEnumerator(T result)
            {
                callback(result,args);
                yield break;
            }

            target.Then(GetEnumerator);
            return target;
        }

        public static IRunnable<T> Then<T, T2, T3>(this IRunnable<T> target, Action<T2, T3> callback, T2 args2, T3 args3)
        {
            IEnumerator GetEnumerator(T result)
            {
                callback(args2, args3);
                yield break;
            }

            target.Then(GetEnumerator);
            return target;
        }

        public static IEnumerator Execute<T>(this IRunnable<T> target, T args)
        {
            target.SetResult(args);
            yield return target.Execute();
        }
    }
}
