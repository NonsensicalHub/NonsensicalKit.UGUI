using System;
using NaughtyAttributes;
using NonsensicalKit.UGUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum CenterOnType
{
    Image,
    Vector2,
}

public class MapController : NonsensicalUI, IDragHandler, IPointerDownHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField, Label("地图前景")] private RectTransform m_maskRectTransform;
    [SerializeField, Label("地图背景")] private RectTransform m_mapRectTransform;

    [Header("交互控制")] [SerializeField, Label("拖拽按钮")]
    private PointerEventData.InputButton m_dragButton = PointerEventData.InputButton.Middle;

    [SerializeField, Label("地图注视居中类型")] private CenterOnType m_centerOnType = CenterOnType.Image;

    // 配置参数
    [Header("拖拽与缩放配置")] [SerializeField, Label("拖拽移动的速度乘数")]
    private float m_dragSensitivity = 1.0f;

    [SerializeField, Label("滚轮缩放的速度")] private float m_zoomSpeed = 0.1f;

    [SerializeField, Label("地图允许的最小缩放比例（相对于原始尺寸）")]
    private float m_minZoomScale = 1.0f;

    [SerializeField, Label("地图允许的最大缩放比例")] private float m_maxZoomScale = 4.0f;


    [Header("角色图标居中")] [SerializeField, Label("地图上的可移动人物图标")]
    private Image m_playerIcon;

    [SerializeField, Label("地图上的可移动人物图标")] private Vector2 m_playerVector2;
    [SerializeField, Label("是否启用自动居中功能")] private bool m_autoCenterOnPlayer = true;

    [SerializeField, Tooltip("用户停止操作后延迟多少秒开始自动居中")]
    private float m_autoCenterDelay = 0.3f;

    [SerializeField, Tooltip("自动居中的缓动速度（值越小越慢）")]
    private float m_centeringSmoothTime = 0.2f;

    private Vector2 _originalMapSize;
    private Vector2 _currentMapSize;
    private Vector2 _maskSize;

    private bool _isDragging = false;
    private float _lastInteractionTime = 0f, _currentScale;

    // 用于 SmoothDamp 的速度缓存
    private Vector2 _centeringVelocity = Vector2.zero;

    public Vector2 PlayerVector2
    {
        get => m_playerVector2;
        set => m_playerVector2 = value;
    }

// 设置玩家图标的位置,当启用自动居中时会以该带来为中心
    public void SetPlayerVector2(Vector2 externalPos)
    {
        // 获取地图的原始尺寸
        Vector2 originalMapSize = m_mapRectTransform.sizeDelta;
        ;
        Vector2 centerBasedPos = new Vector2(
            externalPos.x - originalMapSize.x * 0.5f,
            externalPos.y - originalMapSize.y * 0.5f
        );

        // 将转换后的坐标赋值给 m_playerVector2
        m_playerVector2 = centerBasedPos;
    }

    public float CurrentScale() => _currentScale;

    private bool _isHovering;
    public Action<float> OnZoom;

    protected override void Awake()
    {
        base.Awake();
        m_mapRectTransform ??= GetComponent<RectTransform>();
        m_maskRectTransform ??= m_mapRectTransform.parent.GetComponent<RectTransform>();

        if (m_maskRectTransform == null)
        {
            Debug.LogError("MapController: 父对象必须包含 RectTransform（通常是 Mask 或 Viewport）");
            enabled = false;
            return;
        }

        _originalMapSize = m_mapRectTransform.sizeDelta;
        _currentMapSize = _originalMapSize;
        _maskSize = m_maskRectTransform.sizeDelta;

        ApplyBoundaryClamp(m_mapRectTransform.anchoredPosition);
    }

    private void Update()
    {
        // 滚轮缩放
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        if (scrollDelta != 0 && _isHovering)
        {
            HandleZoom(scrollDelta);
            _lastInteractionTime = Time.time;
            _isDragging = true;
        }

        // 自动居中逻辑（带缓动）
        if (m_autoCenterOnPlayer && (m_playerIcon != null || m_playerVector2 != Vector2.zero))
        {
            bool userIsInteracting = _isDragging || (Time.time - _lastInteractionTime < m_autoCenterDelay);
            if (!userIsInteracting)
            {
                Vector2 playerLocalPos = m_centerOnType == CenterOnType.Image ? m_playerIcon.rectTransform.anchoredPosition : m_playerVector2;
                Vector2 targetPosition = -playerLocalPos;

                // 先对目标位置做边界限制（防止居中导致越界）
                targetPosition = ClampToBoundary(targetPosition);

                // 平滑移动到目标位置
                Vector2 currentPosition = m_mapRectTransform.anchoredPosition;
                Vector2 smoothedPosition = Vector2.SmoothDamp(
                    currentPosition,
                    targetPosition,
                    ref _centeringVelocity,
                    m_centeringSmoothTime
                );

                m_mapRectTransform.anchoredPosition = smoothedPosition;
            }
            else
            {
                _centeringVelocity = Vector2.zero;
            }
        }

        // 如果松开鼠标，结束拖拽状态
        if (_isDragging && !Input.GetMouseButton(0))
        {
            _isDragging = false;
        }
    }

    #region 交互控制

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != m_dragButton) return;
        _isDragging = true;
        _lastInteractionTime = Time.time;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != m_dragButton) return;
        _lastInteractionTime = Time.time;

        Vector2 deltaPixel = eventData.delta * m_dragSensitivity;
        Vector2 newPosition = m_mapRectTransform.anchoredPosition + deltaPixel;
        ApplyBoundaryClamp(newPosition);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovering = false;
    }

    #endregion

    public void HandleZoomWithPercentageValue(float value)
    {
        var newScale = m_minZoomScale + (m_maxZoomScale - m_minZoomScale) * value;
        var scaleChange = newScale / _currentScale;

        Vector2 currentAnchoredPos = m_mapRectTransform.anchoredPosition;
        _currentMapSize = new Vector2(_originalMapSize.x * newScale, _originalMapSize.y * newScale);

        if (m_autoCenterOnPlayer && (m_playerIcon != null || m_playerVector2 != Vector2.zero))
        {
            var newAnchoredPos = currentAnchoredPos +
                                 (m_centerOnType == CenterOnType.Image ? m_playerIcon.rectTransform.anchoredPosition : m_playerVector2)
                                 * (1 - scaleChange);
            m_mapRectTransform.sizeDelta = _currentMapSize;
            ApplyBoundaryClamp(newAnchoredPos);
        }
        else
        {
            m_mapRectTransform.sizeDelta = _currentMapSize;
            ApplyBoundaryClamp(currentAnchoredPos);
        }

        _currentScale = _currentMapSize.x / _originalMapSize.x;
    }

    public void HandleZoom(float delta)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                m_maskRectTransform,
                Input.mousePosition,
                null,
                out var localMousePos))
            return;

        _currentScale = _currentMapSize.x / _originalMapSize.x;
        var newScale = _currentScale + delta * m_zoomSpeed;
        newScale = Mathf.Clamp(newScale, m_minZoomScale, m_maxZoomScale);

        var scaleChange = newScale / _currentScale;

        Vector2 currentAnchoredPos = m_mapRectTransform.anchoredPosition;
        Vector2 mouseRelativeToMap = localMousePos - currentAnchoredPos;
        Vector2 newAnchoredPos = currentAnchoredPos + mouseRelativeToMap * (1 - scaleChange);

        _currentMapSize = new Vector2(_originalMapSize.x * newScale, _originalMapSize.y * newScale);
        m_mapRectTransform.sizeDelta = _currentMapSize;

        ApplyBoundaryClamp(newAnchoredPos);
        _currentScale = _currentMapSize.x / _originalMapSize.x;
        OnZoom?.Invoke(_currentScale);
    }

    // 提取边界限制逻辑为独立方法，供缓动居中时预计算合法目标
    private Vector2 ClampToBoundary(Vector2 targetPosition)
    {
        float horizontalMargin = (_currentMapSize.x - _maskSize.x) * 0.5f;
        float verticalMargin = (_currentMapSize.y - _maskSize.y) * 0.5f;

        float minX = -horizontalMargin;
        float maxX = horizontalMargin;
        float minY = -verticalMargin;
        float maxY = verticalMargin;

        float clampedX = Mathf.Clamp(targetPosition.x, minX, maxX);
        float clampedY = Mathf.Clamp(targetPosition.y, minY, maxY);

        if (_currentMapSize.x <= _maskSize.x) clampedX = 0f;
        if (_currentMapSize.y <= _maskSize.y) clampedY = 0f;

        return new Vector2(clampedX, clampedY);
    }

    // 原有的 ApplyBoundaryClamp 保留，用于拖拽/缩放后的硬限制
    private void ApplyBoundaryClamp(Vector2 targetPosition)
    {
        m_mapRectTransform.anchoredPosition = ClampToBoundary(targetPosition);
    }
}