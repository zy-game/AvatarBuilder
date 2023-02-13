using System;
using UnityEngine;

namespace Gaming.Extension
{
    class UnityCallback : MonoBehaviour
    {
        private Action destory;
        private event Action update;
        private event Action fixedUpdate;
        private static UnityCallback _instance;
        public static UnityCallback instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("UnityCallback").AddComponent<UnityCallback>();
                    GameObject.DontDestroyOnLoad(_instance.gameObject);
                }
                return _instance;
            }
        }

        private void OnApplicationQuit()
        {
            Services.Events.Notice("AppQuit");
        }

        private void OnApplicationFocus(bool focus)
        {
            Services.Events.Notice("AppFocus", focus);
        }

        public void OnDestroy()
        {
            if (destory == null)
            {
                return;
            }
            try
            {
                destory();
            }
            catch
            {
            }
        }

        private void FixedUpdate()
        {
            if (fixedUpdate == null)
            {
                return;
            }
            try
            {
                fixedUpdate();
            }
            catch
            {
            }
        }

        private void Update()
        {
            if (update == null)
            {
                return;
            }
            try
            {
                update();
            }
            catch
            {
            }
        }

        public void AddUpdate(Action onUpdateCallback)
        {
            update += onUpdateCallback;
        }

        public void RemoveCallback(Action callback)
        {
            update -= callback;
            fixedUpdate -= callback;
        }

        public void AddFixedUpdate(Action callback)
        {
            fixedUpdate += callback;
        }

        public static void ListenDestory(GameObject gameObject, Action action)
        {
            gameObject.AddComponent<UnityCallback>().destory = action;
        }

        public static void ListenDestory<T>(GameObject gameObject, Action<T> action, T args)
        {
            ListenDestory(gameObject, () => { action(args); });
        }
    }
}
