using NaughtyAttributes;
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

        [ShowIf("m_signalType", SignalType.Bool)][SerializeField] private bool m_boolValue;
        [ShowIf("m_signalType", SignalType.Int)][SerializeField] private int m_intValue;
        [ShowIf("m_signalType", SignalType.Float)][SerializeField] private float m_floatValue;
        [ShowIf("m_signalType", SignalType.String)][SerializeField] private string m_stringValue;

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
