namespace Gaming.Config
{
    using Gaming.Avatar;
    using System;
    using System.Collections.Generic;

    public sealed class BonesData : IConfigData
    {
        public string path;
        public Element element;

        public void Dispose()
        {
        }
    }

    /// <summary>
    /// 骨骼配置，表明每个部件挂点的骨骼路径
    /// </summary>
    public sealed class BonesConfig : Singleton<BonesConfig>, IConfig<BonesData>
    {
        public List<BonesData> configs;
        private void EnsureLoadConfig()
        {
            if (configs != null)
            {
                return;
            }
            //todo 加载骨骼配置表,这里可能会写死
            configs = new List<BonesData>();
            AddConfig(Element.MouthAccessory, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02/SK_neck/SK_head/SK_mouth");
            AddConfig(Element.Moustache, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02/SK_neck/SK_head/SK_mouth");
            AddConfig(Element.Wings, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02");
            AddConfig(Element.HeadAccessory_large, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02/SK_neck/SK_head");
            AddConfig(Element.Ankles_L, "root/root01/SK_pine_00/SK_L_high/SK_L_calf/SK_L_foot");
            AddConfig(Element.Wrist_L, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02/SK_L_arm/SK_L_forearm/SK_L_wrist");
            AddConfig(Element.HeadAccessory_large, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02/SK_neck/SK_head/SK_hair");
            AddConfig(Element.Cape, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02/SK_neck/SK_head/SK_hair");
            AddConfig(Element.HeadAccessory_small, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02/SK_neck/SK_head/SK_hair");
            AddConfig(Element.NoseAccessory, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02/SK_neck/SK_head/SK_nose");
            AddConfig(Element.EyeAccessory, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02/SK_neck/SK_head/SK_eye00/SK_eye01");
            AddConfig(Element.Neck, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02/SK_neck");
            AddConfig(Element.Shoulders_L, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02");
            AddConfig(Element.Handheld_L, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02/SK_R_arm/SK_R_forearm/SK_R_wrist/SK_R_hand_00");
            AddConfig(Element.HairExtension, "root/root01/SK_pine_00/SK_pine_03/SK_pine_02/SK_neck/SK_head/SK_hair");
        }

        public void AddConfig(Element element, string bonesPath)
        {
            EnsureLoadConfig();
            RemoveConfig(element);
            BonesData config = Services.Refrence.Require<BonesData>();
            config.element = element;
            config.path = bonesPath;
            configs.Add(config);
        }
        public void RemoveConfig(Element element)
        {
            EnsureLoadConfig();
            BonesData config = GetConfig(element);
            if (config == null)
            {
                return;
            }
            configs.Remove(config);
        }
        public BonesData GetConfig(Element element)
        {
            EnsureLoadConfig();
            BonesData config = null;
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

        public void Initialized(string config)
        {
            if (string.IsNullOrEmpty(config))
            {
                return;
            }
            configs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BonesData>>(config);
        }

        public void Dispose()
        {
        }

        public void AddConfig(BonesData config)
        {
            throw new NotImplementedException();
        }

        public void RemoveConfig(BonesData config)
        {
            throw new NotImplementedException();
        }

        public BonesData GetConfig(Func<BonesData, bool> finder)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }
    }
}