using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI.UIFactory
{
    [RequireComponent(typeof(TooltipRect))]
    public class ToggleTooltip : MonoBehaviour
    {
        [SerializeField] private Toggle m_toggle;

        [TextArea] [SerializeField] private string m_onText = "On Tooltip";
        [TextArea] [SerializeField] private string m_offText = "Off Tooltip";

        private TooltipRect _tooltip;

        private void Start()
        {
            _tooltip = GetComponent<TooltipRect>();

            if (m_toggle == null)
            {
                m_toggle = GetComponent<Toggle>();
                OnValueChanged(m_toggle.isOn);
            }

            if (m_toggle != null)
            {
                m_toggle.onValueChanged.AddListener(OnValueChanged);
            }
        }

        private void OnDestroy()
        {
            if (m_toggle != null)
            {
                m_toggle.onValueChanged.RemoveListener(OnValueChanged);
            }
        }

        private void OnValueChanged(bool value)
        {
            if (value)
            {
                _tooltip.ChangeText(m_onText);
            }
            else
            {
                _tooltip.ChangeText(m_offText);
            }
        }
    }
}
