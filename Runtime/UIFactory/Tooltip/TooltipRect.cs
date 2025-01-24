using NonsensicalKit.Core.Log;
using NonsensicalKit.Core.Service;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NonsensicalKit.UGUI.UIFactory
{
    public class TooltipRect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private RectTransform.Edge m_location = RectTransform.Edge.Right;

        [SerializeField] private RectTransform m_targetRect;

        [TextArea] [SerializeField] protected string m_text = "Tooltip";

        protected Tooltip Tooltip;

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
            if (Tooltip != null)
            {
                Tooltip.Close();
            }
        }

        public void ChangeText(string text)
        {
            m_text = text;
            if (Tooltip != null)
            {
                Tooltip.ChangeText(text);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Tooltip != null)
            {
                Tooltip.Show();
            }
            else
            {
                ShowToolTip();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (Tooltip != null)
            {
                Tooltip.Hide();
            }
        }

        protected void ShowToolTip()
        {
            TooltipInfo info = new TooltipInfo(m_location, m_text);
            UIFactory factory = ServiceCore.Get<UIFactory>();
            if (factory != null)
            {
                Tooltip = factory.OpenUI(nameof(UGUI.UIFactory.Tooltip), info).GetComponent<Tooltip>();
                Tooltip.Show();
            }
            else
            {
                LogCore.Warning("未找到UIFactory");
            }
        }
    }
}
