using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI
{
    public class NonsensicalButton : Button
    {
        [SerializeField][Tooltip("最小交互间隔")] private float m_minimumInteractionInterval = 0.1f;

        private float _lastPressTime;

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (Time.unscaledTime - _lastPressTime < m_minimumInteractionInterval)
            {
                Press();
                _lastPressTime = Time.unscaledTime;
            }
        }

        private void Press()
        {
            if (!IsActive() || !IsInteractable())
                return;

            UISystemProfilerApi.AddMarker("Button.onClick", this);
            onClick.Invoke();
        }
    }
}
