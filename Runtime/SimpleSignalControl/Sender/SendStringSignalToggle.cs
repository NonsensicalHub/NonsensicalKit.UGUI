using NonsensicalKit.Editor;
using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.Editor.SimpleSignalControl
{
    [RequireComponent(typeof(Toggle))]
    public class SendStringSignalToggle : NonsensicalMono
    {
        [SerializeField] private string m_signal;
        [SerializeField] private string m_message;
        private Toggle _tog_Self;

        private void Awake()
        {
            _tog_Self = GetComponent<Toggle>();
            if (_tog_Self != null)
            {
                _tog_Self.onValueChanged.AddListener(SendSignal);
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
