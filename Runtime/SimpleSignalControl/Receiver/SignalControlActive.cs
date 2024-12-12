using NonsensicalKit.Core;
using UnityEngine;

namespace NonsensicalKit.UGUI.SimpleSignalControl
{
    /// <summary>
    /// 信号控制激活
    /// TODO:修改编辑器显示，当不使用双信号控制时隐藏signal2，且修改显示的文字（signal=>显示信号）
    /// </summary>
    public class SignalControlActive : NonsensicalMono
    {
        [SerializeField] private GameObject m_target;
        [SerializeField] private bool m_twoSignalControl ;
        [SerializeField] private string m_signal;
        [SerializeField] private string m_signal2;
        [SerializeField] private bool m_defaultState = true;

        private void Reset()
        {
            if (m_target == null)
            {
                m_target = gameObject;
            }
        }

        private void Awake()
        {
            if (m_twoSignalControl)
            {
                Subscribe(m_signal, Show);
                Subscribe(m_signal2, Hide);
            }
            else
            {
                Subscribe<bool>(m_signal, Switch);
                Subscribe(m_signal, Switch);
            }

            Switch(m_defaultState);
        }

        private void Switch()
        {
            m_target.SetActive(!m_target.activeSelf);
        }

        private void Switch(bool state)
        {
            m_target.SetActive(state);
        }

        private void Show()
        {
            m_target.SetActive(true);
        }

        private void Hide()
        {
            m_target.SetActive(false);
        }
    }
}
