using NonsensicalKit.Editor.Log;
using NonsensicalKit.Editor.Service;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NonsensicalKit.Editor.UIFactory
{
    public class TooltipRect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private RectTransform.Edge m_location = RectTransform.Edge.Right;

        [SerializeField] private RectTransform m_targetRect;

        [TextArea][SerializeField] protected string m_text = "Tooltip";

        protected Tooltip _tooltip;

        private void Awake()
        {
            if (m_targetRect == null)
            {
                m_targetRect = GetComponent<RectTransform>();
            }
            if (m_targetRect == null)
            {
                LogCore.Warning("m_targetRect无可用对象");
            }
        }

        private void OnDisable()
        {
            if (_tooltip != null)
            {
                _tooltip.Close();
            }
        }

        public void ChangeText(string text)
        {
            m_text = text;
            if (_tooltip != null)
            {
                _tooltip.ChangeText(text);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_tooltip != null)
            {
                _tooltip.Show();
            }
            else
            {
                ShowToolTip();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_tooltip != null)
            {
                _tooltip.Hide();
            }
        }

        protected void ShowToolTip()
        {
            TooltipInfo info = new TooltipInfo(m_location, m_text);
            UIFactory factory = ServiceCore.Get<UIFactory>();
            if (factory != null)
            {
                _tooltip = factory.OpenUI(nameof(Tooltip), info).GetComponent<Tooltip>();
                _tooltip.Show();
            }
            else
            {
                LogCore.Warning("未找到UIFactory");
            }
        }
    }
}
