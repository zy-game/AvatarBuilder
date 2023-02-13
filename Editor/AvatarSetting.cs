#if UNITY_EDITOR
namespace Gaming.Editor
{
    using UnityEngine;
    using Object = UnityEngine.Object;

    [System.Serializable]
    [ExcludeFromPreset]
    [CreateAssetMenu(menuName = "Gaming/AvatarSetting", fileName = "AvatarSetting")]
    public class AvatarSetting : ScriptableObject
    {
        [SerializeField]
        [Tooltip("服务器地址")]
        public string address;

        [SerializeField]
        [Tooltip("组织名字")]
        public string organization;

        [SerializeField]
        [Tooltip("icon输出路径")]
        public Object iconFolder;

        [SerializeField]
        [Tooltip("部件输出路径")]
        public Object elementsFolder;

        [SerializeField]
        [Tooltip("预制件输出路径")]
        public Object prefabFolder;

        private static AvatarSetting _instance;

        public static AvatarSetting instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<AvatarSetting>("AvatarSetting");
                }
                return _instance;
            }
        }
    }
}
#endif