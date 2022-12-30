namespace AvatarBuild.Config
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 骨骼配置，表明每个部件挂点的骨骼路径
    /// </summary>
    public sealed class BonesConfig
    {
        public string path;
        public Element element;


        private static List<BonesConfig> configs;
        private static void EnsureLoadConfig()
        {
            if (configs != null)
            {
                return;
            }
            //todo 加载骨骼配置表,这里可能会写死
            configs = new List<BonesConfig>();
            AddConfig(Element.MouthAccessory, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02/SK_neck/SK_head/SK_mouth");
            AddConfig(Element.Moustache, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02/SK_neck/SK_head/SK_mouth");
            AddConfig(Element.Wings, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02");
            AddConfig(Element.HeadAccessory_large, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02/SK_neck/SK_head");
            AddConfig(Element.Ankles, "root/root01/SK_pine_00/SK_L_high/SK_L_calf/SK_L_foot");
            AddConfig(Element.Wrist, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02/SK_L_arm/SK_L_forearm/SK_L_wrist");
            AddConfig(Element.HeadAccessory_large, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02/SK_neck/SK_head/SK_hair");
            AddConfig(Element.Cape, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02/SK_neck/SK_head/SK_hair");
            AddConfig(Element.HeadAccessory_small, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02/SK_neck/SK_head/SK_hair");
            AddConfig(Element.NoseAccessory, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02/SK_neck/SK_head/SK_nose");
            AddConfig(Element.EyeAccessory, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02/SK_neck/SK_head/SK_eye00/SK_eye01");
            AddConfig(Element.Neck, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02/SK_neck");
            AddConfig(Element.Shoulders, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02");
            AddConfig(Element.Handheld, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02/SK_R_arm/SK_R_forearm/SK_R_wrist/SK_R_hand_00");
            AddConfig(Element.HairExtension, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02/SK_neck/SK_head/SK_hair");
        }

        public static void AddConfig(Element element, string bonesPath)
        {
            EnsureLoadConfig();
            RemoveConfig(element);
            configs.Add(new BonesConfig() { element = element, path = bonesPath });
        }

        public static void RemoveConfig(Element element)
        {
            EnsureLoadConfig();
            BonesConfig config = GetConfig(element);
            if (config == null)
            {
                return;
            }
            configs.Remove(config);
        }
        public static BonesConfig GetConfig(Element element)
        {
            EnsureLoadConfig();
            BonesConfig config = null;
            for (int i = 0; i < configs.Count; i++)
            {
                if (configs[i].element == element)
                {
                    config = configs[i];
                    break;
                }
            }
            return config;
        }
    }
}