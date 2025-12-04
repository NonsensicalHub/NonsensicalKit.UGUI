using NaughtyAttributes;
using NonsensicalKit.Core;
using UnityEngine;

/// <summary>
/// 坐标映射工具
/// 小地图UI坐标与世界坐标相互映射
/// </summary>
public class CoordinateTransformation : NonsensicalMono
{
    [SerializeField] private bool m_listeningMapSizeChange;
    [SerializeField] private RectTransform m_mapRect;
    [SerializeField] private RectTransform m_startRect;
    [SerializeField] private RectTransform m_endRect;
    [SerializeField] private Transform m_startPoint;
    [SerializeField] private Transform m_endPoint;

    private Camera _checkCam;
    private Vector2 _lastSize; // 记录上一帧的大小
    private float _minx, _miny, _maxx, _maxy;
    private Vector3 _startRectTackPos, _endRectTackPos;
    public static CoordinateTransformation Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        _checkCam = Camera.main;
        CalculateMap();
        _lastSize = m_mapRect.rect.size;
        if (m_startPoint || m_endPoint)
        {
            Debug.LogWarning("世界锚点配置为空,请检查 ");
        }
    }

    private void Update()
    {
        if (m_listeningMapSizeChange)
        {
            Vector2 currentSize = m_mapRect.rect.size;

            if (!Mathf.Approximately(currentSize.x, _lastSize.x) ||
                !Mathf.Approximately(currentSize.y, _lastSize.y))
            {
                CalculateMap();
                // 更新记录的大小
                _lastSize = currentSize;
            }
        }
    }

    public Vector3 MapToWorld(Vector2 screenPoint, Camera cam = null)
    {
        if (cam != null)
        {
            _checkCam = cam;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_mapRect, screenPoint, _checkCam, out var pos);
        var pointX = (pos.x - _startRectTackPos.x) / _minx;
        var pointY = (pos.y - _startRectTackPos.y) / _miny;
        return m_startPoint.position + (_maxx * pointX) * Vector3.right + (_maxy * pointY) * Vector3.forward;
    }

    public Vector2 WorldToMap(Vector3 worldPos)
    {
        var posX = (worldPos.x - m_startPoint.position.x) / _maxx;
        var posZ = (worldPos.z - m_startPoint.position.z) / _maxy;
        //return m_startRect.anchoredPosition+ new Vector2(_minx * a, _miny * b);
        return (Vector2)_startRectTackPos + new Vector2(_minx * posX, _miny * posZ);
    }

    public void SetCenterPositionToMap(RectTransform rect, Vector2 targetLocalPos)
    {
        SetCenterPositionToParentRect(rect, targetLocalPos, m_mapRect);
    }

    public void SetCenterPositionToParentRect(RectTransform rect, Vector2 targetLocalPos, RectTransform parent = null)
    {
        if (parent == null)
        {
            rect.localPosition = new Vector3(targetLocalPos.x, targetLocalPos.y, rect.localPosition.z);
            return;
        }

        var parentSize = parent.rect.size;

        // 当前锚点在父节点的实际位置
        Vector2 anchorPos;

        // 如果父节点存在拉伸（anchorMin != anchorMax），取平均
        if (rect.anchorMin != rect.anchorMax)
        {
            var anchorMinPos = new Vector2(
                Mathf.Lerp(0, parentSize.x, rect.anchorMin.x),
                Mathf.Lerp(0, parentSize.y, rect.anchorMin.y)
            );
            var anchorMaxPos = new Vector2(
                Mathf.Lerp(0, parentSize.x, rect.anchorMax.x),
                Mathf.Lerp(0, parentSize.y, rect.anchorMax.y)
            );
            anchorPos = (anchorMinPos + anchorMaxPos) * 0.5f;
        }
        else
        {
            anchorPos = new Vector2(
                Mathf.Lerp(0, parentSize.x, rect.anchorMin.x),
                Mathf.Lerp(0, parentSize.y, rect.anchorMin.y)
            );
        }

        // ⭐ 新的 pivotOffset：不依赖 rect 大小，只保留方向信息
        Vector2 pivotOffset = new Vector2(
            rect.pivot.x - 0.5f,
            rect.pivot.y - 0.5f
        ) * 0f; // == Vector2.zero（你需要的话这里可以调整，不影响大小）

        // 设置 anchoredPosition，排除掉 rect 大小的影响
        rect.anchoredPosition = targetLocalPos - anchorPos + pivotOffset;
    }


    [Button]
    private void CalculateMap()
    {
        //_minx = m_endRect.anchoredPosition.x - m_startRect.anchoredPosition.x;
        // _miny = m_endRect.anchoredPosition.y - m_startRect.anchoredPosition.y;
        _startRectTackPos = GetLocalPositionRelativeToAncestor(m_startRect, m_mapRect);
        _endRectTackPos = GetLocalPositionRelativeToAncestor(m_endRect, m_mapRect);
        _minx = _endRectTackPos.x - _startRectTackPos.x;
        _miny = _endRectTackPos.y - _startRectTackPos.y;
        _maxx = m_endPoint.position.x - m_startPoint.position.x;
        _maxy = m_endPoint.position.z - m_startPoint.position.z;
    }

    /// <summary>
    /// 计算 child 在 ancestor 的局部位置
    /// </summary>
    /// <param name="child">目标 RectTransform</param>
    /// <param name="ancestor">祖先 RectTransform</param>
    private Vector3 GetLocalPositionRelativeToAncestor(RectTransform child, RectTransform ancestor)
    {
        if (child == null || ancestor == null) return Vector3.zero;

        var localPos = Vector3.zero;
        var current = child;

        while (current != null && current != ancestor)
        {
            var parent = current.parent as RectTransform;
            if (parent == null) break;

            var parentSize = parent.rect.size;

            // 当前 RectTransform 的锚点在父节点上的位置

            // 如果父节点是拉伸
            Vector2 anchorPos;
            if (current.anchorMin != current.anchorMax)
            {
                var anchorMinPos = new Vector2(
                    Mathf.Lerp(0, parentSize.x, current.anchorMin.x),
                    Mathf.Lerp(0, parentSize.y, current.anchorMin.y)
                );
                Vector2 anchorMaxPos = new Vector2(
                    Mathf.Lerp(0, parentSize.x, current.anchorMax.x),
                    Mathf.Lerp(0, parentSize.y, current.anchorMax.y)
                );
                anchorPos = (anchorMinPos + anchorMaxPos) * 0.5f;
            }
            else
            {
                anchorPos = new Vector2(
                    Mathf.Lerp(0, parentSize.x, current.anchorMin.x),
                    Mathf.Lerp(0, parentSize.y, current.anchorMin.y)
                );
            }

            // pivot 偏移
            var pivotOffset = new Vector2(
                current.pivot.x * current.rect.width,
                current.pivot.y * current.rect.height
            );

            // 当前在父节点局部位置
            var curLocal = new Vector3(
                anchorPos.x + current.anchoredPosition.x - pivotOffset.x,
                anchorPos.y + current.anchoredPosition.y - pivotOffset.y,
                current.localPosition.z
            );

            // 累加
            localPos += curLocal;
            current = parent;
        }

        return localPos;
    }
}
