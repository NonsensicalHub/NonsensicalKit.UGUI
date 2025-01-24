using UnityEngine;
using UnityEngine.Events;

namespace NonsensicalKit.UGUI.RestrictedInputField
{
    public abstract class RestrictedInputFieldBase : InputFieldBase
    {
        [SerializeField] private UnityEvent<string> m_onEndEdit;

        public UnityEvent<string> OnEndEdit
        {
            get => m_onEndEdit;
            set => m_onEndEdit = value;
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
