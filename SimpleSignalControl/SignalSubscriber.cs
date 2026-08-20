using NaughtyAttributes;
using NonsensicalKit.Core;
using UnityEngine;
using UnityEngine.Events;

namespace NonsensicalKit.UGUI.SimpleSignalControl
{
    public class SignalSubscriber : NonsensicalMono
    {
        [SerializeField] private string m_signal;
        [SerializeField] private SignalType m_signalType;

        [ShowIf("m_signalType", SignalType.Empty)] [SerializeField]
        private UnityEvent m_onReceive;

        [ShowIf("m_signalType", SignalType.Bool)] [SerializeField]
        private UnityEvent<bool> m_onReceiveBool;

        [ShowIf("m_signalType", SignalType.Int)] [SerializeField]
        private UnityEvent<int> m_onReceiveInt;

        [ShowIf("m_signalType", SignalType.Float)] [SerializeField]
        private UnityEvent<float> m_onReceiveFloat;

        [ShowIf("m_signalType", SignalType.String)] [SerializeField]
        private UnityEvent<string> m_onReceiveString;

        private void Awake()
        {
            switch (m_signalType)
            {
                default:
                case SignalType.Empty:
                    Subscribe(m_signal, OnReceive);
                    break;
                case SignalType.Bool:
                    Subscribe<bool>(m_signal, OnReceiveBool);
                    break;
                case SignalType.Int:
                    Subscribe<int>(m_signal, OnReceiveInt);
                    break;
                case SignalType.Float:
                    Subscribe<float>(m_signal, OnReceiveFloat);
                    break;
                case SignalType.String:
                    Subscribe<string>(m_signal, OnReceiveString);
                    break;
            }
        }

        private void OnReceive()
        {
            m_onReceive?.Invoke();
        }

        private void OnReceiveBool(bool value)
        {
            m_onReceiveBool?.Invoke(value);
        }

        private void OnReceiveInt(int value)
        {
            m_onReceiveInt?.Invoke(value);
        }

        private void OnReceiveFloat(float value)
        {
            m_onReceiveFloat?.Invoke(value);
        }

        private void OnReceiveString(string value)
        {
            m_onReceiveString?.Invoke(value);
        }
    }
}
