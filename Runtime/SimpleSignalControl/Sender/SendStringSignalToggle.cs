using NonsensicalKit.Core;
using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI.SimpleSignalControl
{
    [RequireComponent(typeof(Toggle))]
    public class SendStringSignalToggle : NonsensicalMono
    {
        [SerializeField] private string m_signal;
        [SerializeField] private string m_message;
        private Toggle _togSelf;

        private void Awake()
        {
            _togSelf = GetComponent<Toggle>();
            if (_togSelf != null)
            {
                _togSelf.onValueChanged.AddListener(SendSignal);
            }
        }

        public void SetSignalAndMessage(string newSignal, string newMessage)
        {
            m_signal = newSignal;
            m_message = newMessage;
        }

        public void SetSignal(string newSignal)
        {
            m_signal = newSignal;
        }

        public void SetMessage(string newMessage)
        {
            m_message = newMessage;
        }

        private void SendSignal(bool isOn)
        {
            if (isOn)
            {
                Publish(m_signal, m_message);
                PublishWithID(m_signal, m_message);
            }
        }
    }
}
