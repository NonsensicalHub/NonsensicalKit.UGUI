using TMPro;
using UnityEngine;

namespace NonsensicalKit.UGUI.RestrictedInputField
{
    public class RestrictedInputFieldFloatRange : RestrictedInputFieldBase
    {
        [SerializeField] private float m_minValue;
        [SerializeField] private float m_maxValue;

        protected override void Awake()
        {
            base.Awake();

            if (m_minValue > m_maxValue)
            {
                m_maxValue = m_minValue;
            }
#if TEXTMESHPRO_PRESENT
            _ipf_self.contentType = TMP_InputField.ContentType.DecimalNumber;
#else
            _ipf_self.contentType = UnityEngine.UI.InputField.ContentType.DecimalNumber;
#endif
        }

        protected override void Restrict()
        {
            if (float.TryParse(_ipf_self.text, out var v))
            {
                if (v < m_minValue)
                {
                    _ipf_self.text = m_minValue.ToString();
                }
                else if (v > m_maxValue)
                {
                    _ipf_self.text = m_maxValue.ToString();
                }
            }
            else
            {
                _ipf_self.text = m_defaultValue;
            }
        }
    }
}
