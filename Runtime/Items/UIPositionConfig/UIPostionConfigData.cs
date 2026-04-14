using System;
using NonsensicalKit.Core.Service.Config;
using UnityEngine;

namespace NonsensicalKit.UGUI.UIPosition
{
    [CreateAssetMenu(fileName = "UIPositionConfig", menuName = "ScriptableObjects/UIPositionConfig")]
    public class UIPositionConfig : ConfigObject
    {
        public UIPositionData ConfigData;

        public override ConfigData GetData()
        {
            return ConfigData;
        }

        public override void SetData(ConfigData cd)
        {
            if (CheckType<UIPositionData>(cd))
            {
                ConfigData = cd as UIPositionData;
            }
        }
    }

    [Serializable]
    public class UIPositionData : ConfigData
    {
        public UIPostionParameter[] ButtonsParameter;
    }

    public enum HorizonType
    {
        None,
        Left,
        Right
    }

    public enum VerticalType
    {
        None,
        Top,
        Bottom
    }

    [Serializable]
    public class UIPostionParameter
    {
        public string ID = string.Empty;
        public HorizonType HorizonType = 0;
        public VerticalType VerticalType = 0;
        public float DistanceHorizon = 100;
        public float DistanceVertical = 100;
        public bool ChangeSize = false;
        public float Width = 100;
        public float Height = 100;
    }
}
