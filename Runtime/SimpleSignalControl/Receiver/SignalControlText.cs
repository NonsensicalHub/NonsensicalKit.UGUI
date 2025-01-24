using UnityEngine;

namespace NonsensicalKit.UGUI.SimpleSignalControl
{
    public class SignalControlText : TextBase
    {
        [SerializeField] private string m_signal;

        protected override void Awake()
        {
            base.Awake();
            Subscribe<string>(m_signal, OnReceive);
        }

        private void OnReceive(string str)
        {
            TxtSelf.text = str;
        }
    }
}
