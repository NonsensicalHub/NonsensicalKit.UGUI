using NonsensicalKit.Core;
using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI.SimpleSignalControl
{
    /// <summary>
    /// 发送信号按钮
    /// TODO：根据信号类型隐藏未选择部分
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class SendSignalButton : NonsensicalMono
    {
        [SerializeField] private string m_signal;
        [SerializeField] private SignalType m_signalType;

        [SerializeField] private bool m_boolValue;
        [SerializeField] private int m_intValue;
        [SerializeField] private float m_floatValue;
        [SerializeField] private string m_stringValue;

        private Button _btn_Self;

        private void Awake()
        {
            _btn_Self = GetComponent<Button>();
            if (_btn_Self != null)
            {
                _btn_Self.onClick.AddListener(SendSignal);
            }
        }

        public void SetSignal(string newSignal)
        {
            m_signal = newSignal;
        }

        private void SendSignal()
        {
            switch (m_signalType)
            {
                default:
                case SignalType.Empty:
                    Publish(m_signal);
                    break;
                case SignalType.Bool:
                    Publish(m_signal, m_boolValue);
                    break;
                case SignalType.Int:
                    Publish(m_signal, m_intValue);
                    break;
                case SignalType.Float:
                    Publish(m_signal, m_boolValue);
                    break;
                case SignalType.String:
                    Publish(m_signal, m_stringValue);
                    break;
            }
        }
    }
}
