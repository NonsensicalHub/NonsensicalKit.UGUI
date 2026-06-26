using NonsensicalKit.Core;
using NonsensicalKit.Core.Service;
using NonsensicalKit.Core.Service.Config;
using UnityEngine;

namespace NonsensicalKit.UGUI.UIPosition
{
    /// <summary>
    /// 通过配置文件动态修改ui位置
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UIPositionSetter : NonsensicalMono
    {
        [SerializeField] private string m_configID;

        private RectTransform _rtSelf;
        private float _width;
        private float _height;

        private void Awake()
        {
            _rtSelf = GetComponent<RectTransform>();

            _width = _rtSelf.rect.width;
            _height = _rtSelf.rect.height;
        }

        private void Start()
        {
            ServiceCore.SafeGet<ConfigService>(OnGetManager);
        }

        private void OnGetManager(ConfigService manager)
        {
            if (manager.TryGetConfig<UIPositionData>(out var v))
            {
                for (int i = 0; i < v.ButtonsParameter.Length; i++)
                {
                    if (m_configID == v.ButtonsParameter[i].ID)
                    {
                        ChangePos(v.ButtonsParameter[i]);
                        break;
                    }
                }
            }
        }

        private void ChangePos(UIPostionParameter bp)
        {
            switch (bp.HorizonType)
            {
                case HorizonType.Left:
                    _rtSelf.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, bp.DistanceHorizon, bp.ChangeSize ? bp.Width : _width);
                    break;
                case HorizonType.Right:
                    _rtSelf.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, -bp.DistanceHorizon, bp.ChangeSize ? bp.Width : _width);
                    break;
            }

            switch (bp.VerticalType)
            {
                case VerticalType.Top:
                    _rtSelf.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, -bp.DistanceVertical, bp.ChangeSize ? bp.Height : _height);
                    break;
                case VerticalType.Bottom:
                    _rtSelf.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, bp.DistanceVertical, bp.ChangeSize ? bp.Height : _height);
                    break;
            }
        }
    }
}
