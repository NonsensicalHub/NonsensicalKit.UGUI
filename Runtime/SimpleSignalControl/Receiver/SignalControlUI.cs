using UnityEngine;

namespace NonsensicalKit.UGUI.SimpleSignalControl
{
    public class SignalControlUI : NonsensicalUI
    {
        [SerializeField] private string m_showSignal;
        [SerializeField] private string m_hideSignal;
        [SerializeField] private string m_switchSignal;
        [SerializeField] private string m_changeSignal;

        protected override void Awake()
        {
            base.Awake();
            if (!string.IsNullOrEmpty(m_showSignal)) Subscribe(m_showSignal, OpenSelf);
            if (!string.IsNullOrEmpty(m_hideSignal)) Subscribe(m_hideSignal, CloseSelf);
            if (!string.IsNullOrEmpty(m_switchSignal)) Subscribe(m_switchSignal, SwitchSelf);
            if (!string.IsNullOrEmpty(m_changeSignal)) Subscribe<bool>(m_changeSignal, ChangeSelf);
        }
    }
}
