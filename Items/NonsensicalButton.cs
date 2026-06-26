using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI
{
    public class NonsensicalButton : Button
    {
        [SerializeField] private PointerEventData.InputButton m_interactionMouseButton = PointerEventData.InputButton.Left;
        [SerializeField] [Tooltip("最小交互间隔")] private float m_minimumInteractionInterval = 0.1f;
        [SerializeField] [Tooltip("双击判定间隔")] private float m_doubleClickInterval = 0.3f;
        [SerializeField] private UnityEvent m_onDoubleClick;

        public UnityEvent OnDoubleClick => m_onDoubleClick;

        private float _lastPressTime;
        private Coroutine _pendingSingleClick;
        

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != m_interactionMouseButton) return;
            if (Time.unscaledTime - _lastPressTime < m_minimumInteractionInterval) return;

            _lastPressTime = Time.unscaledTime;

            if (_pendingSingleClick != null)
            {
                StopCoroutine(_pendingSingleClick);
                _pendingSingleClick = null;
                InvokeEvent(m_onDoubleClick, "Button.onDoubleClick");
            }
            else
            {
                _pendingSingleClick = StartCoroutine(WaitForSingleClick());
            }
        }

        private IEnumerator WaitForSingleClick()
        {
            yield return new WaitForSecondsRealtime(m_doubleClickInterval);
            _pendingSingleClick = null;
            InvokeEvent(onClick, "Button.onClick");
        }

        private void InvokeEvent(UnityEvent evt, string marker)
        {
            if (!IsActive() || !IsInteractable()) return;
            UISystemProfilerApi.AddMarker(marker, this);
            evt.Invoke();
        }
    }
}
