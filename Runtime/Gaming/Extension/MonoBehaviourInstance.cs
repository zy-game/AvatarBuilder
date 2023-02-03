using System;
using UnityEngine;
using System.Collections;

namespace Gaming.Extension
{
    public class MonoBehaviourInstance : MonoBehaviour
    {
        private static event Action update;
        private static event Action fixedUpdate;
        private static MonoBehaviourInstance _instance;
        private static MonoBehaviourInstance instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("MonoBehaviourInstance").AddComponent<MonoBehaviourInstance>();
                    GameObject.DontDestroyOnLoad(_instance.gameObject);
                }
                return _instance;
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

        public static void StartCor(IEnumerator enumerator)
        {
            instance.StartCoroutine(enumerator);
        }

        public static void AddUpdate(Action onUpdateCallback)
        {
            update += onUpdateCallback;
        }

        public static void RemoveUpdate(Action callback)
        {
            update -= callback;
        }

        public static void AddFixedUpdate(Action callback)
        {
            fixedUpdate += callback;
        }
        public static void RemoveFixedUpdate(Action callback)
        {
            fixedUpdate -= callback;
        }
    }
}
