using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace NonsensicalKit.UGUI.RestrictedInputField
{
    public abstract class RestrictedInputFieldBase : InputFieldBase
    {
        [Serializable]
        public class EndEditEvent : UnityEvent<string> { }
        [FormerlySerializedAs("onEndEdit")]
        [SerializeField]
        private EndEditEvent m_OnEndEdit = new EndEditEvent();
        public EndEditEvent OnEndEdit
        {
            get { return m_OnEndEdit; }
            set { m_OnEndEdit = value; }
        }

        protected override void Awake()
        {
            base.Awake();
            _ipf_self.text = m_defaultValue;
            _ipf_self.onEndEdit.AddListener(OnInputFieldEndEdit);
        }

        private void OnInputFieldEndEdit(string value)
        {
            Restrict();
            OnEndEdit?.Invoke(_ipf_self.text);
        }

        protected abstract void Restrict();
    }
}
