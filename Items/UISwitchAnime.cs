using NonsensicalKit.Core;
using UnityEngine;

namespace NonsensicalKit.UGUI
{
    public class UISwitchAnime : NonsensicalMono
    {
        [SerializeField] private string m_signal;
        [SerializeField] private Transform m_control;
        [SerializeField] private Transform m_openPos;
        [SerializeField] private Transform m_closePos;
        [SerializeField] private bool m_initState;

        private bool _isOpen;

        private void Awake()
        {
            Subscribe<bool>(m_signal, OnSwitch);
            _isOpen = m_initState;
        }

        private void Update()
        {
            Vector3 targetPos = _isOpen ? m_openPos.position : m_closePos.position;
            m_control.position = Vector3.Lerp(m_control.position, targetPos, 0.1f);
            if (Vector3.Distance(m_control.position, targetPos) < 1)
            {
                m_control.position = targetPos;
                enabled = false;
            }
        }

        private void OnSwitch(bool value)
        {
            _isOpen = value;
            enabled = true;
        }
    }
}
