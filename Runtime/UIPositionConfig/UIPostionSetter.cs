using NonsensicalKit.Editor;
using NonsensicalKit.Editor.Service;
using NonsensicalKit.Editor.Service.Config;
using UnityEngine;

namespace NonsensicalKit.Editor.UIPosition
{
    /// <summary>
    /// 通过配置文件动态修改ui位置
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UIPostionSetter : NonsensicalMono
    {
        [SerializeField] private string m_configID;

        private RectTransform _rt_self;
        private float _width;
        private float _height;

        private void Awake()
        {
            _rt_self = GetComponent<RectTransform>();

            _width = _rt_self.rect.width;
            _height = _rt_self.rect.height;
        }

        private void Start()
        {
            ServiceCore.SafeGet<ConfigService>(OnGetManager);
        }

        private void OnGetManager(ConfigService manager)
        {
            ConfigService configmanager = manager as ConfigService;
            if (configmanager.TryGetConfig<UIPositionData>(out var v))
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
                    _rt_self.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, bp.DistanceHorizon, bp.ChangeSize ? bp.Width : _width);
                    break;
                case HorizonType.Right:
                    _rt_self.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, -bp.DistanceHorizon, bp.ChangeSize ? bp.Width : _width);
                    break;
            }

            switch (bp.VerticalType)
            {
                case VerticalType.Top:
                    _rt_self.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, -bp.DistanceVertical, bp.ChangeSize ? bp.Height : _height);
                    break;
                case VerticalType.Bottom:
                    _rt_self.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, bp.DistanceVertical, bp.ChangeSize ? bp.Height : _height);
                    break;
            }
        }
    }
}
