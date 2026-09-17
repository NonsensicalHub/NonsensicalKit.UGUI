using NonsensicalKit.Core;
using UnityEngine;

namespace NonsensicalKit.UGUI.SimpleSignalControl
{
    public class BoolControlActive : NonsensicalMono
    {
        [SerializeField] private string m_valueName;
        [SerializeField] private bool m_invert;

        private void Awake()
        {
            var value = IOCC.Get<bool>(m_valueName);
            ChangeActive(value);
            AddListener<bool>(m_valueName, ChangeActive);
        }

        private void ChangeActive(bool value)
        {
            gameObject.SetActive(m_invert ? !value : value);
        }
    }
}
