using UnityEngine;
using UnityEngine.EventSystems;

namespace NonsensicalKit.Editor
{
    public class DragSpace : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        [SerializeField] private RectTransform m_controlTarget;

        [SerializeField] private bool m_ensureInBoundary;
        [SerializeField] private RectTransform m_boundary;

        private Vector3 _startOffset;

        private Camera _eventCamera;

        private void Awake()
        {
            if (m_controlTarget == null)
            {
                Debug.LogError("未设置控制对象");
            }
        }

        public void SetBoundary(RectTransform newBoundary)
        {
            m_boundary = newBoundary;
            if (m_boundary != null)
            {
                m_ensureInBoundary = true;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (m_boundary == null)
            {
                m_ensureInBoundary = false;
            }
            _eventCamera = eventData.enterEventCamera;
            Vector3 pos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(m_controlTarget, eventData.position, _eventCamera, out pos);
            _startOffset = m_controlTarget.position - pos;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector3 pos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(m_controlTarget, eventData.position, _eventCamera, out pos);
            if (m_ensureInBoundary)
            {
                m_controlTarget.position = EnsureRectInBounds(m_controlTarget, pos + _startOffset);
            }
            else
            {
                m_controlTarget.position = pos + _startOffset;
            }
        }

        private Vector3 EnsureRectInBounds(RectTransform rect, Vector3 targetPosition)
        {
            Vector3[] minMax = new Vector3[2];

            rect.GetWorldMinMax(ref minMax);
            var newMin = targetPosition - rect.transform.position + minMax[0];
            var newMax = targetPosition - rect.transform.position + minMax[1];
            Vector2 minPoint = m_boundary.transform.InverseTransformPoint(newMin);
            Vector2 maxPoint = m_boundary.transform.InverseTransformPoint(newMax);
            Vector2 canvasMin = m_boundary.rect.min;
            Vector2 canvasMax = m_boundary.rect.max;

            float moveX = 0;
            float moveY = 0;
            Vector2 minOffset = minPoint - canvasMin;
            Vector2 maxOffset = maxPoint - canvasMax;
            if (minOffset.x < 0)
            {
                moveX = -minOffset.x;
            }
            else if (maxOffset.x > 0)
            {
                moveX = -maxOffset.x;
            }
            if (minOffset.y < 0)
            {
                moveY = -minOffset.y;
            }
            else if (maxOffset.y > 0)
            {
                moveY = -maxOffset.y;
            }
            var worldMove = m_boundary.transform.TransformVector(new Vector3(moveX, moveY));
            targetPosition += worldMove;
            return targetPosition;
        }
    }
}
