using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI
{
    [RequireComponent(typeof(Button), typeof(Image))]
    public class ToggleButton : MonoBehaviour
    {
        [SerializeField] private GameObject m_onState;
        [SerializeField] private GameObject m_offState;
        [SerializeField] private bool m_isOn;
        [SerializeField] private ToggleButtonGroup m_group;
        [SerializeField] private bool m_invokeOnStart = true;

        public bool IsOn
        {
            get
            {
                return m_isOn;
            }
            set
            {
                if (m_isOn != value)
                {
                    if (m_group)
                    {
                        if (m_group.Switch(this, value) == false)
                        {
                            return;
                        }
                    }

                    m_isOn = value;
                    OnValueChanged?.Invoke(m_isOn);
                    UpdateUI();
                }
            }
        }

        public UnityEvent<bool> OnValueChanged;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(Press);
            UpdateUI();
            if (m_group)
            {
                m_group.AddToGroup(this);
            }
        }

        private void Start()
        {
            if (m_invokeOnStart)
            {
                OnValueChanged?.Invoke(IsOn);
            }
        }

        public void Press()
        {
            ToggleState();
        }

        public void ToggleState()
        {
            IsOn = !IsOn;
        }

        public void ToggleState(bool value)
        {
            IsOn = value;
        }

        public void SetState(bool value)
        {
            m_isOn = value;
            UpdateUI();
        }

        private void UpdateUI()
        {
            m_onState.SetActive(IsOn);
            if (m_offState)
            {
                m_offState?.SetActive(!IsOn);
            }
        }

        public void ResetState(bool state)
        {
            m_isOn = state;
            UpdateUI();
        }
    }
}
