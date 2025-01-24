using NaughtyAttributes;
using NonsensicalKit.Core.Log;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NonsensicalKit.UGUI
{
    public class DragSpace : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        /// <summary>
        /// 需要移动的对象
        /// </summary>
        [SerializeField] private RectTransform m_controlRect;

        /// <summary>
        /// 是否限定范围
        /// </summary>
        [SerializeField] private bool m_ensureInBoundary;

        /// <summary>
        /// 限定在范围内的对象
        /// </summary>
        [ShowIf("m_ensureInBoundary")] [SerializeField]
        private RectTransform m_checkRect;

        /// <summary>
        /// 确定范围的对象
        /// </summary>
        [ShowIf("m_ensureInBoundary")] [SerializeField]
        private RectTransform m_boundaryRect;

        /// <summary>
        /// 开始拖拽时鼠标和拖拽对象位置点的偏移，移动时要维持此偏移不变
        /// </summary>
        private Vector3 _startOffset;

        /// <summary>
        /// 触发拖拽的事件摄像机，为了防止多摄像机渲染时的参考系变更，故进行存储
        /// </summary>
        private Camera _eventCamera;

        private void Awake()
        {
            if (m_controlRect == null)
            {
                LogCore.Warning("未设置控制对象", gameObject);
                enabled = false;
            }

            if (m_checkRect == null)
            {
                m_checkRect = m_controlRect;
            }
        }

        public void SetBoundary(RectTransform newBoundary)
        {
            m_boundaryRect = newBoundary;
            if (m_boundaryRect != null)
            {
                m_ensureInBoundary = true;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (m_boundaryRect == null)
            {
                m_ensureInBoundary = false;
            }

            _eventCamera = eventData.enterEventCamera;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(m_controlRect, eventData.position, _eventCamera, out var pos);
            _startOffset = m_controlRect.position - pos;
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(m_controlRect, eventData.position, _eventCamera, out var pos);
            if (m_ensureInBoundary)
            {
                m_controlRect.position = EnsureRectInBounds(m_controlRect, pos + _startOffset, m_checkRect, m_boundaryRect);
            }
            else
            {
                m_controlRect.position = pos + _startOffset;
            }
        }

        /// <summary>
        /// 在controlRect试图移动到targetPosition时，确保移动后的checkRect仍然被boundaryRect包围
        /// </summary>
        /// <param name="controlRect"></param>
        /// <param name="targetPosition"></param>
        /// <param name="checkRect"></param>
        /// <param name="boundaryRect"></param>
        /// <returns></returns>
        private Vector3 EnsureRectInBounds(RectTransform controlRect, Vector3 targetPosition, RectTransform checkRect, RectTransform boundaryRect)
        {
            Vector3[] minMax = new Vector3[2];

            checkRect.GetWorldMinMax(ref minMax);
            var newMin = targetPosition - controlRect.transform.position + minMax[0];
            var newMax = targetPosition - controlRect.transform.position + minMax[1];
            Vector2 localMin = boundaryRect.transform.InverseTransformPoint(newMin);
            Vector2 localMax = boundaryRect.transform.InverseTransformPoint(newMax);
            Vector2 boundaryMin = boundaryRect.rect.min;
            Vector2 boundaryMax = boundaryRect.rect.max;

            Vector2 move = Vector2.zero;
            Vector2 minOffset = localMin - boundaryMin;
            Vector2 maxOffset = localMax - boundaryMax;
            if (minOffset.x < 0)
            {
                move.x = -minOffset.x;
            }
            else if (maxOffset.x > 0)
            {
                move.x = -maxOffset.x;
            }

            if (minOffset.y < 0)
            {
                move.y = -minOffset.y;
            }
            else if (maxOffset.y > 0)
            {
                move.y = -maxOffset.y;
            }

            var worldMove = boundaryRect.transform.TransformVector(move);
            targetPosition += worldMove;
            return targetPosition;
        }
    }
}
