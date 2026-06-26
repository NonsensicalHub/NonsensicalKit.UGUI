using NonsensicalKit.Core;
using UnityEngine;

namespace NonsensicalKit.UGUI.SimpleSignalControl
{
    public class SignalControlActiveGroup : NonsensicalMono
    {
        [SerializeField] private GameObject[] m_target;
        [SerializeField] private string m_signal;
        [SerializeField] private int m_defaultIndex;

        private void Awake()
        {
            Subscribe<int>(m_signal, Switch);

            Switch(m_defaultIndex);
        }

        private void Switch(int index)
        {
            for (int i = 0; i < m_target.Length; i++)
            {
                m_target[i].SetActive(i == index);
            }
        }
    }
}
