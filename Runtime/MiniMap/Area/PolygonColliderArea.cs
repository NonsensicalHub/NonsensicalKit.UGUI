using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PolygonColliderArea : AreaInput
{
    [SerializeField] private LayerMask m_layerMask;
    [SerializeField] private RectTransform m_mapRect;

    protected override void OnClick(PointerEventData eventData)
    {
        if (CheckOnArea(eventData, out Collider2D _))
        {
            var worldPosition = CoordinateTransformation.Instance.MapToWorld(eventData.position, eventData.pressEventCamera);
        }
    }

    private bool CheckOnArea(PointerEventData eventData, out Collider2D polygonCollider)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            m_mapRect,
            eventData.position,
            eventData.pressEventCamera,
            out var mouseWorldPos
        );

        var raycastHit = Physics2D.Raycast(
            mouseWorldPos, // 射线原点（鼠标世界坐标）
            Vector2.zero, // 射线方向（无方向，仅用原点检测）
            0f, // 射线长度（0 = 只检测原点位置）
            m_layerMask
        );

        polygonCollider = null;
        if (raycastHit.collider == null) return false;
        polygonCollider = raycastHit.collider;
        return true;
    }
}
