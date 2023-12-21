using NonsensicalKit.Editor;
using NonsensicalKit.Tools.InputTool;
using NonsensicalKit.Editor.Table;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NonsensicalKit.Editor.VisualLogicGraph
{
    /// <summary>
    /// 黑板管理类
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class VisualLogicBoard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler
    {
        /// <summary>
        /// 点击右键后创建的菜单
        /// </summary>
        private MultilevelMenu _menu;
        /// <summary>
        /// 渲染相机
        /// </summary>
        private Camera _renderCamera;
        /// <summary>
        /// 开始拖拽时鼠标和中心点的偏移量
        /// </summary>
        private Vector3 _startOffset;
        /// <summary>
        /// 自身的RectTransform
        /// </summary>
        private RectTransform _selfRect;
        /// <summary>
        /// 鼠标是否悬浮
        /// </summary>
        private bool _mouseHover;
        /// <summary>
        /// 可视区域宽度的一半，用于计算
        /// </summary>
        private float _halfViewPortWidth;
        /// <summary>
        /// 可视区域高度的一半，用于计算
        /// </summary>
        private float _halfViewPortHeight;
        /// <summary>
        /// 当前缩放比例
        /// </summary>
        private float _crtScale;

        private void Awake()
        {
            _selfRect = GetComponent<RectTransform>();
            _renderCamera = GetComponentInParent<Canvas>().worldCamera;
            _selfRect.anchorMin = Vector3.one * 0.5f;
            _selfRect.anchorMax = Vector3.one * 0.5f;
            _selfRect.pivot = Vector3.one * 0.5f;
            _crtScale = transform.localScale.x;

            IOCC.Set<Vector3>(VisualLogicEnum.CreatPos, transform.position + (Vector3)_selfRect.rect.center);
        }

        private void Start()
        {
            Resize();
        }

        private void Update()
        {
            if (_mouseHover)
            {
                var scroll = InputHub.Instance.CrtZoom;
                if (scroll > 0)
                {
                    scroll = 1;
                }
                else if (scroll < 0)
                {
                    scroll = -1;
                }
                else
                {
                    return;
                }
                _crtScale += scroll * Time.deltaTime * 5;
                if (_crtScale < 0.1f)
                {
                    _crtScale = 0.1f;
                }
                transform.localScale = Vector3.one * _crtScale;
                Resize();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Vector3 pos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(_selfRect, eventData.position, eventData.enterEventCamera, out pos);
            _startOffset = _selfRect.position - pos;
        }

        public void OnDrag(PointerEventData eventData)
        {
            //拖拽是移动此黑板并确保尺寸合格
            Vector3 pos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(_selfRect, eventData.position, eventData.enterEventCamera, out pos);
            _selfRect.position = pos + _startOffset;
            Resize();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _mouseHover = true;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var startPoint = IOCC.Get<VisualLogicPointBase>(VisualLogicEnum.ConnectingPoint);
            if (startPoint != null)
            {
                //点击空白处时清空正在连接的线
                startPoint.StoreFlyLine();
                IOCC.Set<VisualLogicPointBase>(VisualLogicEnum.ConnectingPoint,null);
            }

            if (eventData.button == PointerEventData.InputButton.Right)
            {
                //右键开启新建菜单
                RectTransformUtility.ScreenPointToWorldPointInRectangle(_selfRect, eventData.position, _renderCamera, out var pos) ;
                _menu.transform.position = pos;
                RectTransformUtility.ScreenPointToWorldPointInRectangle(_selfRect, eventData.position, eventData.enterEventCamera, out var pos2);
                IOCC.Set<Vector3>(VisualLogicEnum.CreatPos, pos2);
                _menu.Open();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _mouseHover = false;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="viewportSize"></param>
        /// <param name="menu"></param>
        public void Init(Vector2 viewportSize, MultilevelMenu menu)
        {
            _menu = menu;
            _halfViewPortWidth = viewportSize.x * 0.5f;
            _halfViewPortHeight = viewportSize.y * 0.5f;
        }

        /// <summary>
        /// 根据传入的可视区域尺寸计算尺寸
        /// </summary>
        /// <param name="viewportSize"></param>
        public void ViewportResize(Vector2 viewportSize)
        {
            _halfViewPortWidth = viewportSize.x * 0.5f;
            _halfViewPortHeight = viewportSize.y * 0.5f;
            Resize();
        }

        /// <summary>
        /// 存档时获取尺寸
        /// </summary>
        /// <returns></returns>
        public Vector2 GetSize()
        {
            return _selfRect.rect.size;
        }

        /// <summary>
        /// 读档时设置尺寸
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SetSize(float width, float height)
        {
            _selfRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            _selfRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        /// <summary>
        /// 计算尺寸确保能够覆盖可视区域
        /// </summary>
        private void Resize()
        {
            Vector2 offset = Vector2.zero;

            var pos = _selfRect.anchoredPosition;
            var min = pos + _selfRect.rect.min * transform.localScale;
            var newMin = min;
            newMin.x = Mathf.Min(newMin.x, -_halfViewPortWidth);
            newMin.y = Mathf.Min(newMin.y, -_halfViewPortHeight);
            var minOffset = newMin - min;
            offset += minOffset * transform.localScale;

            var max = pos + _selfRect.rect.max * transform.localScale;
            var newMax = max;
            newMax.x = Mathf.Max(newMax.x, _halfViewPortWidth);
            newMax.y = Mathf.Max(newMax.y, _halfViewPortHeight);
            var maxOffset = newMax - max;
            offset += maxOffset * transform.localScale;

            var newSize = newMax - newMin;
            newSize /= transform.localScale;
            _selfRect.sizeDelta = newSize;
            _selfRect.anchoredPosition += offset;
        }
    }
}
