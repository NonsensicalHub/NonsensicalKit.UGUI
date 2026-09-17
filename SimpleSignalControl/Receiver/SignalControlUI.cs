using NonsensicalKit.UGUI;
using UnityEngine;

namespace NonsensicalKit.UGUI.SimpleSignalControl
{
    public class SignalControlUI : NonsensicalUI
    {
        [SerializeField] private string m_showSignal;
        [SerializeField] private string m_hideSignal;
        [SerializeField] private string m_switchSignal;
        [SerializeField] private string m_changeSignal;
        [SerializeField] private bool m_invert;

        protected override void Awake()
        {
            base.Awake();
            if (!string.IsNullOrEmpty(m_showSignal)) Subscribe(m_showSignal, OpenSelf);
            if (!string.IsNullOrEmpty(m_hideSignal)) Subscribe(m_hideSignal, CloseSelf);
            if (!string.IsNullOrEmpty(m_switchSignal)) Subscribe(m_switchSignal, SwitchSelf);
            if (!string.IsNullOrEmpty(m_changeSignal)) Subscribe<bool>(m_changeSignal, OnChange);
        }

        private void OnChange(bool value)
        {
            ChangeSelf(m_invert ? !value : value);
        }
    }
}
