using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using NonsensicalKit.Core;
using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI.SimpleSignalControl
{
    [RequireComponent(typeof(Toggle))]
    public class SendSignalToggle : NonsensicalMono
    {
        [SerializeField] private string m_signal;
        [SerializeField] private string m_valueName;
        [SerializeField] private bool m_sendOnStart;

        private Toggle _togSelf;

        private void Awake()
        {
            _togSelf = GetComponent<Toggle>();
            if (_togSelf != null)
            {
                _togSelf.onValueChanged.AddListener(SendSignal);
            }
        }

        private void Start()
        {
            Publish(m_signal, _togSelf.isOn);
            IOCC.Set(m_valueName,_togSelf.isOn);
        }

        public void SetSignal(string newSignal)
        {
            m_signal = newSignal;
        }

        private void SendSignal(bool value)
        {
            Publish(m_signal, value);
            IOCC.Set(m_valueName,value);
        }
    }
}